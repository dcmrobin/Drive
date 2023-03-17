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
    //public Camera SceneCam;
    // Start is called before the first frame update
    void Awake()
    {
        CreatePlayer();
        //Destroy(GameObject.Find("theCanvas"));
        //Destroy(GameObject.Find("LobbyController"));
        GameObject.Find("theCanvas").SetActive(false);
        //GameObject.Find("LobbyController").SetActive(false);
    }

    private void CreatePlayer()
    {
        Debug.Log("Creating Player");
        GameObject myPlayerGo = (GameObject)PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), Vector3.zero, Quaternion.identity);
        myPlayerGo.transform.Find("Camerapivot").Find("Camera").GetComponent<Camera>().enabled = true;
        usrText = GameObject.FindGameObjectWithTag("lobbyController").GetComponent<LobbyController>().userNmText;
        myPlayerGo.GetComponent<PhotonView>().Owner.NickName = usrText.text;
    }
}
