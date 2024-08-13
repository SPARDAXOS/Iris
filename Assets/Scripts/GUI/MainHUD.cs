using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainHUD : Entity {

    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        initialized = true;
    }


}
