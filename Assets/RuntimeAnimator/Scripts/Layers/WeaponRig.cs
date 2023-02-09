using System.Collections.Generic;
using UnityEngine;

public class WeaponRig : MonoBehaviour
{
    [SerializeField]
    private List<Transform> rigs;
    public List<Transform> Rigs => this.rigs;

    private void Start()
    {
        
    }
}
