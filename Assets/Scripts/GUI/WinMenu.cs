using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinMenu : Entity {
    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        initialized = true;
    }

    public void PlayButton() {
        if (Netcode.IsHost())
            gameInstanceRef.RestartGame();
        else {
            RPCManagement management = gameInstanceRef.GetRPCManagement();
            management.RequestGameRestartServerRpc(Netcode.GetClientID());
        }
        //gameInstanceRef.GetSoundSystem().PlaySFX("ButtonConfirm");
    }
    public void QuitButton() {
        //gameInstanceRef.GetSoundSystem().PlaySFX("ButtonCancel");
        gameInstanceRef.QuitApplication();
    }
}
