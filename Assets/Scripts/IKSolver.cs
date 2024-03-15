using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKSolver : MonoBehaviour
{
    [SerializeField]
    bool stableAngleLimit = false;

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
                Vector3 currAxis = bone.transform.rotation * bone.jointAxis;
                Vector3 currParentAxis = Vector3.zero;
                
                if (bone.parentBone != null)
                     currParentAxis = bone.parentBone.transform.rotation * bone.jointAxis;
                else
                    currParentAxis = Quaternion.identity * bone.jointAxis;

                bone.transform.rotation = Quaternion.FromToRotation(currAxis, currParentAxis) * bone.transform.rotation;

                // Bloated implementation but incredibly stable, used for benchmark performance and quality of other implementations, taken from:
                // https://github.com/zalo/MathUtilities/blob/master/Assets/Constraints/Constraints.cs 
                // https://github.com/zalo/MathUtilities/blob/master/Assets/IK/CCDIK/CCDIKJoint.cs
                if (stableAngleLimit)
                {
                    Quaternion rot = bone.transform.rotation;
                    Quaternion parentRot = bone.parentBone != null ? bone.parentBone.transform.rotation : Quaternion.identity;
                    // basically the joint's forwards?
                    Vector3 perpendicular = Perpendicular(bone.jointAxis);
                    // Find the vector that represents the clamped rotation(?) using current direction and parent's direction (????)
                    Vector3 constraintVector = ConstrainToNormal(rot * perpendicular, parentRot * perpendicular, bone.jointMaxLimits);
                    // Find compensating rotation and apply it to unclamped rotation
                    bone.transform.rotation = Quaternion.FromToRotation(rot * perpendicular, constraintVector) * rot;
                }
                else // just clamp the rotation quaternion's angle around its own axis
                // Known instabilities: stutters on spawn/fast movement, inability to solve then teleportation and reverse joints if range does not cross 0
                {
                    Quaternion clampRotation = ClampRotation(bone.transform.localRotation, bone.jointMinLimits, bone.jointMaxLimits);
                    bone.transform.localRotation = clampRotation;
                }
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

        float dot = Mathf.Clamp(Vector3.Dot(direction.normalized, normalDirection.normalized), -1f, 1f);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        // direction represents the vector without a rotation, normalDirection the maximum and "(angle - maxAngle) / angle" calculates the clamp??
        Vector3 ret = Vector3.Slerp(direction.normalized, normalDirection.normalized, (angle - maxAngle) / angle);
        return ret * direction.magnitude;
    }

    Quaternion ClampRotation(Quaternion rotation, float minAngle, float maxAngle)
    {
        float angle;
        Vector3 axis;

        rotation.ToAngleAxis(out angle, out axis);

        // clamp range is -180.180, unity angle range is 0.360 so we convert ranges
        angle = Angle360To180(angle);
        angle = Mathf.Clamp(angle, minAngle, maxAngle);
        angle = Angle180To360(angle);

        Quaternion ret = Quaternion.AngleAxis(angle, axis);

        return ret;
    }

    // Change angle from 0.360 range to -180.180 range
    float Angle360To180(float angle)
    {
        if (angle <= 180f)
            return angle;

        return -(360f - angle);
    }
    // Change angle from -180.180 range to 0.360 range
    float Angle180To360(float angle)
    {
        if (angle >= 0f)
            return angle;

        return 360f + angle;
    }
}
