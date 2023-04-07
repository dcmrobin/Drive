using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviourPunCallbacks
{
    public Text userNmText;
    [SerializeField]
    private Text buttonText;
    [SerializeField]
    private int roomSize;
    private bool connected;
    private bool starting;
    public int singleplayerSceneIndex = 2;
    public enum mode{Singleplayer, Multiplayer};
    public mode gameMode;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        connected = true;
        buttonText.text = "Multiplayer";
    }

    public void LoadScene()
    {
        gameMode = mode.Singleplayer;
        SceneManager.LoadScene(singleplayerSceneIndex);
        GameObject.Find("theCanvas").SetActive(false);
    }

    public void GameButton()
    {
        gameMode = mode.Multiplayer;
        if (connected)
        {
            Debug.Log("Clicked");
            //PhotonNetwork.JoinRandomRoom(); // attempt joining a room
            if (!starting)
            {
                starting = true;
                buttonText.text = "Starting...";
                PhotonNetwork.JoinRandomOrCreateRoom(); // attempt joining a room
            }
            else
            {
                starting = false;
                buttonText.text = "Multiplayer";
                PhotonNetwork.LeaveRoom(); // cancel the request
            }
        }
        else
            Debug.Log("Not connected to server!");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join a room... creating room");
	    CreateRoom();
    }

    void CreateRoom()
    {
        Debug.Log("Creating room now");
        int randomRoomNumber = Random.Range(0, 10000);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, 
                IsOpen = true, MaxPlayers = (byte)roomSize };
        PhotonNetwork.CreateRoom("Room" + randomRoomNumber, roomOps); 
        Debug.Log(randomRoomNumber); 
    }
    public override void OnCreateRoomFailed(short returnCode, string message) 
    {
        Debug.Log("Failed to create room... trying again"); 
        CreateRoom();
    }
}
