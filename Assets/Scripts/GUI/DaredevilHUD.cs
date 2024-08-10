using UnityEngine;

public class DaredevilHUD : Entity {

    public enum DaredevilKeyCode
    {
        MoveLeft = 0,
        MoveRight = 1,
        Accelerate = 2,
        Decelerate = 3
    }
    
    private Player playerRef;
    private Daredevil daredevilRef;

    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        initialized = true;
    }
    public void SetupStartState() {

    }


    public void SetPlayerReference(Player player) {
        playerRef = player;
        if (playerRef)
            daredevilRef = playerRef.GetDaredevilData();
    }


  


    public void BrakeOnEvent() {
        daredevilRef.SetBrakeState(true);
    }
    public void BrakeOffEvent() {
        daredevilRef.SetBrakeState(false);
    }
    public void GasOnEvent() {
        daredevilRef.SetMovementState(true);
    }
    public void GasOffEvent() {
        daredevilRef.SetMovementState(false);
    }
}
