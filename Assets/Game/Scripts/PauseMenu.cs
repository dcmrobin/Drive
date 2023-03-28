using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PauseMenu : MonoBehaviour
{
    public GameObject player;
    public GameObject carPrefab;
    public bool buttonCooldown = false;

    public void Continue()
    {
        player.GetComponent<PlayerController>().isPaused = false;
        gameObject.SetActive(false);
    }

    public void SpawnCar()
    {
        if (!buttonCooldown)
        {
            PhotonNetwork.Instantiate("car_root", new Vector3(player.transform.position.x + 5, player.transform.position.y, player.transform.position.z), Quaternion.identity);
            Invoke("ResetCooldown", 5.0f);
            buttonCooldown = true;
        }
    }

    public void Respawn()
    {
        if (!player.GetComponent<PlayerController>().driving)
        {
            player.transform.position = new Vector3(0, 2, 0);
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void ResetCooldown()
    {
        buttonCooldown = false;
    }
}
