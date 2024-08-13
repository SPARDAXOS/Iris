using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : Entity {


    private Player playerRef = null;


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized)
            return;

    }
    public void SetPlayerReference(Player player) {
        playerRef = player;
    }


}
