using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class baddie : MonoBehaviour
{
    Transform target;
    public float moveSpeed = 1;
    public int damage = 5;

    private Rigidbody rb;
    //public double health = 10;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<PhotonView>().RequestOwnership();
        rb = GetComponent<Rigidbody>();
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        // Determine the direction to the target
        Vector3 direction = (target.position - transform.position).normalized;

        // Set the velocity to move towards the target
        rb.velocity = direction * moveSpeed;

        // Determine which direction to rotate towards
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Rotate towards the target direction gradually
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);

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
