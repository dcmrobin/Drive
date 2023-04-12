using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RPC : MonoBehaviour
{
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
    void UpdatePosition(int ID)
    {
        PhotonView id = PhotonView.Find(ID);
        transform.position = id.GetComponent<PlayerController>().objTarget.transform.position;
    }
}
