using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class AnimData
{
    public List<float3> Position { get; set; } = new List<float3>();
    public List<quaternion> Rotation { get; set; } = new List<quaternion>();

    public float Time { get; set; }

    public void SetupPoint(List<Transform> selected, float time)
    {
        this.Position.Clear();
        this.Rotation.Clear();

        selected.ForEach(x =>
        {
            this.Position.Add(x.localPosition);
            this.Rotation.Add(x.localRotation);
        });

        this.Time = time;
    }

    public void UpdateTime(float time)
    {
        this.Time = time;
    }

    public AnimData DeepCopy()
    {
        AnimData animData = (AnimData)this.MemberwiseClone();

        animData.Position = new List<float3>();
        animData.Rotation = new List<quaternion>();

        for (int i = 0; i < this.Position.Count; i++)
        {
            animData.Position.Add(this.Position[i]);
            animData.Rotation.Add(this.Rotation[i]);
        }

        return animData;
    }
}

// Before WeapnController
[DefaultExecutionOrder(-2)]
public class AnimationController : MonoBehaviour, IAnimationController
{
    [SerializeReference]
    private AnimRuntimeDriver animDriverRef;
    public IAnimRuntimeDriver AnimDriver => this.animDriverRef;

    [SerializeField]
    private bool rootMootionMove;
    [SerializeField]
    private Transform playerMotionNode;

    private WeaponActionsData coreActionData;

    private string weaponGuid;
    private WeaponRig weaponRigLoad;

    private Dictionary<AnimType, AnimationState> _animationStates =  new Dictionary<AnimType, AnimationState>();

    public void Init()
    {
        this.AnimDriver.Init(this);
    }

    public void SetupWeapon(string weaponGuid, WeaponRig weaponRig)
    {
        this.weaponGuid = weaponGuid;
        this.weaponRigLoad = weaponRig;

        this.coreActionData = AnimDriver.GetCoreActionData();

        var weaponAction = AnimDriver.GetWeaponActionData(weaponGuid);

        _animationStates.Clear();

        foreach (AnimType animType in Enum.GetValues(typeof(AnimType)))
        {
            if (weaponAction.ActionsContainer.Values.ToList().Exists(x => x.AnimType == animType))
                _animationStates.Add(animType, new AnimationState(AnimDriver, animType, weaponGuid, coreAction: coreActionData));
        }
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(this.weaponGuid)) return;

        this.SetupWeapon(this.weaponGuid, this.weaponRigLoad);
    }

    public void Play(AnimType animType, Action complite, Action<float2> speed)
    {
        _animationStates[animType].PlayRootMotion(complite, speed);
    }

    public void Play(AnimType animType, Action complite)
    {
        _animationStates[animType].Play(complite);
    }

    public void Play(AnimType animType)
    {
        _animationStates[animType].Play();
    }

    private void OnDestroy()
    {
        _animationStates.Values.ToList().ForEach(x => x.Stop());
    }
}

public interface IAnimationController
{
    void Load();
    void SetupWeapon(string weaponGuid, WeaponRig weaponRig);
    void Play(AnimType animType, Action complite, Action<float2> speed);
    void Play(AnimType animType, Action complite);
    void Play(AnimType animType);

}