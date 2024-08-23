using System.Collections.Generic;
using UnityEngine;
using static MyUtility.Utility;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEditor;
using Unity.Services.Authentication;
using UnityEngine.EventSystems;
using Unity.Netcode;
using Unity.VisualScripting;
using static Player;

public class GameInstance : MonoBehaviour {

    public enum ApplicationStatus {
        ERROR = 0,
        LOADING_ASSETS,
        RUNNING
    }
    public enum GameState {
        ERROR = 0,
        MAIN_MENU,
        CONNECTION_MENU,
        WIN_MENU,
        LOSE_MENU,
        PLAYING,
        PAUSED
    }

    [SerializeField] private bool initializeOnStartup = true;
    [SerializeField] private bool debugging = true;


    //Addressables Labels
    private const string essentialAssetsLabel       = "Essential";
    private const string loadingScreenLabel         = "LoadingScreen";



    public ApplicationStatus currentApplicationStatus = ApplicationStatus.ERROR;
    public GameState currentGameState = GameState.ERROR;



    private bool initialized = false;
    private bool initializationInProgress = false;
    private bool assetsLoadingInProgress = false;
    private bool gameStarted = false;
    private bool gamePaused = false;

    private int menusFrameTarget = 60;
    private int gameplayFrameTarget = -1; //RefreshRate

    private AsyncOperationHandle<IList<GameObject>> loadedAssetsHandle;
    private AsyncOperationHandle<GameObject> loadingScreenHandle;

    private Resolution deviceResolution;

    //Entities
    private GameObject soundSystem;
    private GameObject eventSystem;
    private GameObject netcode;

    private GameObject player1Asset;
    private GameObject player2Asset;

    private GameObject player1;
    private GameObject player2;
    private GameObject mainHUD;

    private GameObject mainCamera;
    private GameObject mainMenu;
    private GameObject connectionMenu;
    private GameObject winMenu;
    private GameObject loseMenu;

    private GameObject pauseMenu;
    private GameObject fadeTransition;
    private GameObject loadingScreen;

    //Scripts
    private GameObject rpcManagement;
    private RPCManagement rpcManagementScript;
    private SoundSystem soundSystemScript;
    private LevelManagement levelManagementScript = new LevelManagement();
    private Player player1Script;
    private Player player2Script;
    private NetworkObject player1NetworkObject;
    private NetworkObject player2NetworkObject;

    private MainHUD mainHUDScript;


    private MainCamera mainCameraScript;
    private Netcode netcodeScript;
    private MainMenu mainMenuScript;
    private WinMenu winMenuScript;
    private LoseMenu loseMenuScript;
    private ConnectionMenu connectionMenuScript;
    private FadeTransition fadeTransitionScript;
    private LoadingScreen loadingScreenScript;




    public void Initialize() {
        if (!initializeOnStartup)
            return;

        if (initialized) {
            Warning("Attempted to initialize game while its already initialized!");
            return;
        }
        if (initializationInProgress) {
            Warning("Attempted to initialize game while initialization is in progress!");
            return;
        }

        SetupApplicationInitialSettings();
        initializationInProgress = true;
        levelManagementScript.Initialize(this); //So it starts loading level bundle concurrently.
        LoadAssets();
    }


