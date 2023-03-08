using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject player;
    public GameObject carPrefab;

    public void Continue()
    {
        player.GetComponent<PlayerController>().isPaused = false;
        gameObject.SetActive(false);
    }

    public void SpawnCar()
    {
        Instantiate(carPrefab, new Vector3(player.transform.position.x + 5, player.transform.position.y, player.transform.position.z), Quaternion.identity);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
