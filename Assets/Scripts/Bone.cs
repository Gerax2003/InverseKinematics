using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
    public Bone childBone = null;
    public Bone parentBone = null;

    public Vector3 jointAxis = Vector3.zero;
    public float jointMinLimits = -360f;
    public float jointMaxLimits = 360f;

    // Start is called before the first frame update
    void Awake()
    {
        if (jointAxis != Vector3.zero) 
            jointAxis.Normalize();

        parentBone = transform.parent.GetComponent<Bone>();

        Bone[] children = GetComponentsInChildren<Bone>();
    
        foreach (Bone b in children) 
        {
            if (b != this && b.transform.IsChildOf(transform))
            {
                childBone = b;
                return;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
