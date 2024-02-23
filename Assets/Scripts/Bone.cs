using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
    public Bone childBone = null;
    public Bone parentBone = null;

    // Start is called before the first frame update
    void Start()
    {
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
