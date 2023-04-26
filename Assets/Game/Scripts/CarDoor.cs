using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CarDoor : MonoBehaviour
{
    public enum stat {Closed, Open};
    public enum id {FrontLeft, FrontRight, BackLeft, BackRight, Boot};
    public stat status;
    public id identity;

    /*public ExitGames.Client.Photon.Hashtable myProperties;

    private void Start() {
        myProperties = new ExitGames.Client.Photon.Hashtable();
        myProperties.Add("status", status);
    }*/

    private void Update() {
        if (status == stat.Closed)
        {
            //GetComponent<PhotonView>().Owner.SetCustomProperties(myProperties);
            if (identity == id.FrontLeft)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else if (identity == id.FrontRight)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else if (identity == id.BackLeft)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else if (identity == id.BackRight)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else if (identity == id.Boot)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
        else if (status == stat.Open)
        {
            //GetComponent<PhotonView>().Owner.SetCustomProperties(myProperties);
            if (identity == id.FrontLeft)
            {
                transform.localRotation = Quaternion.Euler(0, 90, 0);
            }
            else if (identity == id.FrontRight)
            {
                transform.localRotation = Quaternion.Euler(0, -90, 0);
            }
            else if (identity == id.BackLeft)
            {
                transform.localRotation = Quaternion.Euler(0, 90, 0);
            }
            else if (identity == id.BackRight)
            {
                transform.localRotation = Quaternion.Euler(0, -90, 0);
            }
            else if (identity == id.Boot)
            {
                transform.localRotation = Quaternion.Euler(90, 0, 0);
            }
        }
    }

    [PunRPC]
    public void UpdateDoorValue(bool open)
    {
        if (open)
        {
            status = stat.Open;
        }
        else if (!open)
        {
            status = stat.Closed;
        }
    }
}
