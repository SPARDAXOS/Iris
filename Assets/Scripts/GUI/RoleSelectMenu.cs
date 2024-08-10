using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;
using static MyUtility.Utility;

public class RoleSelectMenu : Entity {

    public bool client1Ready = false;
    public bool client2Ready = false;

    public Player.Identity client1Identity = Player.Identity.NONE;
    public Player.Identity client2Identity = Player.Identity.NONE;


    private Image daredevilClient1Checkmark = null;
    private Image daredevilClient2Checkmark = null;

    private Image coordinatorClient1Checkmark = null;
    private Image coordinatorClient2Checkmark = null;

    private Image client1ReadyCheck = null;
    private Image client2ReadyCheck = null;




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


        //Daredevil Button
        Transform daredevilButtonTransform = transform.Find("DaredevilButton");
        Validate(daredevilButtonTransform, "Failed to get DaredevilButton transform", ValidationLevel.ERROR, true);

        //Daredevil Client 1 Checkmark
        Transform daredevilClient1Transform = daredevilButtonTransform.Find("Client1Checkmark");
        Validate(daredevilClient1Transform, "Failed to get Client1Checkmark transform - Daredevil", ValidationLevel.ERROR, true);
        daredevilClient1Checkmark = daredevilClient1Transform.GetComponent<Image>();
        Validate(daredevilClient1Checkmark, "Failed to get daredevilClient1Checkmark component", ValidationLevel.ERROR, true);

        //Daredevil Client 2 Checkmark
        Transform daredevilClient2Transform = daredevilButtonTransform.Find("Client2Checkmark");
        Validate(daredevilClient2Transform, "Failed to get Client2Checkmark transform - Daredevil", ValidationLevel.ERROR, true);
        daredevilClient2Checkmark = daredevilClient2Transform.GetComponent<Image>();
        Validate(daredevilClient2Checkmark, "Failed to get daredevilClient2Checkmark component", ValidationLevel.ERROR, true);


        //Coordinator Button
        Transform coordinatorButtonTransform = transform.Find("CoordinatorButton");
        Validate(coordinatorButtonTransform, "Failed to get CoordinatorButton transform", ValidationLevel.ERROR, true);

        //Coordinator Client 1 Checkmark
        Transform coordinatorClient1Transform = coordinatorButtonTransform.Find("Client1Checkmark");
        Validate(coordinatorClient1Transform, "Failed to get Client1Checkmark transform - Coordinator", ValidationLevel.ERROR, true);
        coordinatorClient1Checkmark = coordinatorClient1Transform.GetComponent<Image>();
        Validate(coordinatorClient1Checkmark, "Failed to get coordinatorClient1Checkmark component", ValidationLevel.ERROR, true);

        //Coordinator Client 2 Checkmark
        Transform coordinatorClient2Transform = coordinatorButtonTransform.Find("Client2Checkmark");
        Validate(coordinatorClient2Transform, "Failed to get Client2Checkmark transform - Coordinator", ValidationLevel.ERROR, true);
        coordinatorClient2Checkmark = coordinatorClient2Transform.GetComponent<Image>();
        Validate(coordinatorClient2Checkmark, "Failed to get coordinatorClient2Checkmark component", ValidationLevel.ERROR, true);


        //Ready Button
        Transform readyButtonTransform = transform.Find("ReadyButton");
        Validate(readyButtonTransform, "Failed to get ReadyButton transform", ValidationLevel.ERROR, true);

        //Client 1 Ready Check
        Transform client1Transform = readyButtonTransform.Find("Client1ReadyCheck");
        Validate(client1Transform, "Failed to get Client1ReadyCheck transform", ValidationLevel.ERROR, true);
        client1ReadyCheck = client1Transform.GetComponent<Image>();
        Validate(client1ReadyCheck, "Failed to get Image1 component", ValidationLevel.ERROR, true);

