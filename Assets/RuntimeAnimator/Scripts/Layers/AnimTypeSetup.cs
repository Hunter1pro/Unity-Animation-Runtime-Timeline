using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ERigTamplate { Full, Move, Aim, Shot, WeaponRig}

[Serializable]
public class RigTamplate
{
    [SerializeField]
    private ERigTamplate tamplate;
    public ERigTamplate Tamplate => this.tamplate;

    [SerializeField]
    private List<Transform> activeObjects;
    public List<Transform> rigTransforms => this.activeObjects;
}

[Serializable]
public class AnimTypeRigView
{
    [SerializeField]
    private AnimType animType;
    public AnimType AnimType => this.animType;

    [SerializeField]
    private ERigTamplate tamplate;
    public ERigTamplate Tamplate => this.tamplate;

    private List<Transform> startRigs = new List<Transform>();
    private List<Transform> activeObjects;
    public List<Transform> rigTransforms => this.activeObjects;

    private List<Vector3> poss = new List<Vector3>();
    private List<Vector3> startPoss = new List<Vector3>();
    private List<Quaternion> rot = new List<Quaternion>();
    private List<Quaternion> startRot = new List<Quaternion>();

    private int initCount;
    private bool init;

    public void Active(bool value)
    {
        this.activeObjects.ForEach(transform =>
        {
            if (transform.TryGetComponent<Collider>(out Collider collider))
                collider.enabled = value;

            if(transform.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
            {
                meshRenderer.enabled = value;
            }
            else
            {
                if (transform.TryGetComponent<RigMeshLink>(out RigMeshLink rigMesh))
                {
                    rigMesh.MeshRenderer.enabled = value;
                }
                else
                {
                    Debug.Log($"NotFound");
                }
            }
        });
    }

    public void Init(List<Transform> activeObjects)
    {
        this.activeObjects = activeObjects;

        // For Double Init Fix 
        if (this.init) return;

        this.activeObjects.ForEach(x =>
        {
            this.poss.Add(x.localPosition);
            this.rot.Add(x.localRotation);
        });

        this.startRigs = this.activeObjects.ToList();
        this.startPoss = this.poss.ToList();
        this.startRot = this.rot.ToList();

        this.initCount = activeObjects.Count;

        this.init = true;
    }

    public void Reset()
    {
        for (int i = 0; i < this.activeObjects.Count; i++)
        {
            this.activeObjects[i].localPosition = this.poss[i];
            this.activeObjects[i].localRotation = this.rot[i];
        }
    }

    public void AddWeaponRig(List<Transform> weaponRig)
    {
        weaponRig.ForEach(x =>
        {
            this.rigTransforms.Add(x);
            this.poss.Add(x.localPosition);
            this.rot.Add(x.localRotation);
        });
    }

    public void ToStartRigs()
    {
        // TODO Check if this active is need

        //this.Active(false);
        this.activeObjects = this.startRigs.ToList();
        //this.Active(false);
        this.poss = this.startPoss.ToList();
        this.rot = this.startRot.ToList();
    }
}

public class AnimTypeSetup : MonoBehaviour
{
    [SerializeField]
    private ActionsPanel actionPanel;

    [SerializeField]
    private TimeLine timeLine;

    [SerializeField]
    private WeaponSetup weaponSetup;

    public AnimTypeRigView AnimTypeRig { get; private set; }

    [SerializeField]
    private List<WeaponRig> weaponRigs;

    private IAnimRuntimeDriver animDriver;

    public void Init(IAnimRuntimeDriver animDriver)
    {
        this.animDriver = animDriver;

        this.actionPanel.RemoveListenersAnimType();

        this.actionPanel.SubscribeChangedAnimType(animType =>
        {
            if (this.animDriver.AllRigs()[0].rigTransforms[0] == null) return;

            this.animDriver.AllRigs().ForEach(rig => rig.Active(false));

            this.AnimTypeRig = this.animDriver.AnimTypeRigViews().FirstOrDefault(x => x.AnimType == animType);
            this.AnimTypeRig.Active(true);

            this.animDriver.AllRigs().ForEach(x => x.Reset());

            this.timeLine.UpdateRigLayer(this.AnimTypeRig);

            if (this.weaponSetup.GetWeaponActionData() != null)
            {
                AnimAction rigAnim = this.weaponSetup.GetWeaponActionData().ActionsContainer.FirstOrDefault(y => y.Value.AnimType == AnimType.Rig).Value;

                if (rigAnim != null)
                {
                    this.animDriver.SetAnimTime(0, rigAnim, this.GetAnimType(AnimType.Rig));
                }
            }
        });

        this.weaponRigs.ForEach(x =>
        {
            this.AddWeaponRigsToAll(x.Rigs);
        });
    }

    public AnimTypeRigView GetAnimType(AnimType animType)
    {
        return this.animDriver.AnimTypeRigViews().FirstOrDefault(x => x.AnimType == animType);
    }

    public void ResetRigs()
    {
        this.animDriver.AllRigs().ForEach(x => x.Reset());
    }

    public void AddWeaponRigsToAll(List<Transform> weaponRigs)
    {
        this.animDriver.AllRigs().ForEach(x => x.AddWeaponRig(weaponRigs));
    }
}
