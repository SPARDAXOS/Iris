using System.Net;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MyUtility.Utility;

public class ConnectionMenu : Entity {

    public enum ConnectionTypeSelection {
        NONE = 0,
        LOCAL,
        GLOBAL
    }
    public enum ConnectionMenuState {
        SELECT_CONNECTION,
        SELECT_MODE,
        CLIENT,
        HOST
    }


    private const string connectionSelectionMessage = "Select Connection Type!";
    private const string modeSelectionMessage = "Select Connection Option!";
    private const string hostWaitingMessage = "Waiting for player 2 to join...";
    private const string enterConnectionCodeMessage = "Enter code to connect!";



    public ConnectionMenuState currentMenuState = ConnectionMenuState.SELECT_CONNECTION;
    public ConnectionTypeSelection connectionSelectionType = ConnectionTypeSelection.NONE;


    private Button hostButtonComp = null;
    private Button clientButtonComp = null;
    private Button localMPButtonComp = null;
    private Button globalMPButtonComp = null;
    private Button confirmJoinCodeButtonComp = null;

    private TextMeshProUGUI statusTextComp = null;
    private TextMeshProUGUI connectionCodeTextComp = null; //Could be reused for the join code later

    private TMP_InputField connectionCodeInputComp = null;




    public override void Initialize(GameInstance game) {
        if (initialized)
            return;



        gameInstanceRef = game;
        SetupReferences();
        initialized = true;
    }
    public override void Tick() {
        if (!initialized) {
            Warning("ConnectionMenu was ticked while it was not yet initialized!");
            return;
        }

        CheckConnectionStatus();
    }
    private bool CheckUnityServicesStatus() {
        if (currentMenuState == ConnectionMenuState.SELECT_CONNECTION) {
            if (Application.internetReachability == NetworkReachability.NotReachable) {
                statusTextComp.text = "No internet!\nPlease connect to proceed.";
                globalMPButtonComp.interactable = false;
                localMPButtonComp.interactable = false;
            }
            else if (!RelayManager.IsUnityServicesInitialized()) {
                statusTextComp.text = connectionSelectionMessage + "\nGlobal Multiplayer is unavailable!";
                localMPButtonComp.interactable = true;
                globalMPButtonComp.interactable = false;
            }
            else {
                statusTextComp.text = connectionSelectionMessage;
                localMPButtonComp.interactable = true;
                globalMPButtonComp.interactable = true;
            }
            return true;
        }
        return false;
    }
    private void CheckConnectionStatus() {

        //Check for client screen - Anything before the networking can start!
        //Notes:
        //Unity Services - Sign In - Network Connectivity
        //This should cover up for every possible opening up until the netcode is running
        //-Then the responsibility of handling these openings falls to the netcode manager and calling InterruptGame()


        if (CheckUnityServicesStatus())
            return;
        else if (currentMenuState == ConnectionMenuState.SELECT_MODE && connectionSelectionType == ConnectionTypeSelection.GLOBAL) {
            if (Application.internetReachability == NetworkReachability.NotReachable) { //Somewhat unneccessary since it cant reach this point a 
                statusTextComp.text = "No internet!\nConnect to sign in.";
                hostButtonComp.gameObject.SetActive(false);
                clientButtonComp.gameObject.SetActive(false);
            }
            else if (!RelayManager.IsUnityServicesInitialized()) {
                statusTextComp.text = "Invalid state!\nUnity services are not initialized!";
                hostButtonComp.gameObject.SetActive(false);
                clientButtonComp.gameObject.SetActive(false);
            }
            else if (!RelayManager.IsSignedIn()) {
                statusTextComp.text = "Signing in...\nRestart application if it takes too long.";
                hostButtonComp.gameObject.SetActive(false);
                clientButtonComp.gameObject.SetActive(false);
            }
            else {
                statusTextComp.text = modeSelectionMessage;
                hostButtonComp.gameObject.SetActive(true);
                clientButtonComp.gameObject.SetActive(true);
            }
        }
        else if (currentMenuState == ConnectionMenuState.CLIENT && connectionSelectionType == ConnectionTypeSelection.GLOBAL) {
            if (Application.internetReachability == NetworkReachability.NotReachable) {
                statusTextComp.text = "No internet!\nPlease reconnect to continue.";
                connectionCodeInputComp.gameObject.SetActive(false);
                connectionCodeInputComp.text = null;
            }
            else if (!RelayManager.IsUnityServicesInitialized()) {
                statusTextComp.text = "Invalid state!\nUnity services are not initialized!";
                connectionCodeInputComp.gameObject.SetActive(false);
                connectionCodeInputComp.text = null;
            }
            else if (!RelayManager.IsSignedIn()) {
                statusTextComp.text = "Invalid state!\nUser is not signed in!";
                connectionCodeInputComp.gameObject.SetActive(false);
                connectionCodeInputComp.text = null;
            }
            else {
                statusTextComp.text = enterConnectionCodeMessage;
                connectionCodeInputComp.gameObject.SetActive(true);
            }
        }
        else if (currentMenuState == ConnectionMenuState.CLIENT && connectionSelectionType == ConnectionTypeSelection.LOCAL) {
            if (Application.internetReachability == NetworkReachability.NotReachable) {
                statusTextComp.text = "No internet!\nPlease reconnect to continue.";
                connectionCodeInputComp.gameObject.SetActive(false);
                connectionCodeInputComp.text = null;
            }
            else {
                statusTextComp.text = enterConnectionCodeMessage;
                connectionCodeInputComp.gameObject.SetActive(true);
            }
        }
    }


