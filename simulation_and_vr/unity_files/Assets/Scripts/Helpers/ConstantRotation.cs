using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Transform))]
public class ConstantRotation : MonoBehaviour
{
    [Tooltip("The rotation axis to rotate around.")]
    public Vector3 Axis = Vector3.up;

    [Tooltip("The speed to rotate.")] 
    public float AnglePerSecond = 90f;

    [Tooltip("The rotation space in which to rotate.")]
    public Space Space = Space.Self;

    private Transform cachedTransform;

    void Start()
    {
        cachedTransform = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        this.cachedTransform.Rotate(Axis, AnglePerSecond * Time.deltaTime, Space);
    }
}
