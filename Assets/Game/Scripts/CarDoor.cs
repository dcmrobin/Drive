using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDoor : MonoBehaviour
{
    public enum stat {Closed, Open};
    public enum id {FrontLeft, FrontRight, BackLeft, BackRight};
    public stat status;
    public id identity;
}
