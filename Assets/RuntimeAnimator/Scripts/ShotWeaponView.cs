using UnityEngine;

public class ShotWeaponView : MonoBehaviour
{
    [SerializeField]
    private Transform shotPoint;
    public Transform ShotPoint => shotPoint;
}
