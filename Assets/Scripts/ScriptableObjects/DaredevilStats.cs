using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DaredevilStats", menuName = "Player/DaredevilStats", order = 0)]
public class DaredevilStats : ScriptableObject {

    [Header("Speed Settings")]
    [Tooltip("Speed of acceleration")]
    [Range(0.1f, 500.0f)][SerializeField] public float accelerationRate = 200.0f;

    [Tooltip("Speed of deceleration")]
    [Range(0.1f, 500.0f)][SerializeField] public float decelerationRate = 150.0f;

    [Tooltip("Maximum amount of speed attainable")]
    [Range(0.1f, 1000.0f)][SerializeField] public float maxSpeed = 300.0f;
    [Range(0.1f, 5000.0f)][SerializeField] public float maxReverseSpeed = 100.0f;
    [Range(0.1f, 1000.0f)][SerializeField] public float reverseRate = 50.0f;
    [Range(0.1f, 1000.0f)][SerializeField] public float brakeRate = 200.0f;


    [Tooltip("How much you turn left and right each turning action")]
    //[Range(0.1f, 5000.0f)][SerializeField] public float turnRate = 500.0f;
    [Range(0.1f, 1000.0f)][SerializeField] public float rotationCorrectionRate = 5.0f;


    [Header("Gyroscope")]
    //Try to keep the ranges equal for best results.
    [Range(0.1f, 10.0f)][SerializeField] public float midSpectrumSize = 3.0f; //Remove
    [Range(0.1f, 10.0f)][SerializeField] public float leftSpectrumSize = 3.0f; //Remove
    [Range(0.1f, 10.0f)][SerializeField] public float rightSpectrumSize = 3.0f; //Remove

    [Range(0.1f, 10.0f)][SerializeField] public float gyroSensitivity = 1.0f; //Remove
    [Range(0.01f, 500.0f)][SerializeField] public float gyroTiltRate = 50.0f; //Remove
    [Range(0.01f, 2.0f)][SerializeField] public float gyroResetDuration = 0.5f; //Remove
    [Range(0.01f, 1.0f)][SerializeField] public float driftRate = 0.1f; //Remove


    [Header("Speed Boost Settings")]
    [Tooltip("The multiplied increase acceleration")]
    [Range(0.1f, 500.0f)][SerializeField] public float maxBoostSpeed = 300.0f;
    [Range(0.1f, 500.0f)][SerializeField] public float boostAccelerationRate = 300.0f;
    [Range(0.1f, 500.0f)][SerializeField] public float boostDecelerationRate = 400.0f;


    [Range(0.1f, 1000.0f)][SerializeField] public float gravity = 400.0f;
    [Range(0.1f, 90.0f)][SerializeField] public float terrainAdjustmentAngle = 45.0f; //Remove



}
