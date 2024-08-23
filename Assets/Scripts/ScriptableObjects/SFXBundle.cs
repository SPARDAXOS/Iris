using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public struct SFXEntry {
    public string key;
    public AssetReferenceT<AudioClip> clip;
    [Range(0.0f, 1.0f)] public float volume;
    [Range(0.0f, 2.0f)] public float minPitch;
    [Range(0.0f, 2.0f)] public float maxPitch;
}

[CreateAssetMenu(fileName = "SFXBundle", menuName = "Data/SFXBundle", order = 9)]
public class SFXBundle : ScriptableObject {

    public SFXEntry[] entries = null;
}


