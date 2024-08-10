using System.Xml.XPath;
using UnityEngine;


public class CameraShakeType {
    public float intensity;
    public float duration;
    public Vector3 direction;
}

public class MainCamera : Entity {

    [SerializeField] private CameraStats cameraStats;

    private Player playerRef;
    private Daredevil daredevilData;
    




    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        initialized = true;
    }

    public override void Tick() {
        if (!initialized)
            return;



    }

    public override void FixedTick() {
        if (!initialized)
            return;



        UpdatePostion();
        UpdateRotation();
    }
    private void UpdatePostion() {
        Vector3 calculatedOffset = playerRef.transform.position + playerRef.transform.rotation * cameraStats.positionOffset;
        transform.position = Vector3.Lerp(transform.position, calculatedOffset, cameraStats.followSpeed * Time.fixedDeltaTime);
    }
    private void UpdateRotation() {
        transform.forward = playerRef.transform.forward;
    }







    //private void UpdatePositionAndRotation(Transform transform1, Vector3 offset)
    //{
    //    transform.position = Vector3.Lerp(
    //        transform.position,
    //        transform1.position + offset,
    //        cameraStats.GetCameraFollowSpeed() * Time.deltaTime
    //    );
    //    transform.LookAt(transform1);
    //}
    ////private Vector3 CalculateOffset(out Transform transform1)
    //{
    //    var cameraValuesYOffSet = cameraStats.GetYOffset();
    //    var zOffset = Mathf.Lerp(cameraStats.GetMinCameraZOffset(), cameraStats.GetMaxCameraZOffset(),
    //        daredevilData.GetCurrentSpeedPercentage());
    //    var offset = new Vector3(0f, cameraValuesYOffSet, zOffset);
    //    transform1 = playerRef.transform;
    //    return offset;
    //}
    ////public void TriggerShake(CameraShakeType cameraShakeType)
    //{
    //    _shakeCamera = true;
    //    _shakeDuration = cameraShakeType.Duration;
    //    this.cameraShakeType = cameraShakeType;
    //    originalRotation = _cameraTransform.localRotation;
    //}
    ////private void ShakeCamera()
    //{
    //    if (!_shakeCamera)
    //        return;
    //
    //    Quaternion randomDisplacement;
    //    var direction = cameraShakeType.Direction;
    //    var shakeIntensity = cameraShakeType.Intensity;
    //    if (direction == Vector3.up || direction == Vector3.down)
    //    {
    //        randomDisplacement = Quaternion.Euler(Random.Range(-shakeIntensity, shakeIntensity), 0f, 0f);
    //    }
    //    else if (direction == Vector3.left || direction == Vector3.right)
    //    {
    //        randomDisplacement = Quaternion.Euler(0f, Random.Range(-shakeIntensity, shakeIntensity), 0f);
    //    }
    //    else
    //    {
    //        randomDisplacement = Quaternion.Euler(Random.insideUnitSphere * shakeIntensity);
    //    }
    //
    //    _cameraTransform.localRotation = originalRotation * randomDisplacement;
    //
    //    _shakeDuration -= Time.deltaTime;
    //
    //    if (!(_shakeDuration <= 0))
    //        return;
    //
    //    _shakeCamera = false;
    //    _cameraTransform.localRotation = originalRotation;
    //}







    public void SetPlayerReference(Player player)
    {
        playerRef = player;
        daredevilData = playerRef.GetDaredevilData();
    }
}