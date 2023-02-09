using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponContext : MonoBehaviour
{
    [SerializeField]
    private List<WeaponView> weaponViews;
    public List<WeaponView> WeaponViews => weaponViews;

    private void OnValidate()
    {
        weaponViews.ForEach(x => { if (string.IsNullOrEmpty(x.GuidString)) x.InitGuid(); });
    }

    public WeaponView GetView(string guid)
    {
        return weaponViews.First(x => x.GuidString == guid);
    }
}
