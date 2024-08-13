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
        gameInstanceRef.StartGame();
        //gameInstanceRef.Transition(GameState.ROLE_SELECT_MENU);
    }

    //References
    [ClientRpc]
    public void RelayPlayerReferenceClientRpc(NetworkObjectReference reference, Player.PlayerID player, ClientRpcParams clientRpcParameters = default) {
        gameInstanceRef.SetReceivedPlayerReferenceRpc(reference, player);
    }



    //Input
    [ServerRpc(RequireOwnership = false)]
    public void CalculatePlayer2PositionServerRpc(float input) {
        gameInstanceRef.ProccessPlayer2MovementRpc(input);
    }



    //[ServerRpc(RequireOwnership = false)]
    //public void SetBoostStateServerRpc(ulong senderID, bool state) {
    //    ClientRpcParams? clientParams = CreateClientRpcParams(senderID);
    //    if (clientParams == null) {
    //        Warning("Invalid client rpc params returned at UpdateReadyCheckServerRpc");
    //        return;
    //    }

    //    RelayBoostStateClientRpc(senderID, state, clientParams.Value);
    //}
    //[ClientRpc]
    //public void RelayBoostStateClientRpc(ulong senderID, bool state, ClientRpcParams paramsPack) {
    //    gameInstanceRef.GetPlayer().GetDaredevilData().SetBoostState(state);
    //}
}
