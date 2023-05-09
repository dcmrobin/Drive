using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class baddie : MonoBehaviour
{
    Transform target;
    public double moveSpeed = 1;
    public int damage = 5;
    //public double health = 10;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<PhotonView>().RequestOwnership();
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        var step =  (float)moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        // Determine which direction to rotate towards
        Vector3 targetDirection = target.position - transform.position;
        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, new Vector3(targetDirection.x, 0, targetDirection.z), step, 0.0f);
        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);

        if (gameObject.transform.position.y <= -2000)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerController>().lobbyController != null)
            {
                if (other.GetComponent<PlayerController>().lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Multiplayer)
                {
                    other.GetComponent<PhotonView>().RPC("GetHurt", RpcTarget.All, damage);
                }
                else if (other.GetComponent<PlayerController>().lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Singleplayer)
                {
                    other.GetComponent<PlayerController>().GetHurt(damage);
                }
            }
        }
    }
}
