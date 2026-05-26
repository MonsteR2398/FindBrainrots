using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothRotation : MonoBehaviour
{
    public Vector3 rotationAxis = new Vector3(0, 1, 0);
    public float rotationSpeed = 50f;

    [Header("Vertical PingPong")]
    public bool enableVerticalFloat = false;
    public float verticalAmplitude = 1f;
    public float verticalSpeed = 1f;

    private float startY;

    void Start()
    {
        startY = transform.localPosition.y;
    }

    void Update()
    {
        RotateContinuously();
        FloatVertically();
    }

    void RotateContinuously()
    {
        float step = (rotationSpeed * 10) * Time.deltaTime;
        Quaternion rotation = Quaternion.Euler(rotationAxis * step);

        transform.rotation *= rotation;
    }

    void FloatVertically()
    {
        if (!enableVerticalFloat) return;

        float offset = Mathf.Sin(Time.time * verticalSpeed) * verticalAmplitude;
        Vector3 pos = transform.localPosition;
        pos.y = startY + offset;
        transform.localPosition = pos;
    }
}
