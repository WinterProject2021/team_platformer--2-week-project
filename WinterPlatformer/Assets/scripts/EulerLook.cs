using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EulerLook : MonoBehaviour
{
    [SerializeField] private Vector3 euler;
    [SerializeField] private float sensitivity = 10;

    // Start is called before the first frame update
    void Start()
    {
        euler = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        Vector2 input = new Vector2(
            -Input.GetAxisRaw("Mouse Y"),
            Input.GetAxisRaw("Mouse X")
        );

        euler[0] += input[0] * sensitivity * dt;
        euler[1] += input[1] * sensitivity * dt;

        euler[0] = (euler[0] < -90F) ? -90F : euler[0];
        euler[0] = (euler[0] > 90F) ? 90F : euler[0];

        transform.position = - transform.forward * 4F;

        // 0 -> x
        // 1 -> y
        // 2 -> z

        transform.eulerAngles = euler;
    }
}
