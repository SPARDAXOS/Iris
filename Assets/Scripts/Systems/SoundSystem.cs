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

    [Header("Bundles")]
    [SerializeField] private SFXBundle sfxBundle;
    [SerializeField] private TracksBundle tracksBundle;

    [Space(5)]
    [Header("General")]
    [Range(1, 100)][SerializeField] private uint soundUnitsLimit = 50;

    [Space(5)]
    [Header("Fade")]
    [Range(0.01f, 1.0f)][SerializeField] private float fadeInSpeed = 0.1f;
    [Range(0.01f, 1.0f)][SerializeField] private float fadeOutSpeed = 0.1f;
    [Tooltip("Should interrupt a fade in/out and play a new track")]
    [SerializeField] private bool canInterruptFade = true;

    [Space(5)]
    [Header("Volume")]
    [Range(0.0f, 1.0f)][SerializeField] private float masterVolume = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] private float musicVolume = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] private float sfxVolume = 1.0f;


    private bool canPlaySFX = false;
    private bool canPlayTracks = false;

    private bool loadingSFXAssets = false;
    private bool loadingTracksAssets = false;
    private bool loadingAssets = false;

    private Dictionary<string, AsyncOperationHandle<AudioClip>> loadedSFXClips = new Dictionary<string, AsyncOperationHandle<AudioClip>>();
    private Dictionary<string, AsyncOperationHandle<AudioClip>> loadedTracksClips = new Dictionary<string, AsyncOperationHandle<AudioClip>>();


    private bool fadingIn = false;
    private bool fadingOut = false;
    private TrackEntry? fadeTargetTrack = null;
    private string currentPlayingTrackKey = null;

    private float currentTrackVolume = 0.0f;
    private float currentTrackEntryVolume = 0.0f;

    private AudioSource trackAudioSource = null;

    private GameObject units = null;
    private List<AudioSource> soundUnits = new List<AudioSource>();
    private List<SFXRequest> SFXRequests = new List<SFXRequest>();


    private void OnDestroy() {
        UnloadAllAssets();
    }
    public override void Initialize(GameInstance instance) {
        if (initialized) {
            Debug.LogWarning("Attempted to initialize an already intialized entity! - " + gameObject.name);
            return;
        }

        ValidateBundles();
        LoadAssets();
        SetupReferences();

        gameInstanceRef = instance;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized) {
            Debug.LogWarning("Attempted to tick an unintialized entity! - " + gameObject.name);
            return;
        }

        if (loadingAssets) {
            CheckAssetsLoadingStatus();
            return;
        }

        if (canPlaySFX && SFXRequests.Count > 0)
            UpdateSFXRequests();
        else if (canPlayTracks) {
            if (fadingIn)
                UpdateTrackFadeIn();
            else if (fadingOut)
                UpdateTrackFadeOut();
            else
                UpdateTrackVolume();
        }
    }
    private void SetupReferences() {

        trackAudioSource = GetComponent<AudioSource>();
        if (!MyUtility.Utility.Validate(trackAudioSource, "Failed to get AudioSource at " + gameObject.name, MyUtility.Utility.ValidationLevel.WARNING)) {
            canPlaySFX = false;
            canPlayTracks = false;
        }
    }
    private void ValidateBundles() {
        if (!sfxBundle) {
            Debug.LogWarning("SoundManager is missing an SFXBundle - Playing SFX will not be possible!");
            canPlaySFX = false;
        }
        else
            canPlaySFX = true;

        if (!tracksBundle) {
            Debug.LogWarning("SoundManager is missing a TracksBundle - Playing tracks will not be possible!");
            canPlayTracks = false;
        }
        else
            canPlayTracks = true;
    }
    private void LoadAssets() {
        loadingAssets = false;
        if (canPlaySFX) {
            LoadSFXBundle();
            loadingAssets = true;
        }
        if (canPlayTracks) {
            LoadTracksBundle();
            loadingAssets = true;
        }

        if (loadingAssets)
            Debug.Log("SoundManager started loading assets!");
    }
    private void LoadSFXBundle() {
        foreach (var asset in sfxBundle.entries) {
            AsyncOperationHandle<AudioClip> Handle = Addressables.LoadAssetAsync<AudioClip>(asset.clip);
            Handle.Completed += AssetLoadedCallback;
            loadedSFXClips.Add(asset.key, Handle);
        }

        loadingSFXAssets = true;
    }
    private void LoadTracksBundle() {
        foreach (var asset in tracksBundle.entries) {
            AsyncOperationHandle<AudioClip> Handle = Addressables.LoadAssetAsync<AudioClip>(asset.clip);
            Handle.Completed += AssetLoadedCallback;
            loadedTracksClips.Add(asset.key, Handle);
        }

        loadingTracksAssets = true;
    }
    private void UnloadAllAssets() {
        foreach (var entry in loadedSFXClips)
            Addressables.Release(entry.Value);

        foreach (var entry in loadedTracksClips)
            Addressables.Release(entry.Value);

        Debug.Log("SoundManager successfully unloaded all resources");
    }

    private void AssetLoadedCallback(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<AudioClip> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded)
            Debug.Log("Successfully loaded " + handle.Result.ToString());
        else
            Debug.Log("Failed to load " + handle.Result.ToString());
    }

    private void CheckAssetsLoadingStatus() {
        if (!loadingAssets)
            return;

        bool Results = true;
        if (loadingSFXAssets)
            Results &= HasFinishedLoadingSFX();
        if (loadingTracksAssets)
            Results &= HasFinishedLoadingTracks();

        if (Results) {
            loadingAssets = false; //??? it was true
            Debug.Log("SoundManager finished loading assets!");
        }
        //Also confirm if this is usable or not
        //Rework validate bundles into this function and the loadAssets one then!
    }
    private bool HasFinishedLoadingSFX() {
        bool status = true;
        foreach (var asset in loadedSFXClips) {
            if (asset.Value.Status != AsyncOperationStatus.Succeeded)
                status = false;
        }

        loadingSFXAssets = !status;
        return status;
    }
    private bool HasFinishedLoadingTracks() {
        bool status = true;
        foreach (var asset in loadedTracksClips) {
            if (asset.Value.Status != AsyncOperationStatus.Succeeded)
                status = false;
        }

        loadingTracksAssets = !status;
        return status;
    }



    public bool PlaySFX(string key, bool newUnit = false, GameObject owner = null) {
        if (!canPlaySFX) {
            Debug.LogWarning("SoundManager can not play SFX! - PlaySFX will always fail!");
            return false;
        }

        if (loadingSFXAssets) {
            Debug.LogWarning("PlaySFX request rejected due to sfx assets being in the loading process!");
            return false;
        }

        if (!newUnit) {
            SFXRequest? request = FindSFXRequest(owner, key);
            if (request != null)
                return false;
        }

        SFXEntry? targetSFXEntry = FindSFXEntry(key);
        if (targetSFXEntry == null) {
            Debug.Log("Unable to find sfx entry associated with key " + key);
            return false;
        }

        AudioClip targetAudioClip = FindSFXAudioClip(key);
        if (targetAudioClip == null) {
            Debug.Log("Unable to find audio clip associated with key " + key);
            return false;
        }

        AudioSource availableAudioSource = GetAvailableAudioSource();
        if (!availableAudioSource) {
            Debug.LogWarning("Unable to find available audio source to play sfx associated with key " + key);
            return false;
        }

        float volume = masterVolume * sfxVolume * targetSFXEntry.Value.volume;
        availableAudioSource.clip = targetAudioClip;
        availableAudioSource.volume = volume;
        availableAudioSource.pitch = GetRandomizedPitch(targetSFXEntry.Value.minPitch, targetSFXEntry.Value.maxPitch);
        availableAudioSource.Play();

        if (!newUnit)
            AddSFXRequest(owner, key, availableAudioSource);

        return true;
    }
    public bool PlayTrack(string key, bool fade = false) {
        if (!canPlayTracks) {
            Debug.LogWarning("SoundManager can not play tracks! - PlayTrack will always fail!");
            return false;
        }
        if (loadingTracksAssets) {
            //Debug.LogWarning("PlayTrack request rejected due to tracks assets being in the loading process!");
            return false;
        }

        if (currentPlayingTrackKey == key)
            return true;

        TrackEntry? targetTrackEntry = FindTrackEntry(key);
        if (targetTrackEntry == null) {
            Debug.Log("Unable to find track entry " + key + " to play");
            return false;
        }

        if (!fade)
            return ApplyTrack((TrackEntry)targetTrackEntry);

        //Very confusing and could be done better.
        if (canInterruptFade) {
            if (fadingIn || fadingOut) {
                fadingIn = false;
                fadingOut = false;
                fadeTargetTrack = null;
                return ApplyTrack((TrackEntry)targetTrackEntry);
            }
        }

        if (fadingIn) //For book-keeping consistency.
            fadingIn = false;

        fadeTargetTrack = targetTrackEntry;
        if (!trackAudioSource.isPlaying) {
            currentTrackVolume = 0.0f;
            StartFadeIn((TrackEntry)targetTrackEntry);
        }
        else
            StartFadeOut();

        return true;
    }
    public void StopTrack(bool fade = false) {
        if (!trackAudioSource.isPlaying)
            return;

        //Confusing and could be done better.
        if (fadingIn) {
            fadingIn = false;
            fadeTargetTrack = null;
        }

        if (fadingOut) {
            fadingOut = false;
            fadeTargetTrack = null;
        }

        if (fade)
            StartFadeOut();
        else
            trackAudioSource.Stop();
    }



    public void SetMasterVolume(float value) {
        masterVolume = value;
    }
    public void SetMusicVolume(float value) {
        musicVolume = value;
    }
    public void SetSFXVolume(float value) {
        sfxVolume = value;
    }
    public float GetMasterVolume() {
        return masterVolume;
    }
    public float GetMusicVolume() {
        return musicVolume;
    }
    public float GetSFXVolume() {
        return sfxVolume;
    }


    private void UpdateTrackFadeIn() {
        if (currentTrackVolume >= currentTrackEntryVolume)
            return;

        currentTrackVolume += fadeInSpeed * Time.deltaTime;
        trackAudioSource.volume = currentTrackVolume * masterVolume * musicVolume;
        if (currentTrackVolume >= currentTrackEntryVolume) {
            currentTrackVolume = currentTrackEntryVolume;
            trackAudioSource.volume = currentTrackVolume * masterVolume * musicVolume;
            fadingIn = false;
        }
    }
    private void UpdateTrackFadeOut() {
        if (currentTrackEntryVolume <= 0.0f)
            return;

        currentTrackVolume -= fadeOutSpeed * Time.deltaTime;
        trackAudioSource.volume = currentTrackVolume * masterVolume * musicVolume;
        if (currentTrackVolume <= 0.0f) {
            currentTrackVolume = 0.0f;
            trackAudioSource.Stop();
            fadingOut = false;
            if (fadeTargetTrack != null)
                StartFadeIn((TrackEntry)fadeTargetTrack);
        }
    }
    private void UpdateTrackVolume() {
        if (!trackAudioSource.isPlaying)
            return;

        trackAudioSource.volume = masterVolume * musicVolume * currentTrackEntryVolume;
    }


    private bool ApplyTrack(TrackEntry track) {
        AudioClip targetAudioClip = FindTrackAudioClip(track.key);
        if (!targetAudioClip)
            return false;

        trackAudioSource.clip = targetAudioClip;
        currentPlayingTrackKey = track.key;
        currentTrackEntryVolume = track.volume;
        trackAudioSource.volume = masterVolume * musicVolume * currentTrackEntryVolume;
        trackAudioSource.pitch = track.pitch;
        trackAudioSource.Play();

        return true;
    }
    private void StartFadeOut() {
        fadingOut = true;
    }
    private void StartFadeIn(TrackEntry track) {
        ApplyTrack(track);
        fadingIn = true;
    }



    private float GetRandomizedPitch(float min, float max) {
        if (min == max)
            return max;
        if (min > max)
            (max, min) = (min, max);

        return UnityEngine.Random.Range(min, max);
    }
    private AudioSource AddSoundUnit() {
        if (soundUnits.Count >= soundUnitsLimit) {
            Debug.LogWarning("Unable to add new audio source! \n Audio sources limit reached!");
            return null;
        }

        if (soundUnits.Count == 0)
            units = new GameObject("Units");

        var gameObject = new GameObject("AudioSource " + soundUnits.Count);
        var comp = gameObject.AddComponent<AudioSource>();
        gameObject.transform.SetParent(units.transform);
        comp.loop = false;
        comp.playOnAwake = false;
        soundUnits.Add(comp);
        return comp;
    }
    private void AddSFXRequest(GameObject owner, string key, AudioSource unit) {
        var newSFXRequest = new SFXRequest {
            unit = unit,
            key = key,
            owner = owner
        };
        SFXRequests.Add(newSFXRequest);
    }
    private AudioSource GetAvailableAudioSource() {
        if (soundUnits.Count == 0)
            return AddSoundUnit();

        foreach (var entry in soundUnits) {
            if (!entry.isPlaying)
                return entry;
        }

        return AddSoundUnit();
    }


    private AudioClip FindSFXAudioClip(string key) {
        if (key == null)
            return null;

        if (loadedSFXClips.Count == 0)
            return null;

        foreach (var entry in loadedSFXClips) {
            if (entry.Key == key) {
                if (entry.Value.Status == AsyncOperationStatus.Failed) {
                    Debug.LogError("Unable to find SFX clip '" + key + "' due to it being unsuccessfully loaded!");
                    return null;
                }
                return entry.Value.Result;
            }
        }

        return null;
    }
    private AudioClip FindTrackAudioClip(string key) {
        if (key == null)
            return null;

        if (loadedTracksClips.Count == 0)
            return null;

        foreach (var entry in loadedTracksClips) {
            if (entry.Key == key) {
                if (entry.Value.Status == AsyncOperationStatus.Failed) {
                    Debug.LogError("Unable to find Track clip '" + key + "' due to it being unsuccessfully loaded!");
                    return null;
                }
                return entry.Value.Result;
            }
        }

        return null;
    }
    private SFXEntry? FindSFXEntry(string key) {
        if (key == null)
            return null;

        if (sfxBundle.entries.Length == 0)
            return null;

        foreach (var entry in sfxBundle.entries) {
            if (entry.key == key) {
                return entry; //Need to get SFXEntry data and clip individually
            }
        }

        return null;
    }
    private TrackEntry? FindTrackEntry(string key) {
        if (key == null)
            return null;

        if (tracksBundle.entries.Length == 0)
            return null;

        foreach (var entry in tracksBundle.entries) {
            if (entry.key == key)
                return entry;
        }

        return null;
    }
    private SFXRequest? FindSFXRequest(GameObject owner, string key) {
        foreach (var entry in SFXRequests) {
            if (entry.key == key && entry.owner == owner)
                return entry;
        }

        return null;
    }

    private void UpdateSFXRequests() {
        List<SFXRequest> requests = new List<SFXRequest>();
        foreach (var entry in SFXRequests) { //??
            if (!entry.unit.isPlaying)
                requests.Add(entry);
        }

        foreach (var request in requests)
            SFXRequests.Remove(request);
    }
}
