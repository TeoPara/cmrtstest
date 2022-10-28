using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 a = Vector3.zero;

        if (Input.GetKey(KeyCode.A))
            a += new Vector3(-1, 0) * Time.deltaTime * 50;

        if (Input.GetKey(KeyCode.S))
            a += new Vector3(0, -1) * Time.deltaTime * 50;

        if (Input.GetKey(KeyCode.D))
            a += new Vector3(1, 0) * Time.deltaTime * 50;

        if (Input.GetKey(KeyCode.W))
            a += new Vector3(0, 1) * Time.deltaTime * 50;

        transform.position += a;

        if (Input.GetKey(KeyCode.R))
            GetComponent<Camera>().orthographicSize *= ((1) + (0.5f * Time.deltaTime));
        if (Input.GetKey(KeyCode.F))
            GetComponent<Camera>().orthographicSize /= ((1) + (0.5f * Time.deltaTime));
    }
}
