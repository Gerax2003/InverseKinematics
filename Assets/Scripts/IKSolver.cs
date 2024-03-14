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
        //CCDNoConstraints(); 
        CCDConstraints();
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

            // Update bone rotation to point end to target
            Quaternion old_gq = bone.transform.rotation;
            Quaternion new_gq = q * old_gq;
            bone.transform.rotation = new_gq;

            // Go up in chain
            bone = bone.parentBone;
        }
    }

    void CCDConstraints()
    {
        Bone bone = lastBone;

        while (bone != null)
        {
            // Get rotation to from bone->end to bone->target, points end to target
            Vector3 toEnd = (lastBone.transform.position - bone.transform.position).normalized;
            Vector3 toTarget = (target.position - bone.transform.position).normalized;
            Quaternion q = Quaternion.FromToRotation(toEnd, toTarget);

            // Update bone rotation to point end to target
            Quaternion old_gq = bone.transform.rotation;
            Quaternion new_gq = q * old_gq;
            bone.transform.rotation = new_gq;

            // Force axis constraint
            if (bone.jointAxis != Vector3.zero)
            {
                Vector3 curHingeAxis = bone.transform.rotation * bone.jointAxis;
                Vector3 hingeAxis = Vector3.zero;
                
                if (bone.parentBone != null)
                     hingeAxis = bone.parentBone.transform.rotation * bone.jointAxis;
                else
                    hingeAxis = Quaternion.identity * bone.jointAxis;

                bone.transform.rotation = Quaternion.FromToRotation(curHingeAxis, hingeAxis) * bone.transform.rotation;

                //Vector3 euler = ClampEuler(bone.transform.eulerAngles, bone.jointMinLimits, bone.jointMaxLimits);
                //bone.transform.rotation = Quaternion.Euler(euler);

                Quaternion rot = bone.transform.rotation;
                Quaternion parentRot = bone.parentBone != null ? bone.parentBone.transform.rotation : Quaternion.identity;
                // Limit rotation
                Vector3 perpendicular = Perpendicular(bone.jointAxis);
                bone.transform.rotation = Quaternion.FromToRotation(rot * perpendicular, 
                    ConstrainToNormal(rot * perpendicular, parentRot * perpendicular, bone.jointMaxLimits)) * rot;
            }

            // Go up in chain
            bone = bone.parentBone;
        }
    }

    public Vector3 Perpendicular(Vector3 vec)
    {
        return Mathf.Abs(vec.x) > Mathf.Abs(vec.z) ? new Vector3(-vec.y, vec.x, 0f)
                                                   : new Vector3(0f, -vec.z, vec.y);
    }

    public Vector3 ConstrainToNormal(Vector3 direction, Vector3 normalDirection, float maxAngle)
    {
        if (maxAngle <= 0f) return normalDirection.normalized * direction.magnitude; if (maxAngle >= 180f) return direction;
        float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(direction.normalized, normalDirection.normalized), -1f, 1f)) * Mathf.Rad2Deg;
        return Vector3.Slerp(direction.normalized, normalDirection.normalized, (angle - maxAngle) / angle) * direction.magnitude;
    }

    public float ClampAngle(float angle, float min, float max)
    {
        float start = (min + max) * 0.5f - 180;
        float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
        return Mathf.Clamp(angle, min + floor, max + floor);
    }

    public Vector3 ClampEuler(Vector3 angle, float minAngle, float maxAngle)
    {
        Vector3 outAngle = angle;

        outAngle.x = ClampAngle(outAngle.x, minAngle, maxAngle);
        outAngle.y = ClampAngle(outAngle.y, minAngle, maxAngle);
        outAngle.z = ClampAngle(outAngle.z, minAngle, maxAngle);
        
        return outAngle;
    }
}
