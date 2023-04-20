using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class Damageable : MonoBehaviour
{
    public int health;
    public enum OnDead{Explode, Disable};
    public OnDead onDeath;
    public ParticleSystem explosionEffect;
    public ParticleSystem steamEffect;
    public GameObject lobbyController;
    public bool isSteaming;
    bool hasExploded;
    bool isDead = false;

    private void Start() {
        lobbyController = GameObject.FindGameObjectWithTag("lobbyController");
    }

    private void Update() {
        if (!isDead)
        {
            if (health <= 0)
            {
                if (onDeath == OnDead.Explode)
                {
                    Explode();
                }
                else if (onDeath == OnDead.Disable)
                {
                    Disable();
                }
                isDead = true;
            }
        }
    }

    [PunRPC]
    public void TakeDamage(int amount)
    {
        health -= amount;
    }

    public void Explode()
    {
        if (!hasExploded)
        {
            if (lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Multiplayer)
            {
                GameObject effect = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", explosionEffect.name), transform.position, Quaternion.identity);
                effect.transform.parent = gameObject.transform;
                StartCoroutine(PhotonDestroy(gameObject));
                hasExploded = true;
            }
            else if (lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Singleplayer)
            {
                GameObject effect = GameObject.Instantiate(explosionEffect.gameObject, transform.position, Quaternion.identity);
                effect.transform.parent = gameObject.transform;
                StartCoroutine(PhotonDestroy(gameObject));
                hasExploded = true;
            }
        }
    }

    public void Disable()
    {
        if (!isSteaming)
        {
            if (!hasExploded)
            {
                GameObject expEffect;
                if (lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Multiplayer)
                {
                    expEffect = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", explosionEffect.name), transform.position, Quaternion.identity);
                    expEffect.transform.parent = gameObject.transform;
                    hasExploded = true;
                }
                else if (lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Singleplayer)
                {
                    expEffect = GameObject.Instantiate(explosionEffect.gameObject, transform.position, Quaternion.identity);
                    expEffect.transform.parent = gameObject.transform;
                    hasExploded = true;
                }
            }
            GameObject effect;
            if (lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Multiplayer)
            {
                effect = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", steamEffect.name), transform.position, Quaternion.identity);
                effect.transform.parent = gameObject.transform;
                isSteaming = true;
            }
            else if (lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Singleplayer)
            {
                effect = GameObject.Instantiate(steamEffect.gameObject, transform.position, Quaternion.identity);
                effect.transform.parent = gameObject.transform;
                isSteaming = true;
            }
        }
    }

    [PunRPC]
    IEnumerator PhotonDestroy(GameObject thingToDestroy)
    {
        yield return new WaitForSeconds(0.1f);
        PhotonNetwork.Destroy(thingToDestroy);
    }
}
