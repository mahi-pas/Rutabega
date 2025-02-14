using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float MovePower;
    public float MaxAngularSpeed;
    public float HookRetractSpeed;
    public float HookRange;

    bool isGrappled = false;
    float timeTillGrapple = 0;
    float hookAnchorAngle;
    RopeRenderer ropeMaker;
    Rigidbody2D pBody;
    DistanceJoint2D hookEnforcer;
    GameObject hookPoint;
    public GameObject hookPointPrefab;

    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        ropeMaker = GetComponent<RopeRenderer>();
        pBody = GetComponent<Rigidbody2D>();
        hookEnforcer = GetComponent<DistanceJoint2D>();
        hookEnforcer.enabled = false;

        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float joystick = Input.GetAxis("Horizontal");
        if (!isGrappled)
        {
            pBody.AddTorque(-joystick * Time.deltaTime * MovePower, ForceMode2D.Impulse);
            if (pBody.angularVelocity > MaxAngularSpeed)
                pBody.angularVelocity = MaxAngularSpeed;
            else if (pBody.angularVelocity < -MaxAngularSpeed)
                pBody.angularVelocity = -MaxAngularSpeed;
        }
        else
        {
            if (hookEnforcer.distance > 1)
                hookEnforcer.distance -= HookRetractSpeed * Time.deltaTime;
            Vector3 vectorToHook = hookPoint.transform.position - transform.position;
            float angleToHook = Mathf.Atan(vectorToHook.y / vectorToHook.x) + Mathf.PI / 2;
            if (vectorToHook.x > 0)
                angleToHook -= Mathf.PI;
            transform.eulerAngles = Vector3.forward * (angleToHook - hookAnchorAngle) * 180 / Mathf.PI;
        }

        if (timeTillGrapple > 0)
        {
            timeTillGrapple -= Time.deltaTime;
            if (timeTillGrapple <= 0)
            { // This is when the hook lands
                isGrappled = true;
                hookEnforcer.enabled = true;

                Vector3 castDirection = hookPoint.transform.position - transform.position;
                hookEnforcer.distance = castDirection.magnitude;
                hookAnchorAngle = Mathf.Atan(castDirection.y / castDirection.x) + Mathf.PI / 2;
                if (castDirection.x > 0)
                    hookAnchorAngle -= Mathf.PI;
                hookAnchorAngle -= transform.eulerAngles.z * Mathf.PI / 180;
                pBody.angularVelocity = 0;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            //Find the hook point
            Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePoint.z = 0;
            Vector3 castDirection = Vector3.Normalize(mousePoint - transform.position);
            RaycastHit2D rayHit = Physics2D.Raycast(transform.position + castDirection * 0.5f, castDirection, HookRange);
            if (rayHit.collider != null)
            {
                //Attach the hook
                hookPoint = Instantiate(hookPointPrefab);
                hookPoint.transform.position = rayHit.point;
                hookPoint.transform.parent = rayHit.transform;
                hookEnforcer.connectedBody = hookPoint.GetComponent<Rigidbody2D>();
                ropeMaker.Grapple(hookPoint.transform);
                timeTillGrapple = ropeMaker.TravelTime;

                //animation
                anim.SetTrigger("Grapple");
            }
        }
        if (Input.GetMouseButtonUp(0))
        { //Remove the hook
            RemoveHook();
        }
    }

    public void RemoveHook(){
        hookEnforcer.enabled = false;
        Destroy(hookPoint);
        isGrappled = false;
        timeTillGrapple = 0;
        ropeMaker.Unhook();
    }
}
