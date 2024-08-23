using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections;
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



    //Game State
    //References
    [ClientRpc]
    public void RelayPlayerReferenceClientRpc(NetworkObjectReference reference, Player.PlayerID player, ClientRpcParams clientRpcParameters = default) {
        gameInstanceRef.SetReceivedPlayerReferenceRpc(reference, player);
    }
    [ServerRpc (RequireOwnership = false)]
    public void ConfirmConnectionServerRpc() {
        RelayConnectionConfirmationClientRpc();
    }
    [ClientRpc]
    public void RelayConnectionConfirmationClientRpc() {
        gameInstanceRef.StartGame();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestGameRestartServerRpc(ulong senderID) {
        ClientRpcParams? clientRpcParams = CreateClientRpcParams(senderID);
        if (clientRpcParams == null)
            return;

        RelayGameRestartRequestClientRpc((ClientRpcParams)clientRpcParams);
    }
    [ClientRpc]
    public void RelayGameRestartRequestClientRpc(ClientRpcParams clientRpcParameters = default) {
        gameInstanceRef.RestartGame();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RestartGameServerRpc(ulong senderID) {
        ClientRpcParams? clientRpcParams = CreateClientRpcParams(senderID);
        if (clientRpcParams == null)
            return;

        RelayRestartGameClientRpc((ClientRpcParams)clientRpcParams);
    }
    [ClientRpc]
    public void RelayRestartGameClientRpc(ClientRpcParams clientRpcParameters = default) {
        gameInstanceRef.RestartGame();
    }


    //[ServerRpc(RequireOwnership = false)]
    //public void UpdateMatchResultsServerRpc(MatchResults results, ulong senderID) {
    //    ClientRpcParams? clientRpcParams = CreateClientRpcParams(senderID);
    //    if (clientRpcParams == null)
    //        return;

    //    RelayMatchResultsClientRpc(results, (ClientRpcParams)clientRpcParams);
    //}
    //[ClientRpc]
    //public void RelayMatchResultsClientRpc(MatchResults results, ClientRpcParams clientRpcParameters = default) {
    //    gameInstanceRef.ProcessMatchResults(results);
    //}





    //Input
    [ServerRpc(RequireOwnership = false)]
    public void CalculatePlayer2PositionServerRpc(float input) {
        gameInstanceRef.ProcessPlayer2MovementRpc(input);
    }



    //Animations
    [ServerRpc(RequireOwnership = false)]
    public void NotifyMovementAnimationStateServerRpc(bool state) {
        gameInstanceRef.ProcessPlayer2MovementAnimationState(state);
    }



    //HUD
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerHealthServerRpc(float amount, Player.PlayerID playerID, ulong senderID) {
        ClientRpcParams? clientRpcParams = CreateClientRpcParams(senderID);
        if (clientRpcParams == null)
            return;

        RelayPlayerHealthClientRpc(amount, playerID, (ClientRpcParams)clientRpcParams);
    }
    [ClientRpc]
    public void RelayPlayerHealthClientRpc(float amount, Player.PlayerID playerID, ClientRpcParams clientRpcParameters = default) {
        gameInstanceRef.ProcessPlayerHealthRpc(amount, playerID);
    }





    //Sprite Orientation
    [ServerRpc(RequireOwnership = false)]
    public void UpdateSpriteOrientationServerRpc(bool flipX, ulong senderID) {
        ClientRpcParams? clientRpcParams = CreateClientRpcParams(senderID);
        if (clientRpcParams == null)
            return;

        RelaySpriteOrientationClientRpc(flipX, (ClientRpcParams)clientRpcParams);
    }
    [ClientRpc]
    public void RelaySpriteOrientationClientRpc(bool flipX, ClientRpcParams clientRpcParameters = default) {
        gameInstanceRef.ProcessPlayerSpriteOrientation(flipX);
    }



  



    //Chat
    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(FixedString32Bytes message, ulong senderID) {
        ClientRpcParams? clientRpcParams = CreateClientRpcParams(senderID);
        if (clientRpcParams == null)
            return;

        RelayChatMessageClientRpc(message, (ClientRpcParams)clientRpcParams);
    }
    [ClientRpc]
    public void RelayChatMessageClientRpc(FixedString32Bytes message, ClientRpcParams clientRpcParameters = default) {
        gameInstanceRef.ProcessReceivedChatMessage(message.ToString());
    }
}
