using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyUtility.Utility;

public class CreditsMenu : Entity
{

    public override void Initialize(GameInstance game)
    {
        if (initialized)
            return;

        gameInstanceRef = game;
        initialized = true;
    }



    public void BackButton()
    {
        gameInstanceRef.Transition(GameInstance.GameState.MAIN_MENU);
    }




}
