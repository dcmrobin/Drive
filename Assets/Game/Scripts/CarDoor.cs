using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDoor : MonoBehaviour
{
    public enum stat {Closed, Open};
    public enum id {FrontLeft, FrontRight, BackLeft, BackRight, Boot};
    public stat status;
    public id identity;

    private void Update() {
        if (status == stat.Closed)
        {
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
}
