using UnityEngine;

[ExecuteInEditMode]
public class CopyTransform : MonoBehaviour
{
    [SerializeField]
    private Transform copyTo;

    [SerializeField]
    private bool notPosZ;

    [SerializeField]
    private bool rotation;

    private void OnValidate()
    {
        if (copyTo == null) return;

        if (!this.notPosZ)
        {
            this.copyTo.position = this.transform.position;
        }
        else
        {
            this.copyTo.position = new Vector3(this.transform.position.x, this.transform.position.y, this.copyTo.position.z);
        }

        this.copyTo.rotation = this.transform.rotation;
    }

    public void Setup(Transform copyTo, bool notPosZ, bool rotation = true)
    {
        this.copyTo = copyTo;
        this.notPosZ = notPosZ;
        this.rotation = rotation;
    }

    private void Update()
    {
        if (copyTo == null) return;

        if (!this.notPosZ)
        {
            this.copyTo.position = this.transform.position;
        }
        else
        {
            this.copyTo.position = new Vector3(this.transform.position.x, this.copyTo.position.y, this.copyTo.position.z);
        }

        if (this.rotation && this.transform.rotation != this.copyTo.rotation)
        {
            this.copyTo.rotation = this.transform.rotation;
        }
    }
}
