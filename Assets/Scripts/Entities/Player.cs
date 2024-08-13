using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.UI;
using static MyUtility.Utility;

public class Player : Entity {

    [SerializeField] private float healthCap = 100.0f;
    [SerializeField] private float startingHealth = 100.0f;
    [SerializeField] private float accelerationSpeed = 1.0f;
    [SerializeField] private float decelerationSpeed = 1.0f;
    [SerializeField] private float maxSpeed = 100.0f;




    public enum PlayerID {
        NONE = 0,
        PLAYER_1,
        PLAYER_2
    }

    private PlayerID currentPlayerID = PlayerID.NONE;
    private bool active = true;

    private SpriteRenderer spriteRendererRef;
    private Animator animatorRef;
    private Rigidbody2D rigidbody2DRef;
    private BoxCollider2D boxCollider2DRef;
    private NetworkObject networkObjectRef;

    public float currentHealth = 0.0f;
    public float currentSpeed = 0.0f;

    public Vector2 inputDirection = Vector2.zero;
    public Vector2 velocity = Vector2.zero;

    public bool movingRight = false;
    public bool movingLeft = false;


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        spriteRendererRef = GetComponent<SpriteRenderer>();
        animatorRef = GetComponent<Animator>();
        rigidbody2DRef = GetComponent<Rigidbody2D>();
        boxCollider2DRef = GetComponent<BoxCollider2D>();
        networkObjectRef = GetComponent<NetworkObject>();

        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized || !active)
            return;
        if (currentPlayerID == PlayerID.NONE)
            return;

        if (networkObjectRef.IsOwner)
            CheckInput();

    }
    public override void FixedTick() {
        if (!initialized || !active)
            return;
        if (currentPlayerID == PlayerID.NONE)
            return;


        if (networkObjectRef.IsOwner) {
            Netcode netcodeRef = gameInstanceRef.GetNetcode();
            if (netcodeRef.IsClient() && !netcodeRef.IsHost()) {
                gameInstanceRef.GetRPCManagement().CalculatePlayer2PositionServerRpc(inputDirection.x);
            }
        }

        UpdateSpeed();
        UpdateMovement();
    }
    public void SetupStartingState() {
        if (!initialized)
            return;

        currentHealth = startingHealth;
        //Health reset, etc
    }
    public void SetNetworkedEntityState(bool state) {
        active = state;
        if (state) {
            spriteRendererRef.enabled = true;
            rigidbody2DRef.WakeUp();
            boxCollider2DRef.enabled = true;
            animatorRef.enabled = true;
        }
        else if (!state) {
            spriteRendererRef.enabled = false;
            rigidbody2DRef.Sleep();
            boxCollider2DRef.enabled = false;
            animatorRef.enabled = false;
        }
    }



    private void Accelerate() {
        if (currentSpeed >= maxSpeed)
            return;

        currentSpeed += accelerationSpeed * Time.fixedDeltaTime;
        if (currentSpeed >= maxSpeed) {
            currentSpeed = maxSpeed;

        }
    }
    private void Decelerate() {
        if (currentSpeed <= 0.0f)
            return;

        currentSpeed -= decelerationSpeed * Time.fixedDeltaTime;
        if (currentSpeed <= 0.0f) {
            currentSpeed = 0.0f;

        }
    }
    private void CheckInput() {
        bool left = Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.D);
        if (gameInstanceRef.GetNetcode().IsHost()) {
            movingLeft = left;
            movingRight = right;
        }

        if (left && right)
            inputDirection.x = 0.0f;
        else if (left)
            inputDirection.x = -1.0f;
        else if (right)
            inputDirection.x = 1.0f;
        else
            inputDirection.x = 0.0f;
    }
    private void UpdateSpeed() {
        if (movingRight || movingLeft)
            Accelerate();
        else
            Decelerate();
    }
    private void UpdateMovement() {
        velocity = inputDirection * currentSpeed;
        rigidbody2DRef.velocity = velocity;
    }


    public void ProcessMovementInputRpc(float input) {

        inputDirection.x = input;
        if (input == 0.0f) {
            movingLeft = false;
            movingRight = false;
        }
        else if (input < 0.0f) {
            movingLeft = true;
            movingRight = false;
        }
        else if (input > 0.0f) {
            movingLeft = false;
            movingRight = true;
        }

    }


    public PlayerID GetPlayerID() { return currentPlayerID; }
    public void SetPlayerID(PlayerID id) { currentPlayerID = id; }
    public float GetCurrentHealth() { return currentHealth; }
    public float GetCurrentHealthPercentage() { return currentHealth / healthCap; }


}
