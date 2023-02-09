using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using static WeaponController;

public enum WeaponRootPointName { None, HandL, HandR, Chesh, HipsL, HipsR };

[Serializable]
public class WeaponRootPoint
{
    [SerializeField]
    private WeaponRootPointName rootName;
    public WeaponRootPointName RootName => rootName;

    [SerializeField]
    private Transform rootPoint;
    public Transform RootPoint => rootPoint;
}

[Serializable]
public class WeaponView
{
    [SerializeField]
    private string guid;
    public string GuidString => guid;

    public void InitGuid()
    {
        if (!string.IsNullOrEmpty(guid)) 
            return;

        guid = Guid.NewGuid().ToString();
    }

    [SerializeField]
    private string displayName;
    public string DisplayName => displayName;

    [SerializeField]
    private WeaponType weaponType;
    public WeaponType WeaponType => weaponType;

    [SerializeField]
    private GameObject weaponPrefab;
    public GameObject WeaponPrefab => weaponPrefab;

    [SerializeField]
    private WeaponRootPointName handRoot;
    public WeaponRootPointName HandRoot => handRoot;

    [SerializeField]
    private WeaponRootPointName slotRoot;
    public WeaponRootPointName SlotRoot => slotRoot;

    private Transform shotPoint;
    public Transform ShotPoint => shotPoint;

    private WeaponRig weaponRig;
    public WeaponRig WeaponRig => weaponRig;
}

public interface IWeaponView
{
    string GuidString { get; }
    string DisplayName { get; }
    bool IsShow { get; }
    WeaponType WeaponType { get; }
    WeaponRootPointName HandRoot { get; }
    WeaponRootPointName SlotRoot { get; }
    GameObject WeaponPrefab { get; }
    Transform WeaponInstance { get; }
    Transform ShotPoint { get; }
    WeaponRig WeaponRig { get; }

    void Init(WeaponRootPoint handRoot, WeaponRootPoint slotRoot);
    void Show(bool value);
    void SetupView(bool hand);
}

public class WeaponController : MonoBehaviour, IWeaponSetup
{
    public enum WeaponType { None, Rifel, ShotGun, PredatorBow, Pole, Knife }

    [SerializeField]
    private WeaponSelector startWeaponType;
    private string currentWeaponTypeId;

    private WeaponType lastPickUpWeaponType;
    private UnityAction destroyPickUpAction;

    private IWeaponSelectorComp weaponSelectorComp;

    [SerializeField]
    private List<WeaponRootPoint> weaponRootPoints;

    private IAnimationController animationController;

    private Action<string> a_WeaponType;

    private IWeaponView currentWeaponData;

    public void Init(List<WeaponView> weaponViews)
    {
        animationController = GetComponent<IAnimationController>();

        weaponSelectorComp = new WeaponSelectorComp(weaponViews.Where(x => x.GuidString == startWeaponType.CurrentWeapon().ToString()).ToList(), weaponRootPoints);

        Init();
    }

    public void Reinit()
    {
        Init();
    }

    public void SetupEditorWeaponType(string waeponGuid)
    {
        this.SetupWeaponType(waeponGuid);
    }

    private void Init()
    {
        SetupWeaponType(startWeaponType.CurrentWeapon());
    }

    public void SetupWeaponType(string weaponGuid)
    {
        currentWeaponTypeId = weaponGuid;
        IWeaponView weaponView = weaponSelectorComp.GetWeaponSelector(weaponGuid);
        this.currentWeaponData = weaponView;
        weaponSelectorComp.Show(currentWeaponTypeId, true);

        animationController.SetupWeapon(weaponView.GuidString, weaponView.WeaponRig);

        Debug.Log($"Name {name} WeaponName {weaponView.DisplayName} weaponType {this.currentWeaponData.WeaponType}");

        a_WeaponType?.Invoke(weaponGuid);
    }
}

public interface IWeaponSetup
{
    void SetupWeaponType(string weaponGuid);
}
