using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Despawn : MonoBehaviour
{
    public GameObject[] players;
    public float distanceFromPlayer;

    // Update is called once per frame
    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            distanceFromPlayer = Vector3.Distance(gameObject.transform.position, player.transform.position);
            if (distanceFromPlayer >= 10000)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
