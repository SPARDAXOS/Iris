using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CoordinatorStats", menuName = "Player/CoordinatorStats", order = 0)]
public class CoordinatorStats : ScriptableObject {

    [Range(0.1f, 100.0f)][SerializeField] public float batteryLimit = 1.0f;

    [Range(0.1f, 100.0f)][SerializeField] public float boostPowerCost = 1.0f;
    [Range(0.1f, 100.0f)][SerializeField] public float doorOpeningPowerCost = 1.0f;


    //[Range(0.1f, 500.0f)][SerializeField] public float boostDuration = 1.0f;
    //[Range(0.1f, 500.0f)][SerializeField] public float boostCooldown = 1.0f;
    //[Tooltip("The amount of boost charges")]
    //[Range(1, 500)][SerializeField] public int boostCharges = 1;







}
