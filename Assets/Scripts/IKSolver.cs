using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKSolver : MonoBehaviour
{
    Bone lastBone;
    Bone firstBone;

    [SerializeField]
    Transform target;

    // Start is called before the first frame update
    void Start()
    {
        firstBone = GetComponentInChildren<Bone>();
        Bone currBone = firstBone;
        while (currBone != null)
        {
            if (currBone.childBone != null)
            {
                currBone = currBone.childBone;
                continue;
            }

            lastBone = currBone; 
            break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //lastBone.transform.LookAt(target.transform);
        IKCCD();   
    }


    void IKCCD()
    {
        Bone cur = lastBone;

        while (cur != null)
        {
            // get 3 points
            Vector3 lastPos = new Vector3(lastBone.transform.position.x, lastBone.transform.position.y, lastBone.transform.position.z);
            Vector3 targetPos = new Vector3(target.position.x, target.position.y, target.position.z);
            Vector3 currentPos = new Vector3(cur.transform.position.x, cur.transform.position.y, cur.transform.position.z);

            // compute cos theta and rotation axis r
            Vector3 toEnd = (lastPos - currentPos).normalized;
            Vector3 toTarget = (targetPos - currentPos).normalized;
            float rotationAngle = Vector3.Dot(toEnd, toTarget);
            Vector3 axis = Vector3.Cross(toEnd, toTarget);

            // compute quaternion
            Quaternion q = new Quaternion(axis.x * 0.5f, axis.y * 0.5f, axis.z * 0.5f, (rotationAngle + 1.0f) * 0.5f); //Quaternion.AngleAxis(rotationAngle, axis);

            // update cur joint local rotation with quaternion
            Quaternion old_gq = cur.transform.rotation;
            Quaternion new_gq = q * old_gq;
            cur.transform.rotation = new_gq;

            cur = cur.parentBone;
        }
    }
}
