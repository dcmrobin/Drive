using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckIfColliding : MonoBehaviour
{
    public LayerMask collisionMask;

    public bool IsColliding()
    {
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale / 3, Quaternion.identity, collisionMask);
        int i = 0;
        //Check when there is a new collider coming into contact with the box
        while (i < hitColliders.Length)
        {
            i++;
        }

        if (hitColliders.Length > 0)
        {
            return true;
        }

        return false;
    }
}
