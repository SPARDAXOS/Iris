using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MyUtility.Utility;

public class Level : Entity {

    Vector3 player1SpawnPoint = Vector3.zero;
    Vector3 player2SpawnPoint = Vector3.zero;

    public override void Initialize(GameInstance game) {
        if (initialized)
            return;


        SetupReferences();
        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized)
            return;

    }
    public void SetupReferences() {

        player1SpawnPoint = transform.Find("Player1SpawnPoint").transform.position;
        Validate(player1SpawnPoint, "Failed to find player 1 spawn point reference!", ValidationLevel.ERROR, true);

        player2SpawnPoint = transform.Find("Player2SpawnPoint").transform.position;
        Validate(player2SpawnPoint, "Failed to find player 2 spawn point reference!", ValidationLevel.ERROR, true);
    }





    public Vector3 GetPlayer1SpawnPoint() { return player1SpawnPoint; }
    public Vector3 GetPlayer2SpawnPoint() { return player2SpawnPoint; }
}
