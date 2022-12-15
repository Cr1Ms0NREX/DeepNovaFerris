using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;

public class Brandon_PlayerPull : MonoBehaviour
{
    public GameObject player;
    public Transform playerCenter;
    public LayerMask objectsToPull;
    public float pullForce;
    public float refreshRate;
    public float pullRadius;
    private bool canPull = false;


    private void FixedUpdate() // Function for the collision of the Player and original spot. 
    {
        if (canPull)
        {
            Collider[] hitPlayer = Physics.OverlapSphere(playerCenter.position, pullRadius, objectsToPull); 

            foreach (Collider player in hitPlayer)
            {
                StartCoroutine(pullObject(player, true));
            }
            return;
        }
    }

    IEnumerator pullObject(Collider x, bool shouldPull) // Function for the force the pull has on an object
    {
        if (shouldPull & x != null) // Pull
        {
            Vector3 forceDir = playerCenter.position - x.transform.position;
            x.GetComponent<Rigidbody>().AddForce(forceDir.normalized * pullForce * Time.deltaTime);
            yield return new WaitForSeconds(refreshRate);
            StartCoroutine(pullObject(x, !shouldPull));
        }
    }
    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            canPull = false;
        }
    }
    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            canPull = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCenter == null)
            return;
        Gizmos.DrawWireSphere(playerCenter.position, pullRadius);
    }
}