    //Called upon opening this menu
    public void SetupStartState() {
        CheckUnityServicesStatus();
        SetMenuState(ConnectionMenuState.SELECT_CONNECTION);
    }

    private void SetupReferences() {

        //Local Multiplayer Button
        Transform localMPButtonTransform = transform.Find("LocalMPButton");
        if (!Validate(localMPButtonTransform, "ConnectionMenu failed to get reference to LocalMPButton transform", ValidationLevel.ERROR, false))
            return;
        localMPButtonComp = localMPButtonTransform.GetComponent<Button>();
        if (!Validate(localMPButtonComp, "ConnectionMenu failed to get reference to LocalMPButton component", ValidationLevel.ERROR, false))
            return;

        //Global Multiplayer Button
        Transform globalMPButtonTransform = transform.Find("GlobalMPButton");
        if (!Validate(globalMPButtonTransform, "ConnectionMenu failed to get reference to GlobalMPButton transform", ValidationLevel.ERROR, false))
            return;
        globalMPButtonComp = globalMPButtonTransform.GetComponent<Button>();
        if (!Validate(globalMPButtonComp, "ConnectionMenu failed to get reference to GlobalMPButton component", ValidationLevel.ERROR, false))
            return;

        //Host Button
        Transform hostButtonTransform = transform.Find("HostButton");
        if (!Validate(hostButtonTransform, "ConnectionMenu failed to get reference to HostButton transform", ValidationLevel.ERROR, false))
            return;
        hostButtonComp = hostButtonTransform.GetComponent<Button>();
        if (!Validate(hostButtonComp, "ConnectionMenu failed to get reference to host Button component", ValidationLevel.ERROR, false))
            return;

        //Client Button
        Transform clientButtonTransform = transform.Find("ClientButton");
        if (!Validate(clientButtonTransform, "ConnectionMenu failed to get reference to ClientButton transform", ValidationLevel.ERROR, false))
            return;
        clientButtonComp = clientButtonTransform.GetComponent<Button>();
        if (!Validate(clientButtonComp, "ConnectionMenu failed to get reference to client Button component", ValidationLevel.ERROR, false))
            return;

        //Confirm JoinCode Button
        Transform confirmJoinCodeButtonTransform = transform.Find("ConfirmJoinCodeButton");
        if (!Validate(confirmJoinCodeButtonTransform, "ConnectionMenu failed to get reference to ConfirmJoinCodeButton transform", ValidationLevel.ERROR, false))
            return;
        confirmJoinCodeButtonComp = confirmJoinCodeButtonTransform.GetComponent<Button>();
        if (!Validate(confirmJoinCodeButtonComp, "ConnectionMenu failed to get reference to ConfirmJoinCode Button component", ValidationLevel.ERROR, false))
            return;

        //Connection Code Input Field
        Transform connectionCodeTransform = transform.Find("ConnectionCodeInputField");
        if (!Validate(connectionCodeTransform, "ConnectionMenu failed to get reference to ConnectionCodeInputField transform", ValidationLevel.ERROR, false))
            return;
        connectionCodeInputComp = connectionCodeTransform.GetComponent<TMP_InputField>();
        if (!Validate(connectionCodeInputComp, "ConnectionMenu failed to get reference to InputField component", ValidationLevel.ERROR, false))
            return;

        //Status Text
        Transform statusTextTransform = transform.Find("StatusText");
        if (!Validate(statusTextTransform, "ConnectionMenu failed to get reference to status text transform", ValidationLevel.ERROR, false))
            return;
        statusTextComp = statusTextTransform.GetComponent<TextMeshProUGUI>();
        if (!Validate(statusTextComp, "ConnectionMenu failed to get reference to status Text component", ValidationLevel.ERROR, false))
            return;

        //ConnectionCode Text
        Transform connectionCodeTextTransform = transform.Find("ConnectionCode");
        if (!Validate(connectionCodeTextTransform, "ConnectionMenu failed to get reference to ConnectionCode text transform", ValidationLevel.ERROR, false))
            return;
        connectionCodeTextComp = connectionCodeTextTransform.GetComponent<TextMeshProUGUI>();
        if (!Validate(connectionCodeTextComp, "ConnectionMenu failed to get reference to ConnectionCode Text component", ValidationLevel.ERROR, false))
            return;
        connectionCodeInputComp.characterLimit = 12; //This should be the limit on ip address decrypted values (XXX).(XXX).(XXX).(XXX)
    }
    private void SetMenuState(ConnectionMenuState state) {

        currentMenuState = state;
        if (state == ConnectionMenuState.SELECT_CONNECTION) {

            statusTextComp.text = connectionSelectionMessage;
            connectionSelectionType = ConnectionTypeSelection.NONE;

            localMPButtonComp.gameObject.SetActive(true);
            globalMPButtonComp.gameObject.SetActive(true);
            hostButtonComp.gameObject.SetActive(false);
            clientButtonComp.gameObject.SetActive(false);
            connectionCodeTextComp.gameObject.SetActive(false);
            connectionCodeInputComp.gameObject.SetActive(false);
            confirmJoinCodeButtonComp.gameObject.SetActive(false);
        }
        else if (state == ConnectionMenuState.SELECT_MODE) {

            statusTextComp.text = modeSelectionMessage;

            localMPButtonComp.gameObject.SetActive(false);
            globalMPButtonComp.gameObject.SetActive(false);
            hostButtonComp.gameObject.SetActive(true);
            clientButtonComp.gameObject.SetActive(true);
            connectionCodeTextComp.gameObject.SetActive(false);
            connectionCodeInputComp.gameObject.SetActive(false);
            confirmJoinCodeButtonComp.gameObject.SetActive(false);
            connectionCodeTextComp.text = "Retrieving Join Code...";
        }
        else if (state == ConnectionMenuState.HOST) {

            statusTextComp.text = hostWaitingMessage;

            localMPButtonComp.gameObject.SetActive(false);
            globalMPButtonComp.gameObject.SetActive(false);
            hostButtonComp.gameObject.SetActive(false);
            clientButtonComp.gameObject.SetActive(false);
            connectionCodeTextComp.gameObject.SetActive(true);
            connectionCodeInputComp.gameObject.SetActive(false);
            confirmJoinCodeButtonComp.gameObject.SetActive(false);

            if (connectionSelectionType == ConnectionTypeSelection.LOCAL)
                connectionCodeTextComp.text = "Join Code: " + gameInstanceRef.GetNetcode().GetEncryptedLocalHost();
            //Otherwise its the callback that would let it update the text once it is received.
        }
        else if (state == ConnectionMenuState.CLIENT) {

            statusTextComp.text = enterConnectionCodeMessage;
            connectionCodeInputComp.text = "";
            localMPButtonComp.gameObject.SetActive(false);
            globalMPButtonComp.gameObject.SetActive(false);
            hostButtonComp.gameObject.SetActive(false);
            clientButtonComp.gameObject.SetActive(false);
            connectionCodeTextComp.gameObject.SetActive(false);
            connectionCodeInputComp.gameObject.SetActive(true);
            confirmJoinCodeButtonComp.gameObject.SetActive(true);
        }
    }






