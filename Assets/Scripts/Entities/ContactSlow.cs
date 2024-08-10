using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactSlow : Obstacle
{
    [SerializeField] float deacelerationModifier = 2.0f;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;


    }
}
