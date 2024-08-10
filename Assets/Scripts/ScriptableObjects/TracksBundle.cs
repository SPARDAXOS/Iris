using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public struct TrackEntry {
    public string key;
    public AssetReference clip;
    [Range(0.01f, 1.0f)] public float volume;
    [Range(-3.0f, 3.0f)] public float pitch;
}
[CreateAssetMenu(fileName = "TracksBundle", menuName = "Sound/TracksBundle", order = 0)]
public class TracksBundle : ScriptableObject {

    [SerializeField] public TrackEntry[] entries;
}
