using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;
using static GameInstance;
using static MyUtility.Utility;


public class RPCManagement : NetworkedEntity {


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
    private ClientRpcParams? CreateClientRpcParams(ulong senderID) {
        Netcode netcodeRef = gameInstanceRef.GetNetcode();
        var targetID = netcodeRef.GetOtherClient(senderID); //Do more elegant solution
        if (targetID == senderID) {
            Log("Other client look up failed!");
            return null;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams();
        clientRpcParams.Send = new ClientRpcSendParams();
        clientRpcParams.Send.TargetClientIds = new ulong[] { targetID };
        return clientRpcParams;
    }



    [ServerRpc (RequireOwnership = true)]
    public void ConfirmConnectionServerRpc() {
        RelayConnectionConfirmationClientRpc();
    }
    [ClientRpc]
    public void RelayConnectionConfirmationClientRpc() {
        gameInstanceRef.Transition(GameState.ROLE_SELECT_MENU);
    }






    [ServerRpc (RequireOwnership = false)]
    public void UpdateReadyCheckServerRpc(ulong senderID, bool value) {
        ClientRpcParams? clientParams = CreateClientRpcParams(senderID);
        if (clientParams == null) {
            Warning("Invalid client rpc params returned at UpdateReadyCheckServerRpc");
            return;
        }

        RelayReadyCheckClientRpc(senderID, value, clientParams.Value);
    }
    [ClientRpc]
    public void RelayReadyCheckClientRpc(ulong senderID, bool value, ClientRpcParams paramsPack) {
        gameInstanceRef.GetRoleSelectMenu().ReceiveReadyCheckRPC(value);
    }


    [ServerRpc (RequireOwnership = false)]
    public void UpdateRoleSelectionServerRpc(ulong senderID, Player.Identity identity) {
        ClientRpcParams? clientParams = CreateClientRpcParams(senderID);
        if (clientParams == null) {
            Warning("Invalid client rpc params returned at UpdateReadyCheckServerRpc");
            return;
        }

        RelayRoleSelectionClientRpc(senderID, identity, clientParams.Value);
    }
    [ClientRpc]
    public void RelayRoleSelectionClientRpc(ulong senderID, Player.Identity identity, ClientRpcParams paramsPack) {
        gameInstanceRef.GetRoleSelectMenu().ReceiveRoleSelectionRPC(identity);
    }


    [ServerRpc (RequireOwnership = true)]
    public void ConfirmRoleSelectionServerRpc(ulong senderID) {
        ClientRpcParams? clientParams = CreateClientRpcParams(senderID);
        if (clientParams == null) {
            Warning("Invalid client rpc params returned at UpdateReadyCheckServerRpc");
            return;
        }

        RelayRoleSelectionConfirmationClientRpc(senderID, clientParams.Value);
    }
    [ClientRpc]
    public void RelayRoleSelectionConfirmationClientRpc(ulong senderID, ClientRpcParams paramsPack) {
        gameInstanceRef.GetLevelManagement().QueueLevelLoadKey("DebugLevel"); //Temporary
        gameInstanceRef.StartGame();
    }









    //Coordinator
    [ServerRpc(RequireOwnership = false)]
    public void SetBoostStateServerRpc(ulong senderID, bool state) {
        ClientRpcParams? clientParams = CreateClientRpcParams(senderID);
        if (clientParams == null) {
            Warning("Invalid client rpc params returned at UpdateReadyCheckServerRpc");
            return;
        }

        RelayBoostStateClientRpc(senderID, state, clientParams.Value);
    }
    [ClientRpc]
    public void RelayBoostStateClientRpc(ulong senderID, bool state, ClientRpcParams paramsPack) {
        gameInstanceRef.GetPlayer().GetDaredevilData().SetBoostState(state);
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void SetObstacleActivationStateServerRpc(ulong senderID, Obstacle.ObstacleActivationState state) {
        ClientRpcParams? clientParams = CreateClientRpcParams(senderID);
        if (clientParams == null) {
            Warning("Invalid client rpc params returned at UpdateReadyCheckServerRpc");
            return;
        }

        RelayObstacleActivationStateClientRpc(senderID, state, clientParams.Value);
    }
    [ClientRpc]
    public void RelayObstacleActivationStateClientRpc(ulong senderID, Obstacle.ObstacleActivationState state, ClientRpcParams paramsPack) {
        LevelManagement levelManagement = gameInstanceRef.GetLevelManagement();
        if (!levelManagement.IsLevelLoaded()) {
            Warning("Received obstacle activation state rpc while level was not loaded!");
            return;
        }

        levelManagement.GetCurrentLoadedLevel().SetCurrentObstacleState(state);
    }


}
