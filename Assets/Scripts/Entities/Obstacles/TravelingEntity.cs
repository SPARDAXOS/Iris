using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelingEntity : Obstacle
{
    [SerializeField] private Vector3 endLocation;
    [SerializeField] private float movementSpeed = 1.0f;
    [SerializeField] private float resetTime = 10.0f;
    private Vector3 startingLocation = Vector3.zero;
    private float timer = 0.0f;
    
    public override void Initialize(GameInstance game)
    {
        startingLocation = transform.position;
    }

    private void Tick()
    {
        float temp = Vector3.Distance(startingLocation, endLocation);
        PatrolMovement(endLocation, movementSpeed);
        if(temp < 0.1) 
        {
            if(timer >= resetTime)
            {
                timer = 0.0f;
                ResetPatrol();
            }
            else
            {
                timer += Time.deltaTime;
            }
           
            
        }
    }
    private void ResetPatrol()
    {
        transform.position = startingLocation;
    }
    private void PatrolMovement(Vector3 destination, float interpolationRatio)
    {
        Vector3 postionCalculation = Vector3.Lerp(transform.position, destination, interpolationRatio * Time.deltaTime);
        transform.position = postionCalculation;
        float positionDistance = Vector3.Distance(postionCalculation, destination);
        transform.forward = destination - transform.position;
    }


}