    public void SelectLocalConnectionModeButton() {
        gameInstanceRef.GetSoundSystem().PlaySFX("ButtonConfirm");
        connectionSelectionType = ConnectionTypeSelection.LOCAL;
        SetMenuState(ConnectionMenuState.SELECT_MODE);
    }
    public void SelectGlobalConnectionModeButton() {
        gameInstanceRef.GetSoundSystem().PlaySFX("ButtonConfirm");
        connectionSelectionType = ConnectionTypeSelection.GLOBAL;
        SetMenuState(ConnectionMenuState.SELECT_MODE);
    }
    public void ConfirmConnectionCode() {
        gameInstanceRef.GetSoundSystem().PlaySFX("ButtonConfirm");
        Netcode netcodeRef = gameInstanceRef.GetNetcode();
        if (netcodeRef.IsDebugLogEnabled())
            Log("Attempting to connect to " + connectionCodeInputComp.text);

        if (connectionSelectionType == ConnectionTypeSelection.LOCAL) {
            string connectionCode = netcodeRef.DecryptConnectionCode(connectionCodeInputComp.text);
            if (connectionCode != null)
                netcodeRef.StartLocalClient(connectionCode);
            else {
                if (gameInstanceRef.IsDebuggingEnabled())
                    Warning("Invalid code received after decryption");
            }
        }
        else if (connectionSelectionType == ConnectionTypeSelection.GLOBAL)
            netcodeRef.StartGlobalClient(connectionCodeInputComp.text);
            

    }

