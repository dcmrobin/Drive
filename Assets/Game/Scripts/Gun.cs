using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    public TMP_Text ammoNumber;
    public GameObject ammoCanvas;
    public Slider reloadSlider;
    public int maxAmmo = 3;
    public int damage = 25;
    public float timeToReload = 17;
    public int ammo;
    public float reloadTimer = 0;
    int myAmmo;

    private void Start() {
        reloadSlider.maxValue = timeToReload;
        ammo = maxAmmo;
    }

    [PunRPC]
    public void Shoot(int ammoSpent)
    {
        ammo -= ammoSpent;
    }

    [PunRPC]
    public void UpdateAmmo()
    {
        ammo = myAmmo;
    }

    private void Update() {
        myAmmo = ammo;
        reloadSlider.value = reloadTimer;
        if (ammoNumber != null)
        {
            ammoNumber.text = ammo.ToString();
        }
    }
}
