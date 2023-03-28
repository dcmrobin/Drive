using Photon.Pun;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSetupController : MonoBehaviour
{
    //public Camera[] cameras;
    public Camera[] cameras;
    Text usrText;
    Color playerCol;
    //public Camera SceneCam;
    // Start is called before the first frame update
    void Awake()
    {
        playerCol = GameObject.Find("color").GetComponent<Image>().color;
        CreatePlayer();
        GameObject.Find("theCanvas").SetActive(false);
    }

    private void CreatePlayer()
    {
        Debug.Log("Creating Player");
        GameObject myPlayerGo = (GameObject)PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), Vector3.zero, Quaternion.identity);
        myPlayerGo.transform.Find("Camerapivot").Find("Camera").GetComponent<Camera>().enabled = true;
        myPlayerGo.GetComponent<MeshRenderer>().material.color = playerCol;
        usrText = GameObject.FindGameObjectWithTag("lobbyController").GetComponent<LobbyController>().userNmText;
        myPlayerGo.GetComponent<PhotonView>().Owner.NickName = usrText.text;
    }
}
