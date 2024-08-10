using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public struct SFXEntry {
    public string key;
    public AssetReference clip;
    [Range(0.01f, 1.0f)] public float volume;
    [Range(-3.0f, 3.0f)] public float minPitch;
    [Range(-3.0f, 3.0f)] public float maxPitch;
}
[CreateAssetMenu(fileName = "SFXBundle", menuName = "Sound/SFXBundle", order = 1)]
public class SFXBundle : ScriptableObject {

    [SerializeField] public SFXEntry[] entries;
}
