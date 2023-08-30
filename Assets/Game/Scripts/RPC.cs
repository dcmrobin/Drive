using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RPC : MonoBehaviour
{
    private void Start() {
        if (CompareTag("pickupable") || CompareTag("gun"))
        {
            transform.parent = null;
        }
    }

    [PunRPC]
    void UpdateParent(bool boolean, int ID)
    {
        PhotonView id = PhotonView.Find(ID);
        if (boolean)
        {
            transform.parent = id.GetComponent<PlayerController>().objTarget.transform;
        }
        else if (!boolean)
        {
            transform.parent = null;
        }
    }

    [PunRPC]
    void UpdateGunParent(bool boolean, int ID)
    {
        PhotonView id = PhotonView.Find(ID);
        if (boolean)
        {
            transform.parent = id.GetComponent<PlayerController>().gunTarget.transform;
        }
        else if (!boolean)
        {
            transform.parent = null;
        }
    }

    [PunRPC]
    void UpdatePosition(int ID)
    {
        PhotonView id = PhotonView.Find(ID);
        transform.position = id.GetComponent<PlayerController>().objTarget.transform.position;
    }

    [PunRPC]
    void UpdateGunPosition(int ID)
    {
        PhotonView id = PhotonView.Find(ID);
        transform.position = id.GetComponent<PlayerController>().gunTarget.transform.position;
    }

    [PunRPC]
    void UpdateRigidbody(bool boolean, int ID)
    {
        PhotonView id = PhotonView.Find(ID);
        GetComponent<Rigidbody>().isKinematic = boolean;
    }
}
