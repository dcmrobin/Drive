using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class Gun : MonoBehaviourPunCallbacks
{
    public enum type{Handgun, SniperRifle}
    public type gunType;
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
    public void UpdateAmmo(int amt)
    {
        ammo = amt;
    }

    private void Update() {
        myAmmo = ammo;
        reloadSlider.value = reloadTimer;
        if (ammoNumber != null)
        {
            ammoNumber.text = ammo.ToString();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Invoke("UpdateRPC", 2);
    }

    public void UpdateRPC()
    {
        GetComponent<PhotonView>().RPC("UpdateAmmo", RpcTarget.All, myAmmo);
    }
}
