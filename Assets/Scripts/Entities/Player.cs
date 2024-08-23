using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UIElements;
using static MyUtility.Utility;

public class Player : Entity {
    public enum PlayerID {
        NONE = 0,
        PLAYER_1,
        PLAYER_2
    }

    [Header("Health")]
    [Space(10)]
    [SerializeField] private float healthCap = 100.0f;
    [SerializeField] private float startingHealth = 100.0f;

    [Header("Movement")]
    [Space(10)]
    [SerializeField] private float accelerationSpeed = 1.0f;
    [SerializeField] private float decelerationSpeed = 1.0f;
    [SerializeField] private float maxSpeed = 100.0f;

    private PlayerID currentPlayerID = PlayerID.NONE;
    private bool active = true;

    private SpriteRenderer spriteRendererRef;
    private Animator animatorRef;
    private Rigidbody2D rigidbody2DRef;
    private BoxCollider2D boxCollider2DRef;
    private NetworkObject networkObjectRef;

    private MainHUD mainHUDRef;

    public float currentHealth = 0.0f;
    public float currentSpeed = 0.0f;

    public Vector2 inputDirection = Vector2.zero;
    public float horizontalVelocity = 0.0f;
    public float verticalVelocity = 0.0f;

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

        SetupReferences();

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
            if (Netcode.IsClient() && !Netcode.IsHost()) {
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
        //animatorRef.SetBool("isDead", false);
        rigidbody2DRef.constraints = RigidbodyConstraints2D.FreezeRotation;

        //mainHUDRef.UpdatePlayerHealth(GetCurrentHealthPercentage(), currentPlayerID);
        //mainHUDRef.UpdatePlayerMoneyCount(currentMoney, currentPlayerID);
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
    private void SetupReferences() {


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
        bool jump = Input.GetKeyDown(KeyCode.W);
        bool shoot = Input.GetKey(KeyCode.Space);



        if (left && right)
            inputDirection.x = 0.0f;
        else if (left)
            inputDirection.x = -1.0f;
        else if (right)
            inputDirection.x = 1.0f;
        else
            inputDirection.x = 0.0f;

        UpdateSpriteOrientation(inputDirection.x);
        

        if (Netcode.IsHost()) {
            movingLeft = left;
            movingRight = right;
            //if (inputDirection.x != 0.0f && !animatorRef.GetBool("isMoving"))
            //    SetMovementAnimationState(true);
            //else if (inputDirection.x == 0.0f && animatorRef.GetBool("isMoving"))
            //    SetMovementAnimationState(false);
        }
        else if (Netcode.IsClient() && !Netcode.IsHost()) {

            //if (inputDirection.x != 0.0f && !animatorRef.GetBool("isMoving"))
            //    gameInstanceRef.GetRPCManagement().NotifyMovementAnimationStateServerRpc(true);
            //else if (inputDirection.x == 0.0f && animatorRef.GetBool("isMoving"))
            //    gameInstanceRef.GetRPCManagement().NotifyMovementAnimationStateServerRpc(false);
        }
    }

    public void SetMovementAnimationState(bool state) {
        animatorRef.SetBool("isMoving", state);
    }
    private void UpdateSpeed() {
        if (movingRight || movingLeft)
            Accelerate();
        else
            Decelerate();
    }
    private void UpdateMovement() {
        horizontalVelocity = (inputDirection * currentSpeed).x;
        verticalVelocity = rigidbody2DRef.velocity.y; //? weird spot
        rigidbody2DRef.velocity = new Vector2(horizontalVelocity, verticalVelocity);
    }
    private void UpdateSpriteOrientation(float input) {
        if (input == 0.0f)
            return;

        RPCManagement management = gameInstanceRef.GetRPCManagement();
        if (input > 0.0f && spriteRendererRef.flipX) {
            spriteRendererRef.flipX = false;
            management.UpdateSpriteOrientationServerRpc(spriteRendererRef.flipX, Netcode.GetClientID());
        }
        else if (input < 0.0f && !spriteRendererRef.flipX) {
            spriteRendererRef.flipX = true;
            management.UpdateSpriteOrientationServerRpc(spriteRendererRef.flipX, Netcode.GetClientID());
        }
    }


    public void OverrideCurrentHealth(float percentage) { currentHealth = percentage * healthCap;}
    public void TakeDamage(float damage) {
        if (damage == 0.0f)
            return;

        float value = damage;
        if (value < 0.0f)
            value *= -1.0f;

        RPCManagement management = gameInstanceRef.GetRPCManagement();
        currentHealth -= value;
        if (currentHealth <= 0.0f) {
            currentHealth = 0.0f;
            //management.UpdatePlayerDeathServerRpc(currentPlayerID, Netcode.GetClientID());
            Kill();
        }
        mainHUDRef.UpdatePlayerHealth(GetCurrentHealthPercentage(), currentPlayerID);

        management.UpdatePlayerHealthServerRpc(GetCurrentHealthPercentage(), currentPlayerID, Netcode.GetClientID());
    }
    public void Kill() {
        //if (isDead)
        //    return;

        //isDead = true;
        //animatorRef.SetTrigger("deathTrigger");
        //animatorRef.SetBool("isDead", true);
        //gameInstanceRef.GetSoundSystem().PlaySFX("Death");

        //RPCManagement management = gameInstanceRef.GetRPCManagement();
        //management.UpdatePlayerMoneyServerRpc(currentMoney, currentPlayerID, Netcode.GetClientID());
    }


    public void ProcessSpriteOrientationRpc(bool flipX) {
        spriteRendererRef.flipX = flipX;
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
    public void SetMainHUDRef(MainHUD reference) { mainHUDRef = reference; }
    public float GetCurrentHealth() { return currentHealth; }
    public float GetCurrentHealthPercentage() { return currentHealth / healthCap; }
}
