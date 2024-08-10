using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Obstacle base class for types of dynamic obstacles.
/// </summary>
public class Obstacle : Entity {
    public enum ObstacleActivationState {
        NONE = 0,
        RED,
        BLUE
    }

    [SerializeField] protected ObstacleActivationState assignedActivationState = ObstacleActivationState.NONE;


    public bool activated = false;

    public override void Initialize(GameInstance game) { //Called by level
        if (initialized)
            return;


        gameInstanceRef = game;
        initialized = true;
    }

    public ObstacleActivationState GetObstacleActivationState() { return assignedActivationState; }
    public void SetActivationState(bool state) { 
        activated = state; 
        transform.Find("Mesh").gameObject.SetActive(state); //TEMP
    }
}
