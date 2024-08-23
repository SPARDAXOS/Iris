using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseMenu : Entity {

    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        initialized = true;
    }


    public void QuitButton() {
        //gameInstanceRef.GetSoundSystem().PlaySFX("ButtonCancel");
        gameInstanceRef.InterruptGame();
    }
}
