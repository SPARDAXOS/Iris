using System;
using UnityEngine;

public class DoorMovement : Obstacle 
{






    public enum ObstacleState { ERROR = 0, 
        ACTIVE, 
        INACTIVE 
    }


    [SerializeField] Vector3 openedPosition;
    [SerializeField] Vector3 openedRotation;

    [Range(0.1f, 200f)][SerializeField] float openingSpeed;
    [SerializeField] float correctionThreshold;


    public bool moving = false;

    Vector3 initialPosition;
    Vector3 initialRotation;
    ObstacleState doorState = ObstacleState.ERROR;



   

    public override void Initialize(GameInstance game)
    {
        //TODO: To func! - Negative rotation requires testing
        doorState = ObstacleState.INACTIVE;
        initialRotation = gameObject.transform.rotation.eulerAngles;
        initialPosition = gameObject.transform.position;

    }
    public override void Tick() {
        if (doorState == ObstacleState.ACTIVE && moving)
        {
            DoorMove(openedPosition, openedRotation);
        }
        else if (doorState == ObstacleState.INACTIVE && moving)
        {
            DoorMove(initialPosition, initialRotation);
        }
    }

    public void StateChange(ObstacleState state)
    {
        if (state == ObstacleState.ERROR) {
            Debug.Log("Door Error");
            return;
        }

        doorState = state;
        moving = true;
    }
    private void DoorMove(Vector3 destination, Vector3 toRotation)
    {
        Vector3 postionCalculation = Vector3.Lerp(transform.position, destination, openingSpeed * Time.deltaTime);
        transform.position = postionCalculation;
        Vector3 rotationCalculation = Vector3.Lerp(transform.eulerAngles, toRotation, openingSpeed * Time.deltaTime);
        transform.eulerAngles = rotationCalculation;
        float positionDistance = Vector3.Distance(postionCalculation, destination);
        float rotationDistance = Vector3.Distance(rotationCalculation, toRotation);
        if (positionDistance < correctionThreshold && rotationDistance < correctionThreshold)
        {
            transform.position = destination;
            transform.eulerAngles = toRotation;
            moving = false;
        }
    }
}