    //ResourceManagment
    private void LoadAssets() {
        LoadLoadingScreen();
        LoadEssentials();
        assetsLoadingInProgress = true;
        currentApplicationStatus = ApplicationStatus.LOADING_ASSETS;
    }
    private void LoadLoadingScreen() {
        if (debugging)
            Log("Started loading LoadingScreen!");

        loadingScreenHandle = Addressables.LoadAssetAsync<GameObject>(loadingScreenLabel);
        if (!loadingScreenHandle.IsValid()) {
            QuitApplication("Failed to load loading screen\nCheck if label is correct!");
            return;
        }
        loadingScreenHandle.Completed += FinishedLoadingLoadingScreenCallback;
    }
    private void LoadEssentials() {
        if (debugging)
            Log("Started loading essential assets!");

        loadedAssetsHandle = Addressables.LoadAssetsAsync<GameObject>(essentialAssetsLabel, AssetLoadedCallback);
        loadedAssetsHandle.Completed += FinishedLoadingAssetsCallback;
    }
    private bool CheckAssetsLoadingStatus() {
        assetsLoadingInProgress = !loadedAssetsHandle.IsDone;
        return loadedAssetsHandle.IsDone;
    }
    private void ProcessLoadedAsset(GameObject asset) {
        if (!asset)
            return;

        if (asset.CompareTag("Player1")) {
            player1Asset = asset;
        }
        else if (asset.CompareTag("Player2")) {
            player2Asset = asset;
        }
        else if (asset.CompareTag("MainCamera")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            mainCamera = Instantiate(asset);
            mainCameraScript = mainCamera.GetComponent<MainCamera>();
            mainCameraScript.Initialize(this);
            mainCamera.transform.position = new Vector3(0.0f, 0.0f, -10.0f);

            //mainCameraScript.SetPlayerReference(player1Script); //Cant guarantee order!
            Validate(mainCameraScript, "MainCamera component is missing on entity!", ValidationLevel.ERROR, true);
        }
        else if (asset.CompareTag("SoundSystem")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            soundSystem = Instantiate(asset);
            soundSystemScript = soundSystem.GetComponent<SoundSystem>();
            soundSystemScript.Initialize(this);
            Validate(soundSystemScript, "SoundSystem component is missing on entity!", ValidationLevel.ERROR, true);
        }
        else if (asset.CompareTag("RPCManagement")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            rpcManagement = Instantiate(asset);
            rpcManagementScript = rpcManagement.GetComponent<RPCManagement>();
            rpcManagementScript.Initialize(this);
            Validate(rpcManagementScript, "RpcManagement component is missing on entity!", ValidationLevel.ERROR, true);
        }
        else if (asset.CompareTag("EventSystem")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            eventSystem = Instantiate(asset);
        }
        else if (asset.CompareTag("Netcode")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            netcode = Instantiate(asset);
            netcodeScript = netcode.GetComponent<Netcode>();
            netcodeScript.Initialize(this);
            Validate(netcodeScript, "Netcode component is missing on entity!", ValidationLevel.ERROR, true);
        }
        else if (asset.CompareTag("MainMenu")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            mainMenu = Instantiate(asset);
            mainMenuScript = mainMenu.GetComponent<MainMenu>();
            mainMenuScript.Initialize(this);
            Validate(mainMenuScript, "MainMenu component is missing on entity!", ValidationLevel.ERROR, true);
        }
        else if (asset.CompareTag("ConnectionMenu")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            connectionMenu = Instantiate(asset);
            connectionMenuScript = connectionMenu.GetComponent<ConnectionMenu>();
            Validate(connectionMenuScript, "ConnectionMenu component is missing on entity!", ValidationLevel.ERROR, true);
            connectionMenuScript.Initialize(this);
        }
        else if (asset.CompareTag("LoseMenu")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            loseMenu = Instantiate(asset);
            loseMenuScript = loseMenu.GetComponent<LoseMenu>();
            loseMenuScript.Initialize(this);
        }
        else if (asset.CompareTag("WinMenu")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            winMenu = Instantiate(asset);
            winMenuScript = winMenu.GetComponent<WinMenu>();
            winMenuScript.Initialize(this);
        }
        else if (asset.CompareTag("MainHUD")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            mainHUD = Instantiate(asset);
            mainHUDScript = mainHUD.GetComponent<MainHUD>();
            mainHUDScript.Initialize(this);
        }
        else if (asset.CompareTag("PauseMenu")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            pauseMenu = Instantiate(asset);
        }
        else if (asset.CompareTag("FadeTransition")) {
            if (debugging)
                Log("Started creating " + asset.name + " entity");
            fadeTransition = Instantiate(asset);
            fadeTransitionScript = fadeTransition.GetComponent<FadeTransition>();
            fadeTransitionScript.Initialize(this);
        }
        else
            Warning("Loaded an asset that was not recognized!\n[" + asset.name + "]");
    }


