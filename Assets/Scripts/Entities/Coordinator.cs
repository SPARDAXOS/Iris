using Initialization;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MyUtility.Utility;

public class Coordinator
{


    bool initialized;

    //private bool speedBoostBool = false;
    //private bool boostOnCooldown = false;
    //private int boostCharges = 0;
    //private float boostTimer = 0.0f;
    //private float boostCooldown = 0.0f;


    private float currentBattery = 0.0f;
    private bool usingBoost = false;


    GameInstance gameInstanceRef;
    Player playerRef;
    CoordinatorStats stats;


    public void Initialize(GameInstance game, Player player)
    {
        if (initialized)
            return;



        playerRef = player;
        stats = playerRef.GetCoordinatorStats();
        gameInstanceRef = game;
        initialized = true;
    }
    public void Tick()
    {
        if (!initialized)
        {
            Warning("Player attempted to tick Coordinator while it was not initialized");
            return;
        }


        CheckBoostState();
    }
    public void FixedTick()
    {
        if (!initialized)
        {
            Warning("Player attempted to fixed tick Coordinator while it was not initialized");
            return;
        }

    }
    public void SetupStartState() {
        currentBattery = stats.batteryLimit;
        usingBoost = false;
    }



    private void CheckBoostState() {
        if (!usingBoost)
            return;

        Log("Current Battery: " + currentBattery / stats.batteryLimit);
        currentBattery -= stats.boostPowerCost * Time.deltaTime;
        if (currentBattery <= 0.0f) {
            currentBattery = 0.0f;
            usingBoost = false;
            playerRef.GetCoordinatorHUD().RelayBoostState(usingBoost);
        }

        playerRef.GetCoordinatorHUD().UpdatePowerBar(currentBattery / stats.batteryLimit);
    }


    public bool SetBoostState(bool state) {
        if (currentBattery >= stats.boostPowerCost) {
            usingBoost = state;
            return true;
        }
        else
            return false;
    }


    public CoordinatorStats GetStats() { return stats; }









    //private void RestoreCharges() //call this before speedBoost
    //{
    //    //boostCharges = stats.GetBoostCharges();
    //}

    //private void SpeedBoost()
    //{
    //    if (boostCharges <= 0 || speedBoostBool || boostOnCooldown)
    //        return;


    //    boostCharges -= 1;
    //    boostOnCooldown = true;
    //    speedBoostBool = true;
    //    playerRef.SetBoostCheck(speedBoostBool);
    //    //boostCooldown = stats.GetboostCooldown();
    //    //boostTimer = stats.GetBoostDuration();


    //}
    //private void BoostTimer()
    //{
    //    if (boostTimer <= 0.0f)
    //        return;

    //    boostTimer -= Time.deltaTime;
    //    if (boostTimer <= 0.0f)
    //    {
    //        boostTimer = 0.0f;
    //        speedBoostBool = false;
    //        playerRef.SetBoostCheck(speedBoostBool);

    //    }

    //}

    //private void ResetBoostCooldown()
    //{
    //    if (boostCooldown <= 0.0f)
    //    {
    //        return;
    //    }

    //    boostCooldown -= Time.deltaTime;
    //    if (boostCooldown <= 0.0f)
    //    {
    //        boostCooldown = 0.0f;
    //        boostOnCooldown = false;
    //    }

    //}
}
