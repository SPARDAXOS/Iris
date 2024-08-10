using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTrigger : Entity
{
    Level levelRef;
    public override void Initialize(GameInstance game)
    {
        if (initialized)
            return;



        gameInstanceRef = game;
        initialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //TODO notify x in levelRef

        }

    }
}
