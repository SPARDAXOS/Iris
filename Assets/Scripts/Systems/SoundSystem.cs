using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static MyUtility.Utility;




public class SoundSystem : Entity {

    private struct SFXRequest {
        public string key;
        public GameObject owner;
        public AudioSource unit;
    }


    [Space(5)]
    [Header("Optimization")]
    [SerializeField] private uint soundUnitsLimit = 20;

    [Space(5)]
    [Header("Fade")]
    [Range(0.01f, 1.0f)] private float fadeInSpeed = 0.1f;
    [Range(0.01f, 1.0f)] private float fadeOutSpeed = 0.1f;
    [Tooltip("Should interrupt a fade in/out and immediately play the new track")]
    [SerializeField] private bool canInterruptFade = false;

    [Space(5)]
    [Header("Volume")]
    [Range(0.0f, 1.0f)] private float masterVolume = 1.0f;
    [Range(0.0f, 1.0f)] private float musicVolume = 1.0f;
    [Range(0.0f, 1.0f)] private float SFXVolume = 1.0f;


    private const string SFXBundleLabel = "SFXBundle";
    private const string tracksBundleLabel = "TracksBundle";

    private bool initializing = false;
    private bool loadingBundles = false;
    private bool loadingAudioClips = false;

    private bool canPlaySFX = false;
    private bool canPlayTracks = false;

    private AsyncOperationHandle<ScriptableObject> SFXBundleHandle;
    private AsyncOperationHandle<ScriptableObject> tracksBundleHandle;

    private Dictionary<string, AsyncOperationHandle<AudioClip>> loadedSFXAssetsHandles;
    private Dictionary<string, AsyncOperationHandle<AudioClip>> loadedTracksAssets;


    private bool fadingIn = false;
    private bool fadingOut = false;
    private TrackEntry? fadeTargetTrack = null;
    private string currentPlayingTrackKey = null;

    private float currentTrackVolume = 0.0f;
    private float targetTrackEntryVolume = 0.0f;


    private GameObject unit = null;
    private AudioSource trackAudioSource = null; //TODO: Create this only if i know that i can play track sounds!
    private List<AudioSource> soundUnits = new List<AudioSource>(); //TODO: Create this only if i know that i can play sfx sounds!
    private List<SFXRequest> SFXRequests = new List<SFXRequest>(); //TODO: Create this only if i know that i can play sfx sounds!



    public override void Initialize(GameInstance game) {
        if (initialized || initializing)
            return;

        gameInstanceRef = game;
        initializing = true;
        LoadBundles();
    }
    public override void Tick() {
        if (initializing)
            UpdateIntializingState();
        else if (canPlaySFX || canPlayTracks)
            UpdateNormalState();
    }
    public override void CleanUp(string message = "Entity cleaned up successfully!") {
        if (gameInstanceRef.IsDebuggingEnabled())
            Log(message);

        UnloadResources();
    }

    private void UpdateIntializingState() {
        if (loadingBundles) {
            if (HasFinishedLoadingBundles())
                LoadAudioClips();
        }
        else if (loadingAudioClips) {
            if (HasFinishedLoadingAudioClips()) {
                if (gameInstanceRef.IsDebuggingEnabled())
                    Log("SoundSystem has finished loading AudioClips");
                ConfirmInitialization();
            }
        }
        else
            Error("Was not supposed to be called!!!!!!");
    }
    private void UpdateNormalState() {







    }

