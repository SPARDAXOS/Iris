using Unity.VisualScripting;
using UnityEngine;
using static MyUtility.Utility;




public struct HitstopData {
    private float hitstop; //?

    public HitstopData(float cameraShakeIntensity, float cameraShakeDuration) { //?
        hitstop = cameraShakeIntensity / cameraShakeDuration;
    }
}


public class Player : NetworkedEntity {
    public enum Identity {
        NONE = 0,
        DAREDEVIL = 1, //Daredevil
        COORDINATOR = 2 //Coordinator
    }

    [SerializeField] private DaredevilStats daredevilStats;
    [SerializeField] private CoordinatorStats coordinatorStats;

    private Identity assignedPlayerIdentity = Identity.NONE;

    private Daredevil daredevilData = new Daredevil();
    private Coordinator coordinatorData = new Coordinator();

    private CoordinatorHUD coordinatorHUD;
    private DaredevilHUD daredevilHUD;

    private Rigidbody rigidbodyComp;

    private CapsuleCollider capsuleColliderComp;

    private bool speedBoostBool = false;


    public override void Initialize(GameInstance game)
    {
        if (initialized)
            return;


        SetupReference();

        daredevilData.Initialize(game, this);
        coordinatorData.Initialize(game, this);

        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized) {
            Warning("Attempted to tick player before it was initialized!");
            return;
        }

        if (assignedPlayerIdentity == Identity.DAREDEVIL)
        {
            daredevilData.Tick();
            daredevilHUD.Tick();
        }
        else if (assignedPlayerIdentity == Identity.COORDINATOR)
        {
            coordinatorData.Tick();
            coordinatorHUD.Tick();
        }
    }
    public override void FixedTick() {
        if (!initialized) {
            Warning("Attempted to fixed tick player before it was initialized!");
            return;
        }

        if (assignedPlayerIdentity == Identity.DAREDEVIL) {
            daredevilData.FixedTick();
            daredevilHUD.FixedTick();
        }
        else if (assignedPlayerIdentity == Identity.COORDINATOR) {
            coordinatorData.FixedTick();
            coordinatorHUD.FixedTick();
        }
    }
    private void SetupReference()
    {
        rigidbodyComp = GetComponent<Rigidbody>();
        Validate(rigidbodyComp, "Failed to get reference to Rigidbody component!", ValidationLevel.ERROR, true);

        capsuleColliderComp = GetComponent<CapsuleCollider>();
        Validate(capsuleColliderComp, "Failed to get reference to CapsuleCollider component!", ValidationLevel.ERROR, true);
    }
    public void SetupStartState() {
        if (assignedPlayerIdentity == Identity.DAREDEVIL) { //Order matters due to stats being reset in data then HUD using those stats.
            daredevilData.SetupStartState();
            daredevilHUD.SetupStartState();
        }
        else if (assignedPlayerIdentity == Identity.COORDINATOR) {
            coordinatorData.SetupStartState();
            coordinatorHUD.SetupStartState();
        }
    }




    public void AssignPlayerIdentity(Identity playerIdentity) { assignedPlayerIdentity = playerIdentity; }
    public void SetDaredevilHUD(DaredevilHUD hud) { daredevilHUD = hud; }
    public void SetCoordinatorHUD(CoordinatorHUD hud) { coordinatorHUD = hud; }


   




    public Identity GetPlayerIdentity() { return assignedPlayerIdentity; }

    public DaredevilStats GetDaredevilStats() { return daredevilStats; }
    public CoordinatorStats GetCoordinatorStats() { return coordinatorStats; }
    public DaredevilHUD GetDaredevilHUD() { return daredevilHUD; }
    public CoordinatorHUD GetCoordinatorHUD() { return coordinatorHUD; }
    public Daredevil GetDaredevilData() { return daredevilData; }
    public Coordinator GetCoordinatorData() { return coordinatorData; }

    public Rigidbody GetRigidbody() { return rigidbodyComp; }
    public CapsuleCollider GetCapsuleCollider() { return capsuleColliderComp; }

    //?
    public bool GetBoostCheck() { return speedBoostBool; }
    public void SetBoostCheck(bool boostCheck) {  speedBoostBool = boostCheck; }
}
