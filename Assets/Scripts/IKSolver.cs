using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKSolver : MonoBehaviour
{
    [SerializeField]
    Bone lastBone;
    [SerializeField]
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
        IKCCDNoConstraints();   
    }

    void IKCCDNoConstraints()
    {
        Bone joint = lastBone;

        while (joint != null)
        {
            // Get rotation to from end effector to target
            Vector3 toEnd = (lastBone.transform.position - joint.transform.position).normalized;
            Vector3 toTarget = (target.position - joint.transform.position).normalized;
            Quaternion q = Quaternion.FromToRotation(toEnd, toTarget);

            // Update cur joint local rotation with quaternion
            Quaternion old_gq = joint.transform.rotation;
            Quaternion new_gq = q * old_gq;
            joint.transform.rotation = new_gq;

            // Go up in chain
            joint = joint.parentBone;
        }
    }
}
