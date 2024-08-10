using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : Entity {

    public override void Initialize(GameInstance game) {
        if (initialized)
            return;


        gameInstanceRef = game;
        initialized = true;
    }

    public void IdkButton()
    {

    }

    public void BackButton() {
        gameInstanceRef.SetGameState(GameInstance.GameState.MAIN_MENU);
    }
}
