using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static MyUtility.Utility;

public class LevelManagement : ImplementationEntity {

    private const string levelsBundleLabel = "LevelsBundle";

    private AsyncOperationHandle<ScriptableObject> levelsBundleHandle;

    private AsyncOperationHandle<GameObject> currentLoadedLevelHandle;
    private GameObject currentLoadedLevel = null;
    private Level currentLoadedLevelScript = null;
    private string queuedLevelLoadKey = null;



    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        LoadLevelsBundle();
    }
    public override void Tick() {
        if (currentLoadedLevelScript)
            currentLoadedLevelScript.Tick();
    }
    public override void CleanUp(string message = "LevelManagment cleaned up successfully!") {
        UnloadResources();
        if (gameInstanceRef.IsDebuggingEnabled())
            Log(message);
    }
    private void UnloadResources() {

        //TODO: TEST OUT THE STATE OF A HANDLE BEFORE LOADING AND AFTER UNLOADING! IMPORTANT!
        //Note:
        //-on unloading, it becomes invalid! the handle!
        if (currentLoadedLevelHandle.IsValid()) {
            currentLoadedLevelScript.CleanUp();
            if (currentLoadedLevel)
                GameObject.Destroy(currentLoadedLevel);

            Addressables.Release(currentLoadedLevelHandle);
            if (gameInstanceRef.IsDebuggingEnabled())
                Log("Level was destroyed and unloaded successfully!");
        }

        if (levelsBundleHandle.IsValid()) {
            Addressables.Release(levelsBundleHandle);
            if (gameInstanceRef.IsDebuggingEnabled())
                Log("Levels bundle was unloaded successfully!");
        }
    }


    private void LoadLevelsBundle() {
        if (gameInstanceRef.IsDebuggingEnabled())
            Log("Started loading levels bundle!");

        levelsBundleHandle = Addressables.LoadAssetAsync<ScriptableObject>(levelsBundleLabel);
        if (levelsBundleHandle.IsDone)
            Error("Failed to load levels bundle!\nCheck if label is correct!");
        else
            levelsBundleHandle.Completed += FinishedLoadingLevelsBundleCallback;
    }

    public bool LoadQueuedLevelKey() {
        if (queuedLevelLoadKey == null) {
            Error("Failed to loaded queued level key!\nNo queued level key exists!");
            return false;
        }

        return LoadLevel(queuedLevelLoadKey);
    }
    public bool QueueLevelLoadKey(string levelKey) {
        if (queuedLevelLoadKey != null) {
            Error("Failed to queue level key for loading!\nLoad current queued level key or remove key first!");
            return false;
        }

        queuedLevelLoadKey = levelKey;
        return true;
    }
    public void ClearQueuedLevelKey() {
        if (queuedLevelLoadKey == null) {
            Warning("Failed to clear queued level key!\nThere is no level key queued currently!");
            return;
        }

        queuedLevelLoadKey = null;
    }


    public bool LoadLevel(string levelKey) {
        if (currentLoadedLevel) {
            Error("Failed to load level!\nUnload current level first before loading a new one!");
            return false;
        }
        if (!levelsBundleHandle.IsValid()) {
            Error("Failed to load level!\nLevels bundle was not loaded!");
            return false;
        }

        LevelEntry requestedLevel = new LevelEntry();
        bool levelFound = false;
        foreach (LevelEntry level in ((LevelsBundle)levelsBundleHandle.Result).Entries) {
            if (level.key == levelKey) {
                requestedLevel = level;
                levelFound = true;
            }
        }

        if (!levelFound) {
            Error("Failed to load level!\nRequested level was not found in the levels bundle!");
            return false;
        }

        currentLoadedLevelHandle = Addressables.LoadAssetAsync<GameObject>(requestedLevel.asset);
        if (currentLoadedLevelHandle.IsDone) {
            Error("Failed to load level!\nRequested level was not found in the addressables!");
            return false;
        }
        currentLoadedLevelHandle.Completed += FinishedLoadingLevelCallback;

        if (gameInstanceRef.IsDebuggingEnabled())
            Log("Started loading level associated with key [" + levelKey + "]");

        return true;
    }
    public bool UnloadLevel() {
        if (!currentLoadedLevel) {
            Warning("Unable to unload level!\nNo levels are currently loaded!");
            return false;
        }

        currentLoadedLevelScript.CleanUp();
        if (currentLoadedLevel)
            GameObject.Destroy(currentLoadedLevel);
        if (currentLoadedLevelHandle.IsValid())
            Addressables.Release(currentLoadedLevelHandle);

        if (gameInstanceRef.IsDebuggingEnabled())
            Log("Started unloading current level!");

        return true;
    }
    private bool CreateLevel(GameObject asset) {
        if (currentLoadedLevel) {
            Error("Failed to create level\nThere is currently a level loaded already!");
            return false;
        }

        currentLoadedLevel = GameObject.Instantiate(asset);
        if (!currentLoadedLevel) {
            Error("Failed to create level\nInstantiation failed!");
            return false;
        }

        currentLoadedLevelScript = currentLoadedLevel.GetComponent<Level>();
        if (!currentLoadedLevelScript) {
            Error("Failed to create level\nLevel is missing essential component!");
            return false;
        }

        currentLoadedLevelScript.Initialize(gameInstanceRef);
        return true;
    }


    public LevelsBundle GetLevelsBundle() { return (LevelsBundle)levelsBundleHandle.Result; }
    public bool IsLevelLoaded() { return currentLoadedLevel != null; }
    public Level GetCurrentLoadedLevel() { return currentLoadedLevelScript; }


    //Callbacks
    private void FinishedLoadingLevelCallback(AsyncOperationHandle<GameObject> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            if (gameInstanceRef.IsDebuggingEnabled())
                Log("Finished loading level successfully!");

            bool result = CreateLevel(handle.Result);
            if (!result)
                Error("Failed to create level!\nCheck asset for any errors!");

            //Start game or any registered callback for level loading
        }
        else if (handle.Status == AsyncOperationStatus.Failed) {
            Error("Failed to load level!"); //going back to main menu. message
            gameInstanceRef.InterruptGame(); //Should be safe as long as interrupt game cleans all.
        }
    }
    private void FinishedLoadingLevelsBundleCallback(AsyncOperationHandle<ScriptableObject> handle) {
        //NOTE: Could ditch even assinging the callbacks depending on showSystemMessages if its only debugging code in them
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            if (gameInstanceRef.IsDebuggingEnabled())
                Log("Finished loading levels bundle successfully!");
            initialized = true;
        }
        else if (handle.Status == AsyncOperationStatus.Failed) {
            gameInstanceRef.QuitApplication("Failed to load levels bundle!\nCheck if label is correct.");
        }
    }

}
