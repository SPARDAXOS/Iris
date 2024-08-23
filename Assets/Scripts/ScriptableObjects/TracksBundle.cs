using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public struct TrackEntry {
    public string key;
    public AssetReferenceT<AudioClip> clip;
    [Range(0.0f, 1.0f)] public float volume;
    [Range(0.0f, 2.0f)] public float pitch;
}

[CreateAssetMenu(fileName = "TracksBundle", menuName = "Data/TracksBundle", order = 10)]
public class TracksBundle : ScriptableObject {
    public TrackEntry[] entries = null;
}


