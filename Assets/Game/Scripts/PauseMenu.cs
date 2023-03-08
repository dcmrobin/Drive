using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PauseMenu : MonoBehaviour
{
    public GameObject player;
    public GameObject carPrefab;

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    public void Continue()
    {
        player.GetComponent<PlayerController>().isPaused = false;
        gameObject.SetActive(false);
    }

    public void SpawnCar()
    {
        PhotonNetwork.Instantiate("car_root", new Vector3(player.transform.position.x + 5, player.transform.position.y, player.transform.position.z), Quaternion.identity);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
