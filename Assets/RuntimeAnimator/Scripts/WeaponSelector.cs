using System;
using UnityEngine;

public class WeaponSelectorEditorAttribute : PropertyAttribute
{
    public WeaponSelectorEditorAttribute()
    {
    }
}

[Serializable]
public class WeaponSelector
{
    public void SetupWeapon(string weaponView)
    {
        Weapon = weaponView;
    }

    [SerializeField]
    private string Weapon;

    public string CurrentWeapon()
    {
        try
        {
            return Weapon.Split(":")[1];
        }
        catch
        {
            return Weapon;
        }
    }
}