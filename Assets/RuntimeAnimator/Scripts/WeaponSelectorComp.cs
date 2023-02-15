using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static WeaponController;

public class WeaponControllView : IWeaponView
{
    public string GuidString { get; private set; }

    public string DisplayName { get; private set; }

    public bool IsShow { get; private set; }

    public WeaponType WeaponType { get; private set; }

    public WeaponRootPointName HandRoot { get; private set; }

    public WeaponRootPointName SlotRoot { get; private set; }

    public GameObject WeaponPrefab { get; private set; }

    public Transform ShotPoint { get; private set; }

    public WeaponRig WeaponRig { get; private set; }

    private WeaponRootPoint weaponRootHand;

    private WeaponRootPoint weaponRootSlot;

    private GameObject weaponInstance;
    public Transform WeaponInstance => this.weaponInstance.transform;

    public WeaponControllView(WeaponView weaponView)
    {
        GuidString = weaponView.GuidString;
        DisplayName = weaponView.DisplayName;
        WeaponType = weaponView.WeaponType;
        WeaponPrefab = weaponView.WeaponPrefab;
        HandRoot = weaponView.HandRoot;
        SlotRoot = weaponView.SlotRoot;
        ShotPoint = weaponView.ShotPoint;
        WeaponRig = weaponView.WeaponRig;
    }

    public void Init(WeaponRootPoint handRoot, WeaponRootPoint slotRoot)
    {
        if (!WeaponPrefab)
        {
            if (WeaponType != WeaponType.None)
                Debug.Log("WeaponPrefab Dont Setuped");

            return;
        }

        this.weaponRootHand = handRoot;
        this.weaponRootSlot = slotRoot;

        weaponInstance = GameObject.Instantiate(WeaponPrefab, weaponRootSlot.RootPoint);

        var shotView = weaponInstance.GetComponent<ShotWeaponView>();
        if (shotView != null)
            ShotPoint = shotView.ShotPoint;

        this.WeaponRig = weaponInstance.GetComponent<WeaponRig>();

        Show(false);
    }

    public void Destroy()
    {
        GameObject.Destroy(weaponInstance);
    }

    public void Show(bool value)
    {
        IsShow = value;

        if (weaponInstance)
        {
            weaponInstance.SetActive(value);
        }
        else
        {
            if (WeaponType != WeaponType.None)
                Debug.Log("WeaponInstance Dont Setuped");
        }
    }

    public void SetupView(bool hand)
    {
        if (!weaponInstance)
        {
            if (WeaponType != WeaponType.None)
                Debug.Log("WeaponInstance Dont Setuped");

            return;
        }

        if (hand)
        {
            weaponInstance.transform.parent = weaponRootHand.RootPoint;
        }
        else
        {
            weaponInstance.transform.parent = weaponRootSlot.RootPoint;
        }
    }
}

public class WeaponSelectorComp : IWeaponSelectorComp
{
    private List<IWeaponView> weaponSelectors = new List<IWeaponView>();
    List<IWeaponView> IWeaponSelectorComp.WeaponSelectors() => weaponSelectors;

    private Dictionary<string, Action<bool>> weaponContainer = new Dictionary<string, Action<bool>>();
    private Action<bool> outAction;

    public WeaponSelectorComp(List<WeaponView> weaponViews, List<WeaponRootPoint> weaponRootPoints)
    {
        weaponViews.ForEach(x =>
        {
            this.weaponSelectors.Add(new WeaponControllView(x));
        });

        foreach(IWeaponView wSelector in this.weaponSelectors)
        {
            wSelector.Init(weaponRootPoints.First(x => x.RootName == wSelector.HandRoot), weaponRootPoints.First(x => x.RootName == wSelector.SlotRoot));
            weaponContainer.Add(wSelector.GuidString, wSelector.Show);
        }
    }

    public void Parent(bool hand)
    {
        weaponSelectors.ForEach(x => x.SetupView(hand));
    }

    public void Show(string weaponGuid, bool show)
    {
        if (!weaponContainer.TryGetValue(weaponGuid, out outAction))
        {
            Debug.Log($"weaponType {weaponGuid} not Found in weaponContainer");
            return;
        }

        // UnSelect Previouse
        if (show)
            weaponSelectors.FindAll(o => o.GuidString != weaponGuid).ForEach(o => o.Show(false));

        // Select
        outAction(show);
    }

    public void DebugShow(string weaponGuid, bool show)
    {
        if (!weaponContainer.TryGetValue(weaponGuid, out outAction))
        {
            Debug.Log($"weaponType {weaponGuid} not Found in weaponContainer");
            return;
        }

        // UnSelect Previouse
        if (show)
            weaponSelectors.FindAll(o => o.GuidString != weaponGuid).ForEach(o => o.Show(false));

        // Select
        outAction(show);
    }

    public IWeaponView GetWeaponSelector(string weaponGuid)
    {
        return weaponSelectors.First(x => x.GuidString == weaponGuid);
    }
}

public interface IWeaponSelectorComp
{
    void Show(string weaponType, bool show);
    void DebugShow(string weaponType, bool show);

    IWeaponView GetWeaponSelector(string weaponType);

    void Parent(bool hand);

    List<IWeaponView> WeaponSelectors();
}