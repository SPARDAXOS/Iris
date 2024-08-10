using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CameraStats", menuName = "Camera/CameraStats", order = 0)]
public class CameraStats : ScriptableObject {
    [SerializeField] public float followSpeed;
    [SerializeField] public Vector3 positionOffset = new Vector3(0.0f, 1.2f, -3.1f);


}