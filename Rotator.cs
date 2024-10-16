using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] 
    private Vector3 rotateForce = new Vector3(0, 1, 0);
    [SerializeField]
    private float rotateSpeed = 100;

    void Update()
    {
        // rotate this object each frame
        this.transform.Rotate(rotateForce * rotateSpeed * Time.deltaTime);
    }
}