    private void SetupApplicationInitialSettings() {
        deviceResolution = Screen.currentResolution;
        if (debugging) {
            Log("Application started on device.");
            Log("Device information:\nScreen Width: [" + deviceResolution.width 
                + "]\nScreen Height: [" + deviceResolution.height + "]\nRefresh Rate: [" 
                + deviceResolution.refreshRateRatio + "]");
        }

        gameplayFrameTarget = (int)deviceResolution.refreshRateRatio.value;
    }
    private void SetApplicationTargetFrameRate(int target) {
        Application.targetFrameRate = target;
        if (debugging)
            Log("Framerate has been set to " + target + "!");
    }


    public static void AbortApplication(object message = null) {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        if (message != null)
            Error(message);
#else
    Application.Quit();

#endif
    }

    public void QuitApplication(object message = null) {
        UnloadResources();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        if (message != null)
            Error(message);
#else
    Application.Quit();

#endif
    }
    private void OnDestroy() {
        UnloadResources();
    }
    private void UnloadResources() {
        if (!initializeOnStartup)
            return;

        //Steps:
        //-Call CleanUp on all entities
        //-Destroy gameobjects
        //-Release resources


        if (player1Script) 
            player1Script.CleanUp("Player 1 cleaned up successfully!");
        if (player2Script)
            player2Script.CleanUp("Player 2 cleaned up successfully!");

        mainCameraScript.CleanUp("MainCamera cleaned up successfully!");
        soundSystemScript.CleanUp("SoundSystem cleaned up successfully!");
        levelManagementScript.CleanUp();

        //Needed to guarantee destruction of all entities before attempting to release resources.
        ValidateAndDestroy(player1);
        ValidateAndDestroy(player2);
        ValidateAndDestroy(mainCamera);
        ValidateAndDestroy(soundSystem);
        ValidateAndDestroy(eventSystem);

        ValidateAndDestroy(mainHUD);
        ValidateAndDestroy(fadeTransition);
        ValidateAndDestroy(loadingScreen);

        ValidateAndDestroy(rpcManagement);
        ValidateAndDestroy(netcode);
        ValidateAndDestroy(mainMenu);
        ValidateAndDestroy(connectionMenu);
        ValidateAndDestroy(pauseMenu);
        ValidateAndDestroy(winMenu);
        ValidateAndDestroy(loseMenu);

        if (debugging)
            Log("Destroyed all entities successfully!");

        if (loadedAssetsHandle.IsValid()) {
            Addressables.Release(loadedAssetsHandle);
            if (debugging)
                Log("Assets were unloaded successfully!");
        }

        if (debugging) {
            Log("Released all resources successfully!");
            Log("Application has been stopped!");
        }
    }
    private void ValidateAndDestroy(GameObject target) {
        if (target)
            Destroy(target);
    }


