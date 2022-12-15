using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brandon_AddForce : MonoBehaviour
{
    private Transform playerModel;
    private Rigidbody rb;
    public int forwardForce = 1; // Forward Force
    public int movementForce = 1; // Movement Force
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerModel = transform.GetChild(0);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("object"))
        {
            movementForce = 10000;
            rb.AddForce(-transform.forward * movementForce);
        }
        if (collider.CompareTag("object") && Input.GetKey(KeyCode.D))
        {
            movementForce = 50000;
            rb.AddForce(-transform.right * movementForce);
        }
        if (collider.CompareTag("object") && Input.GetKey(KeyCode.W))
        {
            movementForce = 50000;
            rb.AddForce(-transform.up * movementForce);
        }
        if (collider.CompareTag("object") && Input.GetKey(KeyCode.A))
        {
            movementForce = 50000;
            rb.AddForce(transform.right * movementForce);
        }
        if (collider.CompareTag("object") && Input.GetKey(KeyCode.S))
        {
            movementForce = 50000;
            rb.AddForce(transform.up * movementForce);
        }
    }
    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("object"))
        {
            rb.velocity = Vector3.zero;
            movementForce = 0;
        }
        if (collider.CompareTag("Player"))
        {

        }
    }

    void Update()
    {
        
    }
}
