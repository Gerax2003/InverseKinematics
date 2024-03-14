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
        CCDNoConstraints();   
    }

    void CCDNoConstraints()
    {
        Bone bone = lastBone;

        while (bone != null)
        {
            // Get rotation to from bone->end to bone->target, points end to target
            Vector3 toEnd = (lastBone.transform.position - bone.transform.position).normalized;
            Vector3 toTarget = (target.position - bone.transform.position).normalized;
            Quaternion q = Quaternion.FromToRotation(toEnd, toTarget);

            // Update cur bone local rotation with quaternion
            Quaternion old_gq = bone.transform.rotation;
            Quaternion new_gq = q * old_gq;
            bone.transform.rotation = new_gq;

            // Go up in chain
            bone = bone.parentBone;
        }
    }

    void CCDAxisConstraints()
    {

    }
}
