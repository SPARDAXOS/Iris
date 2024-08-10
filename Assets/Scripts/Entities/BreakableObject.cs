using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [SerializeField] float deacelerationModifier = 2.0f;
    [SerializeField] float explosiveForce = 15.0f;
    [SerializeField] float explosionRadius = 4.0f;
    [SerializeField] float upwardsModifier = 1.0f;

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player"))
            return;

        //Player interaction here.

        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<Rigidbody>().AddExplosionForce(explosiveForce, transform.GetChild(i).position, explosionRadius, upwardsModifier, ForceMode.Impulse);
        }
    }
}
