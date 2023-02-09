using UnityEngine;

public class ResetupRigRoot : MonoBehaviour
{
    [SerializeField]
    private Transform rigRootFrom;

    private void Start()
    {
        Vector3 copyLocalPos = this.rigRootFrom.localPosition;
        Quaternion copyLocalRot = this.rigRootFrom.localRotation;
        this.rigRootFrom.parent = this.transform;
        this.rigRootFrom.localPosition = copyLocalPos;
        this.rigRootFrom.localRotation = copyLocalRot;
    }
    
}