        //Client 2 Ready Check
        Transform client2Transform = readyButtonTransform.Find("Client2ReadyCheck");
        Validate(client2Transform, "Failed to get Client2ReadyCheck transform", ValidationLevel.ERROR, true);
        client2ReadyCheck = client2Transform.GetComponent<Image>();
        Validate(client2ReadyCheck, "Failed to get Image2 component", ValidationLevel.ERROR, true);
    }
    public void SetupMenuStartState() {
        client1Ready = false;
        client2Ready = false;

        client1Identity = Player.Identity.NONE;
        client2Identity = Player.Identity.NONE;

        daredevilClient1Checkmark.gameObject.SetActive(false);
        daredevilClient2Checkmark.gameObject.SetActive(false);

        daredevilClient1Checkmark.rectTransform.localPosition = Vector3.zero;
        daredevilClient2Checkmark.rectTransform.localPosition = Vector3.zero;

        coordinatorClient1Checkmark.gameObject.SetActive(false);
        coordinatorClient2Checkmark.gameObject.SetActive(false);

        coordinatorClient1Checkmark.rectTransform.localPosition = Vector3.zero;
        coordinatorClient2Checkmark.rectTransform.localPosition = Vector3.zero;

        client1ReadyCheck.gameObject.SetActive(false);
        client2ReadyCheck.gameObject.SetActive(false);
    }
    private void UpdateGUI() {

        if (client1Identity == Player.Identity.NONE) {
            if (client2Identity == Player.Identity.DAREDEVIL) {

                daredevilClient2Checkmark.gameObject.SetActive(true);
                daredevilClient2Checkmark.rectTransform.localPosition = Vector3.zero;

                coordinatorClient2Checkmark.gameObject.SetActive(false);
                coordinatorClient2Checkmark.rectTransform.localPosition = Vector3.zero;
            }
            else if (client2Identity == Player.Identity.COORDINATOR) {

                coordinatorClient2Checkmark.gameObject.SetActive(true);
                coordinatorClient2Checkmark.rectTransform.localPosition = Vector3.zero;

                daredevilClient2Checkmark.gameObject.SetActive(false);
                daredevilClient2Checkmark.rectTransform.localPosition = Vector3.zero;
            }
            return;
        }

        if (client1Identity == Player.Identity.DAREDEVIL) {
            if (client1Identity == client2Identity) {
                daredevilClient1Checkmark.gameObject.SetActive(true);
                daredevilClient2Checkmark.gameObject.SetActive(true);

                daredevilClient1Checkmark.rectTransform.localPosition = new Vector3(-100.0f, 0.0f, 0.0f);
                daredevilClient2Checkmark.rectTransform.localPosition = new Vector3(100.0f, 0.0f, 0.0f);

                coordinatorClient1Checkmark.gameObject.SetActive(false);
                coordinatorClient2Checkmark.gameObject.SetActive(false);
            }
            else {
                daredevilClient1Checkmark.gameObject.SetActive(true);
                daredevilClient2Checkmark.gameObject.SetActive(false);

                daredevilClient1Checkmark.rectTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                coordinatorClient2Checkmark.rectTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

                coordinatorClient1Checkmark.gameObject.SetActive(false);
                if (client2Identity == Player.Identity.COORDINATOR)
                    coordinatorClient2Checkmark.gameObject.SetActive(true);
                else
                    coordinatorClient2Checkmark.gameObject.SetActive(false);
            }
        }
        else if (client1Identity == Player.Identity.COORDINATOR) {
            if (client1Identity == client2Identity) {
                coordinatorClient1Checkmark.gameObject.SetActive(true);
                coordinatorClient2Checkmark.gameObject.SetActive(true);

                coordinatorClient1Checkmark.rectTransform.localPosition = new Vector3(-100.0f, 0.0f, 0.0f);
                coordinatorClient2Checkmark.rectTransform.localPosition = new Vector3(100.0f, 0.0f, 0.0f);

                daredevilClient1Checkmark.gameObject.SetActive(false);
                daredevilClient2Checkmark.gameObject.SetActive(false);
            }
            else {
                coordinatorClient1Checkmark.gameObject.SetActive(true);
                coordinatorClient2Checkmark.gameObject.SetActive(false);

                coordinatorClient1Checkmark.rectTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                daredevilClient2Checkmark.rectTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

                daredevilClient1Checkmark.gameObject.SetActive(false);
                if (client2Identity == Player.Identity.DAREDEVIL)
                    daredevilClient2Checkmark.gameObject.SetActive(true);
                else
                    daredevilClient2Checkmark.gameObject.SetActive(false);
            }
        }
    }
    private void CheckReadyStatus() {
        if (client1Ready && client2Ready && client1Identity != client2Identity) {
            gameInstanceRef.GetPlayer().AssignPlayerIdentity(client1Identity);
            if (gameInstanceRef.GetNetcode().IsHost()) {
                ulong clientID = Netcode.GetClientID();
                gameInstanceRef.GetRPCManagement().ConfirmRoleSelectionServerRpc((ulong)clientID);
                gameInstanceRef.GetLevelManagement().QueueLevelLoadKey("DebugLevel"); //Temporary
                gameInstanceRef.StartGame();
            }
        }
    }


    public void ReceiveReadyCheckRPC(bool value) {
        client2Ready = value;
        client2ReadyCheck.gameObject.SetActive(value);
        CheckReadyStatus(); //TEST THIS OUT TO FIX A BUG WHERE THE CLIENT DOES IT AFTERWARDS AFTER THE PC
    }
    public void ReceiveRoleSelectionRPC(Player.Identity value) {
        client2Identity = value;
        UpdateGUI();
    }


    public void ReadyButton() {
        ulong clientID = Netcode.GetClientID();
        if (client1Ready)
            client1Ready = false;
        else if (!client1Ready)
            client1Ready = true;

        client1ReadyCheck.gameObject.SetActive(client1Ready);
        gameInstanceRef.GetRPCManagement().UpdateReadyCheckServerRpc((ulong)clientID, client1Ready);
        CheckReadyStatus();
    }
    public void CoordinatorButton() {
        ulong clientID = Netcode.GetClientID();
        if (client1Identity == Player.Identity.COORDINATOR)
            return;

        client1Identity = Player.Identity.COORDINATOR;
        UpdateGUI();
        gameInstanceRef.GetRPCManagement().UpdateRoleSelectionServerRpc((ulong)clientID, client1Identity);
    }
    public void DaredevilButton() {
        ulong clientID = Netcode.GetClientID();
        if (client1Identity == Player.Identity.DAREDEVIL)
            return;

        client1Identity = Player.Identity.DAREDEVIL;
        UpdateGUI();
        gameInstanceRef.GetRPCManagement().UpdateRoleSelectionServerRpc((ulong)clientID, client1Identity);
    }
}