    //Update/Tick
    void Update() {
        if (!initializeOnStartup)
            return;

        switch (currentApplicationStatus) {
            case ApplicationStatus.LOADING_ASSETS:
                UpdateApplicationLoadingAssetsState();
                break;
            case ApplicationStatus.RUNNING:
                UpdateApplicationRunningState(); 
                break;
            case ApplicationStatus.ERROR:
                QuitApplication("Attempted to update application while status was ERROR");
                break;
        }
    }
    private void UpdateApplicationLoadingAssetsState() {
        if (initialized) {
            Warning("Attempted to update application initialization state while game was already initialized!");
            currentApplicationStatus = ApplicationStatus.RUNNING;
            return;
        }

        if (assetsLoadingInProgress) {
            CheckAssetsLoadingStatus();
            if (loadingScreenScript)
                loadingScreenScript.UpdateLoadingBar(loadedAssetsHandle.PercentComplete);
            if (debugging)
                Log("Loading Assets In Progress...");
            return;
        }
        else if (loadingScreenScript.IsLoadingProcessRunning())
            loadingScreenScript.FinishLoadingProcess();


        SetupDependencies();

        initialized = true;
        initializationInProgress = false;
        currentApplicationStatus = ApplicationStatus.RUNNING;
        SetGameState(GameState.MAIN_MENU);

        if (debugging)
            Log("Game successfully initialized!");
    }
    private void UpdateApplicationRunningState() {
        if (currentGameState == GameState.ERROR) {
            Warning("Unable to update game\nCurrent game state is set to ERROR!");
            return;
        }

        UpdateStatelessSystems();
        //Here you can add more states if needed!
        switch (currentGameState) {
            case GameState.PLAYING:
                UpdatePlayingState();
                break;
            case GameState.CONNECTION_MENU:
                UpdateConnectionMenuState();
                break;
        }
    }
    private void SetupDependencies() {
        //mainCameraScript.SetPlayerReference(player1Script);


        if (debugging)
            Log("All dependencies has been setup!");
    }


    private void FixedUpdate() {
        if (currentApplicationStatus != ApplicationStatus.RUNNING)
            return;

        if (currentGameState == GameState.ERROR) {
            Warning("Unable to call fixed-update \nCurrent game state is set to ERROR!");
            return;
        }

        //Here you can add more states if needed!
        switch (currentGameState) {
            case GameState.PLAYING:
                UpdateFixedPlayingState();
                break;
        }
    }




    public void SetGameState(GameState state) {

        switch (state) {
            case GameState.MAIN_MENU:
                SetupMainMenuState();
                break;
            case GameState.CONNECTION_MENU:
                SetupConnectionMenuState();
                break;
            case GameState.WIN_MENU:
                SetupWinMenuState();
                break;
            case GameState.LOSE_MENU:
                SetupLoseMenuState();
                break;
            case GameState.PLAYING:
                Warning("Use StartGame instead of calling SetGameState(GameState.PLAYING)");
                break;
            case GameState.PAUSED:
                Warning("Use PauseGame/UnpauseGame instead of calling SetGameState(GameState.PAUSED)");
                break;
        }
    }
    public void Transition(GameState state) {

        switch (state) {
            case GameState.MAIN_MENU:
                fadeTransitionScript.StartTransition(SetupMainMenuState);
                break;
            case GameState.CONNECTION_MENU:
                fadeTransitionScript.StartTransition(SetupConnectionMenuState);
                break;
            case GameState.WIN_MENU:
                fadeTransitionScript.StartTransition(SetupWinMenuState);
                break;
            case GameState.LOSE_MENU:
                fadeTransitionScript.StartTransition(SetupLoseMenuState);
                break;
            case GameState.PLAYING:
                Warning("Use StartGame instead of calling Transition(GameState.PLAYING)");
                break;
            case GameState.PAUSED:
                Warning("Use PauseGame/UnpauseGame instead of calling Transition(GameState.PAUSED)");
                break;
        }
    }



    public void PauseGame() {
        gamePaused = true;
        //Put on the menu
    }
    public void UnpauseGame() {
        gamePaused = false;

    }