    //Resource Management
    private void UnloadResources() {
        if (SFXBundleHandle.IsValid()) {
            Addressables.Release(SFXBundleHandle);
            if (gameInstanceRef.IsDebuggingEnabled())
                Log("SoundSystem has unloaded SFXBundle successfully!");
        }
        if (tracksBundleHandle.IsValid()) {
            Addressables.Release(tracksBundleHandle);
            if (gameInstanceRef.IsDebuggingEnabled())
                Log("SoundSystem has unloaded TracksBundle successfully!");
        }

        if (loadedSFXAssetsHandles != null) {
            foreach (var asset in loadedSFXAssetsHandles) {
                Addressables.Release(asset.Value);
                if (gameInstanceRef.IsDebuggingEnabled())
                    Log("SoundSystem has unloaded SFX audio clip [" + asset.Key + "]");
            }
        }

        if (loadedTracksAssets != null) {
            foreach (var asset in loadedTracksAssets) {
                Addressables.Release(asset.Value);
                if (gameInstanceRef.IsDebuggingEnabled())
                    Log("SoundSystem has unloaded track audio clip [" + asset.Key + "]");
            }
        }


        if (gameInstanceRef.IsDebuggingEnabled())
            Log("SoundSystem has released all resources successfully!");
    }
    private void LoadBundles() {
        if (gameInstanceRef.IsDebuggingEnabled())
            Log("SoundSystem started loading bundles!");

        //Notes:
        //-All exceptions thrown by LoadAssetAsync are disabled.
        //-Should not crash the game if an exception is thrown.

        SFXBundleHandle = Addressables.LoadAssetAsync<ScriptableObject>(SFXBundleLabel);
        if (SFXBundleHandle.IsDone)
            Warning("SoundSystem failed to load SFXBundle\nReason: " + SFXBundleHandle.OperationException.Message + "\nPlaying SFX will not be possible!");
        else
            SFXBundleHandle.Completed += FinishedLoadingSFXBundleCallback;
        
        tracksBundleHandle = Addressables.LoadAssetAsync<ScriptableObject>(tracksBundleLabel);
        if (tracksBundleHandle.IsDone)
            Warning("SoundSystem failed to load TracksBundle\nReason: " + tracksBundleHandle.OperationException.Message + "\nPlaying tracks will not be possible!");
        else
            tracksBundleHandle.Completed += FinishedLoadingTracksBundleCallback;
        
        if (SFXBundleHandle.IsDone && tracksBundleHandle.IsDone) {
            Warning("SoundSystem will not be able to play any audio clips!");
            ConfirmInitialization();
        }
        else
            loadingBundles = true;
    }
    private void LoadAudioClips() {

        loadingAudioClips = false;

        if (canPlaySFX) {
            loadedSFXAssetsHandles = new Dictionary<string, AsyncOperationHandle<AudioClip>>();
            foreach(var asset in ((SFXBundle)SFXBundleHandle.Result).entries) {
                var handle = Addressables.LoadAssetAsync<AudioClip>(asset.clip);
                handle.Completed += FinishedLoadingAudioClipCallback;
                loadedSFXAssetsHandles.Add(asset.key, handle);
                if (gameInstanceRef.IsDebuggingEnabled())
                    Log("SoundSystem started loading SFX audio clip with key [" + asset.key + "]");
            }
            loadingAudioClips = true;
        }
        if (canPlayTracks) {
            loadedTracksAssets = new Dictionary<string, AsyncOperationHandle<AudioClip>>();
            foreach(var asset in ((TracksBundle)tracksBundleHandle.Result).entries) {
                var handle = Addressables.LoadAssetAsync<AudioClip>(asset.clip);
                handle.Completed += FinishedLoadingAudioClipCallback;
                loadedTracksAssets.Add(asset.key, handle);
                if (gameInstanceRef.IsDebuggingEnabled())
                    Log("SoundSystem started loading track audio clip with key [" + asset.key + "]");
            }
            loadingAudioClips = true;
        }

        
    }

    private bool HasFinishedLoadingBundles() {
        bool result = true;

        result &= SFXBundleHandle.IsDone;
        result &= tracksBundleHandle.IsDone;

        loadingBundles = !result;
        return result;
    }
    private bool HasFinishedLoadingAudioClips() {
        bool result = true;

        foreach(var asset in loadedSFXAssetsHandles)
            result &= asset.Value.IsDone;
        foreach (var asset in loadedTracksAssets)
            result &= asset.Value.IsDone;

        loadingAudioClips = !result;
        return result;
    }
    private void ConfirmInitialization() {
        if (gameInstanceRef.IsDebuggingEnabled())
            Log("SoundSystem has been initialized successfully!");

        initializing = false;
        initialized = true;
    }


    //User
    public bool PlayLocalSFX(string key) {
        if (!canPlaySFX) {
            Warning("Unable to play local sfx associated with key [" + key + "]\n Reason: SoundSystem is not able to play SFX audio clips!");
            return false;
        }






        return true;
    }
    public bool PlayGlobalSFX(string key) {
        if (!canPlaySFX) {
            Warning("Unable to play global sfx associated with key [" + key + "]\n Reason: SoundSystem is not able to play SFX audio clips!");
            return false;
        }




        return true;
    }
    public bool PlayTrack(string key) {
        if (!canPlayTracks) {
            Warning("Unable to play track associated with key [" + key + "]\n Reason: SoundSystem is not able to play track audio clips!");
            return false;
        }






        return true;
    }
    public void StopTrack() {

    }

    //Look into how to update the current used values for sounds when these change!
    public void SetMasterVolume(float volume) {
        masterVolume = volume;
    }
    public void SetMusicVolume(float volume) {
        musicVolume = volume;
    }
    public void SetSFXVolume(float volume) {
        SFXVolume = volume;
    }
    public float GetMasterVolume() { return masterVolume; }
    public float GetMusicVolume() { return musicVolume; }
    public float GetSFXVolume() { return SFXVolume; }


    //Callbacks
    private void FinishedLoadingTracksBundleCallback(AsyncOperationHandle<ScriptableObject> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            if (gameInstanceRef.IsDebuggingEnabled())
                Log("SoundSystem finished loading TracksBundle successfully!");
            canPlayTracks = true;
        }
        else if (handle.Status == AsyncOperationStatus.Failed) {
            Warning("SoundSystem failed to load TracksBundle\nPlaying tracks will not be possible!");
        }
    }
    private void FinishedLoadingSFXBundleCallback(AsyncOperationHandle<ScriptableObject> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            if (gameInstanceRef.IsDebuggingEnabled())
                Log("SoundSystem finished loading SFXBundle successfully!");
            canPlaySFX = true;
        }
        else if (handle.Status == AsyncOperationStatus.Failed) {
            Warning("SoundSystem failed to load SFXBundle\nPlaying SFX will not be possible!");
        }
    }
    private void FinishedLoadingAudioClipCallback(AsyncOperationHandle<AudioClip> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            if (gameInstanceRef.IsDebuggingEnabled())
                Log("SoundSystem loaded audio clip [" + handle.Result.name + "] successfully!");
        }
        else if (handle.Status == AsyncOperationStatus.Failed)
            Warning("SoundSystem failed to load [" + handle.Result.name + "]\n Playing this audio clip will be possible!");
    }
}