    public void UpdateConnectionCode(string code) {
        if (currentMenuState != ConnectionMenuState.HOST) //Catch multi-threading bug
            gameInstanceRef.GetNetcode().StopNetworking();
        else
            connectionCodeTextComp.text = "Join Code: " + code;
    }
    public void HostButton() {
        gameInstanceRef.GetSoundSystem().PlaySFX("ButtonConfirm");
        Netcode netcodeRef = gameInstanceRef.GetNetcode();
        if (connectionSelectionType == ConnectionTypeSelection.LOCAL)
            netcodeRef.StartLocalHost();
        else if (connectionSelectionType == ConnectionTypeSelection.GLOBAL)
            netcodeRef.StartGlobalHost(UpdateConnectionCode);

        SetMenuState(ConnectionMenuState.HOST);
    }
    public void ClientButton() {
        gameInstanceRef.GetSoundSystem().PlaySFX("ButtonConfirm");
        SetMenuState(ConnectionMenuState.CLIENT);
    }
    public void BackButton() {
        gameInstanceRef.GetSoundSystem().PlaySFX("ButtonCancel");
        if (currentMenuState == ConnectionMenuState.HOST) {
            gameInstanceRef.GetNetcode().StopNetworking();
            SetMenuState(ConnectionMenuState.SELECT_MODE);
        }
        else if (currentMenuState == ConnectionMenuState.CLIENT) {
            gameInstanceRef.GetNetcode().StopNetworking();
            SetMenuState(ConnectionMenuState.SELECT_MODE);
        }
        else if (currentMenuState == ConnectionMenuState.SELECT_MODE) {
            connectionSelectionType = ConnectionTypeSelection.NONE;
            SetMenuState(ConnectionMenuState.SELECT_CONNECTION);
        }
        else if (currentMenuState == ConnectionMenuState.SELECT_CONNECTION)
            gameInstanceRef.Transition(GameInstance.GameState.MAIN_MENU);
    }
}