    //State Setup
    private void SetupMainMenuState() {
        currentGameState = GameState.MAIN_MENU;
        HideAllMenus();
        SetCursorState(true);
        mainMenu.SetActive(true);
        SetApplicationTargetFrameRate(menusFrameTarget);
    }
    private void SetupConnectionMenuState() {
        currentGameState = GameState.CONNECTION_MENU;
        HideAllMenus();
        SetCursorState(true);
        connectionMenuScript.SetupStartState();
        connectionMenu.SetActive(true);
        SetApplicationTargetFrameRate(menusFrameTarget);

    }
    private void SetupStartState() {
        currentGameState = GameState.PLAYING;
        HideAllMenus();

        SetApplicationTargetFrameRate(gameplayFrameTarget);
        SetCursorState(false);

        mainHUD.SetActive(true);

        if (Netcode.IsHost()) {

            Level currentLoadedLevel = levelManagementScript.GetCurrentLoadedLevel();
            Vector3 player1SpawnPosition = currentLoadedLevel.GetPlayer1SpawnPoint();
            Vector3 player2SpawnPosition = currentLoadedLevel.GetPlayer2SpawnPoint();
            player1.transform.position = player1SpawnPosition;
            player2.transform.position = player2SpawnPosition;

            ClientRpcParams clientRpcParams = new ClientRpcParams();
            clientRpcParams.Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { player2NetworkObject.OwnerClientId } };
            rpcManagementScript.RelayPlayerReferenceClientRpc(player1, Player.PlayerID.PLAYER_1, clientRpcParams);
            rpcManagementScript.RelayPlayerReferenceClientRpc(player2, Player.PlayerID.PLAYER_2, clientRpcParams);

            player1Script.SetupStartingState();
            player2Script.SetupStartingState();

            player1Script.SetNetworkedEntityState(true);
            player2Script.SetNetworkedEntityState(true);
        }
    }
    private void SetupWinMenuState() {
        currentGameState = GameState.WIN_MENU;
        HideAllMenus();
        SetCursorState(true);
        winMenu.SetActive(true);
        SetApplicationTargetFrameRate(menusFrameTarget);

    }
    private void SetupLoseMenuState() {
        currentGameState = GameState.LOSE_MENU;
        HideAllMenus();
        SetCursorState(true);
        loseMenu.SetActive(true);
        SetApplicationTargetFrameRate(menusFrameTarget);

    }


    //State Update
    private void UpdateStatelessSystems() {
        soundSystemScript.Tick();
        netcodeScript.Tick();
    }
    private void UpdateConnectionMenuState() {
        connectionMenuScript.Tick();
    }
    private void UpdatePlayingState() {
        mainCameraScript.Tick();

        if (player1Script)
            player1Script.Tick();
        if (player2Script)
            player2Script.Tick();

        mainHUDScript.Tick();
        levelManagementScript.Tick();
    }
    private void UpdateFixedPlayingState() {
        //mainCameraScript.FixedTick();

        if (player1Script)
            player1Script.FixedTick();
        if (player2Script)
            player2Script.FixedTick();
    }


    public bool StartGame() {
        if (!levelManagementScript.LoadLevel("NormalLevel")) //Hardcoded Level
            return false;

        fadeTransitionScript.StartTransition(SetupStartState);
        gameStarted = true;
        return true;
    }
    public bool StartGame(string levelName) {
        if (!levelManagementScript.LoadLevel(levelName))
            return false;

        fadeTransitionScript.StartTransition(SetupStartState);
        gameStarted = true;
        return true;
    }
    public void InterruptGame() {
        //In case of player disconnection!
        if (levelManagementScript.IsLevelLoaded())
            levelManagementScript.UnloadLevel();

        gameStarted = false;
        mainHUD.SetActive(false);

        if (Netcode.IsHost()) {
            if (player1)
                Destroy(player1);
            if (player2)
                Destroy(player2);
        }


        player1 = null;
        player2 = null;
        player1Script = null;
        player2Script = null;
        player1NetworkObject = null;
        player2NetworkObject = null;


        if (gamePaused)
            UnpauseGame();

        netcodeScript.StopNetworking();
        gameStarted = false;
        Transition(GameState.MAIN_MENU);
    }
    private void EndGame() {

        SetCursorState(true);
        mainHUD.SetActive(false);

        //Note: Set game results and send results rpc to clients.

        gameStarted = false;
    }
    public void RestartGame() {
        if (gameStarted) {
            Warning("Attempted to restart before gamestartd");
            return;
        }

        if (Netcode.IsHost()) {
            rpcManagementScript.RestartGameServerRpc(Netcode.GetClientID());

            Level currentLoadedLevel = levelManagementScript.GetCurrentLoadedLevel();
            Vector3 player1SpawnPosition = currentLoadedLevel.GetPlayer1SpawnPoint();
            Vector3 player2SpawnPosition = currentLoadedLevel.GetPlayer2SpawnPoint();
            player1.transform.position = player1SpawnPosition;
            player2.transform.position = player2SpawnPosition;
        }



        player1Script.SetupStartingState();
        player2Script.SetupStartingState();

        HideAllMenus();
        SetCursorState(false);
        mainHUD.SetActive(true);

        gameStarted = true;
        currentGameState = GameState.PLAYING;
        soundSystemScript.PlayTrack("GameplayTrack", true);
    }



    //Level Loading - TODO: Move into LevelManagement class along with related vars


    private void HideAllMenus() {
        //Add all GUI here!
        mainMenu.SetActive(false);
        winMenu.SetActive(false);
        loseMenu.SetActive(false);
        connectionMenu.SetActive(false);
        pauseMenu.SetActive(false);
    }
    public bool IsDebuggingEnabled() { return debugging; }
    public void SetCursorState(bool state) {  
        Cursor.visible = state;

        if (state)
            Cursor.lockState = CursorLockMode.None;
        if (!state)
            Cursor.lockState = CursorLockMode.Locked;
    }


    public void CreatePlayer1(ulong id) {
        player1 = Instantiate(player1Asset);
        player1.name = "NetworkedPlayer_1";
        
        player1Script = player1.GetComponent<Player>();
        Validate(player1Script, "Player1 component is missing on entity!", ValidationLevel.ERROR, true);

        player1Script.SetMainHUDRef(mainHUDScript);
        player1Script.SetPlayerID(Player.PlayerID.PLAYER_1);

        player1Script.Initialize(this);
        player1Script.SetNetworkedEntityState(false);

        player1NetworkObject = player1.GetComponent<NetworkObject>();
        Validate(player1NetworkObject, "Player1 component is missing on entity!", ValidationLevel.ERROR, true);

        player1NetworkObject.SpawnWithOwnership(id);
    }
    public void CreatePlayer2(ulong id) {
        player2 = Instantiate(player2Asset);
        player2.name = "NetworkedPlayer_2";

        player2Script = player2.GetComponent<Player>();
        Validate(player2Script, "Player2 component is missing on entity!", ValidationLevel.ERROR, true);

        player2Script.SetMainHUDRef(mainHUDScript);
        player2Script.SetPlayerID(Player.PlayerID.PLAYER_2);

        player2Script.Initialize(this);
        player2Script.SetNetworkedEntityState(false);

        player2NetworkObject = player2.GetComponent<NetworkObject>();
        Validate(player2NetworkObject, "Player2 component is missing on entity!", ValidationLevel.ERROR, true);

        player2NetworkObject.SpawnWithOwnership(id);
    }


    //RPC Processing
    public void SetReceivedPlayerReferenceRpc(NetworkObjectReference reference, Player.PlayerID id) {
        if (id == Player.PlayerID.NONE)
            return;

        if (!player1 && id == Player.PlayerID.PLAYER_1) {
            player1 = reference;
            player1.name = "NetworkedPlayer_1";
            player1NetworkObject = player1.GetComponent<NetworkObject>();
            player1Script = player1.GetComponent<Player>();
            player1Script.SetPlayerID(id);
            player1Script.SetMainHUDRef(mainHUDScript);
            player1Script.Initialize(this);
            player1Script.SetupStartingState();
        }
        else if (!player2 && id == Player.PlayerID.PLAYER_2) {
            player2 = reference;
            player2.name = "NetworkedPlayer_2";
            player2NetworkObject = player2.GetComponent<NetworkObject>();
            player2Script = player2.GetComponent<Player>();
            player2Script.SetPlayerID(id);
            player2Script.SetMainHUDRef(mainHUDScript);
            player2Script.Initialize(this);
            player2Script.SetupStartingState();
        }
    }
    public void ProcessPlayer2MovementRpc(float input) {
        if (player2Script)
            player2Script.ProcessMovementInputRpc(input);
    }
    public void ProcessPlayer2MovementAnimationState(bool state) {
        if (player2Script)
            player2Script.SetMovementAnimationState(state);
    }
    public void ProcessReceivedChatMessage(string message) {
        mainHUDScript.AddReceivedChatMessage(message);
    }
    public void ProcessPlayerSpriteOrientation(bool flipX) {
        if (Netcode.IsHost())
            player2Script.ProcessSpriteOrientationRpc(flipX);
        else
            player1Script.ProcessSpriteOrientationRpc(flipX);
    }
    public void ProcessPlayerHealthRpc(float amount, Player.PlayerID playerID) {
        if (Netcode.IsHost() || playerID == PlayerID.NONE)
            return;

        mainHUDScript.UpdatePlayerHealth(amount, playerID);
        if (playerID == PlayerID.PLAYER_1)
            player1Script.OverrideCurrentHealth(amount);
        else if (playerID == PlayerID.PLAYER_2)
            player2Script.OverrideCurrentHealth(amount);
    }
    public void ProcessPlayerDeathRpc(Player.PlayerID playerID) {
        if (playerID == PlayerID.NONE)
            return;

        if (playerID == PlayerID.PLAYER_1) {
            if (player1Script)
                player1Script.Kill();
        }
        else if (playerID == PlayerID.PLAYER_2) {
            if (player2Script)
                player2Script.Kill();
        }
    }
    //public void ProcessMatchResults(MatchResults results) {
    //    //if (results == MatchResults.DRAW)
    //    //    SetGameState(GameState.LOSE_MENU);
    //    //else if (results == MatchResults.WIN)
    //    //    SetGameState(GameState.WIN_MENU);
    //    //else if (results == MatchResults.LOSE)
    //    //    SetGameState(GameState.LOSE_MENU);

    //    mainHUD.SetActive(false);
    //    SetCursorState(true);
    //    gameStarted = false;
    //}



    //Getters
    public Netcode GetNetcode() { return netcodeScript; }
    public RPCManagement GetRPCManagement() { return rpcManagementScript; }
    public LevelManagement GetLevelManagement() { return levelManagementScript; }
    public Player GetPlayer1() { return player1Script; }
    public Player GetPlayer2() { return player2Script; }
    public SoundSystem GetSoundSystem() { return soundSystemScript; }




    //Callbacks
    private void AssetLoadedCallback(GameObject asset) {
        if (debugging)
            Log(asset.name + " has been loaded successfully!");

        ProcessLoadedAsset(asset);
    }
    private void FinishedLoadingLoadingScreenCallback(AsyncOperationHandle<GameObject> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            if (debugging)
                Log("Finished loading LoadingScreen successfully!");

            loadingScreen = Instantiate(handle.Result);
            loadingScreen.SetActive(false);
            loadingScreenScript = loadingScreen.GetComponent<LoadingScreen>();
            loadingScreenScript.Initialize(this);
            if (assetsLoadingInProgress)
                loadingScreenScript.StartLoadingProcess(LoadingScreen.LoadingProcess.LOADING_ASSETS);

            if (debugging)
                Log("Created " + handle.Result.name);
        }
        else if (handle.Status == AsyncOperationStatus.Failed) {
            QuitApplication("Failed to load LoadingScreen!\nCheck if label is correct.");
        }
    }
    private void FinishedLoadingAssetsCallback(AsyncOperationHandle<IList<GameObject>> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            if (debugging)
                Log("Finished loading assets successfully!");
        }
        else if (handle.Status == AsyncOperationStatus.Failed) {
            QuitApplication("Failed to load assets!\nCheck if label is correct.");
        }
    }

}
