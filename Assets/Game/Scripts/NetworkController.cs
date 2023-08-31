using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkController : MonoBehaviourPunCallbacks
{
    public Dropdown regionDropdown;
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.ConnectToRegion(regionDropdown.options[regionDropdown.value].text);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("We are now connected the " + PhotonNetwork.CloudRegion + " server!");
    }

    public void UpdateRegion()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.ConnectToRegion(regionDropdown.options[regionDropdown.value].text);
    }
}
