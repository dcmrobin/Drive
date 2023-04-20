using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Gun : MonoBehaviour
{
    public int maxAmmo = 3;
    public float timeToReload = 17;
    public int ammo;
    public float reloadTimer = 0;
    int myAmmo;

    private void Start() {
        ammo = maxAmmo;
    }

    [PunRPC]
    public void Shoot(int ammoSpent)
    {
        ammo -= ammoSpent;
        myAmmo = ammo;
    }

    [PunRPC]
    public void UpdateAmmo()
    {
        ammo = myAmmo;
    }
}
