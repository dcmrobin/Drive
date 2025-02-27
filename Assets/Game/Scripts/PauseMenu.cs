using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject player;
    public GameObject carPrefab;
    public Button continueButton;
    public bool buttonCooldown = false;
    public bool antiRollEnabled = true;
    public bool gunAmmoVisible = true;
    public bool exiting;

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
        if (!player.GetComponent<PlayerController>().driving && !player.GetComponent<PlayerController>().inBoot)
        {
            player.transform.position = new Vector3(0, 2, 0);
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void EnableAntiRoll(bool antiRoll)
    {
        antiRollEnabled = antiRoll;
    }

    public void ShowAmmo(bool visible)
    {
        gunAmmoVisible = visible;
    }

    public void ResetCooldown()
    {
        buttonCooldown = false;
    }

    public void MainMenu()
    {
        continueButton.interactable = false;
        GameObject oldCanvas = player.GetComponent<PlayerController>().lobbyController.GetComponent<LobbyController>().theCanvas;
        oldCanvas.name = "oldCanvas";
        oldCanvas.SetActive(true);
        GameObject newCanvas = Instantiate(oldCanvas);
        //DestroyAllDontDestroyOnLoadObjects();
        DontDestroyOnLoad(newCanvas);
        newCanvas.SetActive(true);
        newCanvas.name = "template";
        exiting = true;
        Time.timeScale = 1;
        //PhotonNetwork.LeaveRoom();
        //PhotonNetwork.Disconnect();
        StartCoroutine(player.GetComponent<PlayerController>().Die());
        //SceneManager.LoadScene(0);
    }

    public void DestroyAllDontDestroyOnLoadObjects() {

        var go = new GameObject("Bill");
        DontDestroyOnLoad(go);

        foreach(var root in go.scene.GetRootGameObjects())
        {
            Destroy(root);
        }

    }
}
