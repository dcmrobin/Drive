using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baddie : MonoBehaviour
{
    Transform target;
    public double moveSpeed = 1;
    //public double health = 10;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        var step =  (float)moveSpeed * Time.deltaTime;
        var rotation = Quaternion.LookRotation(target.position);
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, step);

        if (gameObject.transform.position.y <= -2000)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other) {
        //if (other.gameObject.CompareTag("Player"))
        //{
        //    // do stuff
        //}
    }
}
