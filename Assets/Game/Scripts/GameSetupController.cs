using Photon.Pun;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class GameSetupController : MonoBehaviourPunCallbacks
{
    //public Camera[] cameras;
    public Camera[] cameras;
    Text usrText;
    public Color playerCol;
    GameObject player;
    GameObject[] allGuns;
    //public Camera SceneCam;
    // Start is called before the first frame update
    void Awake()
    {
        playerCol = GameObject.Find("color").GetComponent<Image>().color;
        CreatePlayer();
        GameObject.Find("theCanvas").SetActive(false);
    }

    private void Update() {
        allGuns = GameObject.FindGameObjectsWithTag("gun");
    }

    private void CreatePlayer()
    {
        Debug.Log("Creating Player");
        GameObject myPlayerGo = (GameObject)PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(0, 5, 0), Quaternion.identity);
        player = myPlayerGo;
        myPlayerGo.transform.Find("Camerapivot").Find("Camera").GetComponent<Camera>().enabled = true;
        usrText = GameObject.FindGameObjectWithTag("lobbyController").GetComponent<LobbyController>().userNmText;
        myPlayerGo.GetComponent<PhotonView>().Owner.NickName = usrText.text;
        myPlayerGo.GetComponent<PhotonView>().RPC("GetColor", RpcTarget.All, playerCol.r, playerCol.g, playerCol.b);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        player.GetComponent<PhotonView>().RPC("UpdatePlayerColor", RpcTarget.All, playerCol.r, playerCol.g, playerCol.b);
        player.GetComponent<PhotonView>().RPC("UpdateHealth", RpcTarget.All);
        /*player.GetComponent<PlayerController>().currentCar.GetComponent<PhotonView>().RPC("UpdateDriver", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
        if (player.GetComponent<PlayerController>().grabby)
        {
            player.GetComponent<PlayerController>().currentObject.GetComponent<PhotonView>().RPC("UpdateParent", RpcTarget.All, true, player.GetComponent<PhotonView>().ViewID);
            player.GetComponent<PlayerController>().currentObject.GetComponent<PhotonView>().RPC("UpdatePosition", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
        }
        else if (!player.GetComponent<PlayerController>().grabby)
        {
            player.GetComponent<PlayerController>().currentObject.GetComponent<PhotonView>().RPC("UpdateParent", RpcTarget.All, false, player.GetComponent<PhotonView>().ViewID);
        }*/
        for (int i = 0; i < allGuns.Length; i++)
        {
            if (allGuns[i].GetComponent<PhotonView>() != null)
            {
                allGuns[i].GetComponent<PhotonView>().RPC("UpdateAmmo", RpcTarget.All);
            }
        }
    }
}
