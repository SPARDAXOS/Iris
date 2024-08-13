using System.Collections;
using System.Collections.Generic;
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
        player2SpawnPoint = transform.Find("Player2SpawnPoint").transform.position;



        //Spawn Point
        //Transform spawnPointTransform = transform.Find("SpawnPoint");
        //if (Validate(spawnPointTransform, "No spawn point was found!\nSpawn point set to 0.0.0!", ValidationLevel.WARNING)) {
        //    spawnPoint = spawnPointTransform.position;
        //    spawnPointTransform.gameObject.SetActive(false);
        //}




    }






    public Vector3 GetPlayer1SpawnPoint() { return player1SpawnPoint; }
    public Vector3 GetPlayer2SpawnPoint() { return player2SpawnPoint; }
}
