using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PauseMenu : MonoBehaviour
{
    public GameObject player;
    public GameObject carPrefab;
    public bool buttonCooldown = false;
    public bool antiRollEnabled = true;

    public void Continue()
    {
        player.GetComponent<PlayerController>().isPaused = false;
        gameObject.SetActive(false);
    }

    public void SpawnCar()
    {
        if (!buttonCooldown && player.GetComponent<PlayerController>().lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Multiplayer)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "car_root"), new Vector3(player.transform.position.x + 5, player.transform.position.y, player.transform.position.z), Quaternion.identity);
            Invoke("ResetCooldown", .5f);
            buttonCooldown = true;
        }
        else if (player.GetComponent<PlayerController>().lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Singleplayer)
        {
            GameObject.Instantiate(carPrefab, new Vector3(player.transform.position.x + 5, player.transform.position.y, player.transform.position.z), Quaternion.identity);
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

    public void enableAntiRoll(bool antiRoll)
    {
        antiRollEnabled = antiRoll;
    }

    public void ResetCooldown()
    {
        buttonCooldown = false;
    }
}
