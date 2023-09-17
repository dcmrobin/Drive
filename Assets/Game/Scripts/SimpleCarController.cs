using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;

[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}
     
public class SimpleCarController : MonoBehaviour {
    public List<AxleInfo> axleInfos; 
    public float maxMotorTorque;
    public float maxSteeringAngle;
    public Transform steeringWheel;
    public GameObject currentDriver;
    public TMP_Text speedNumText;
    public PhotonView pv;
    public GameObject[] objectsInCar;

    private void Start() {
        if (objectsInCar != null)
        {
            for (int i = 0; i < objectsInCar.Length; i++)
            {
                objectsInCar[i].GetComponent<PhotonView>().RPC("UpdateParent", RpcTarget.All, true, pv.ViewID);
            }
        }
    }
    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0) {
            return;
        }
     
        Transform visualWheel = collider.transform.GetChild(0);
     
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
     
        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }
     
    public void FixedUpdate()
    {
        speedNumText.text = Mathf.Round(GetComponent<Rigidbody>().velocity.magnitude).ToString();
        if (GetComponent<Damageable>() != null)
        {
            maxMotorTorque = GetComponent<Damageable>().health;
        }
        if (maxMotorTorque < 0)
        {
            maxMotorTorque = 0;
        }
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
     
        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.steering) {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
                steeringWheel.localRotation = Quaternion.Euler(0, 0, -steering);
            }
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            //IsSkidding(axleInfo.rightWheel);
            //IsSkidding(axleInfo.leftWheel);
        }
    }

    public void IsSkidding(WheelCollider collider)
    {
        WheelHit hit = new WheelHit();

        if (collider.isGrounded && collider.GetGroundHit(out hit))
        {
            if (hit.sidewaysSlip > 0.15 || hit.sidewaysSlip < -0.15)
            {
                StartEmitting(collider);
            }
            else
            {
                StopEmitting(collider);
            }
        }
    }

    void StartEmitting(WheelCollider collider)
    {
        collider.transform.Find("trail").GetComponent<TrailRenderer>().emitting = true;
    }
    void StopEmitting(WheelCollider collider)
    {
        collider.transform.Find("trail").GetComponent<TrailRenderer>().emitting = false;
    }

    [PunRPC]
    void UpdateDriver(bool isDriving, int playerViewID)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        if (isDriving)
        {
            currentDriver = playerView.gameObject;
        }
        else if (!isDriving)
        {
            currentDriver = null;
        }
    }

    private void OnCollisionEnter(Collision other) {
        if (GetComponent<Rigidbody>().velocity.magnitude >= 20)
        {
            if (other.gameObject.GetComponent<Damageable>() != null)
            {
                if (!other.gameObject.CompareTag("Player"))
                {
                    other.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, Mathf.RoundToInt(GetComponent<Rigidbody>().velocity.magnitude));
                }
            }
    
            if (other.gameObject.CompareTag("Player"))
            {
                if (other.gameObject.GetComponent<Rigidbody>().velocity.magnitude < GetComponent<Rigidbody>().velocity.magnitude - 10)
                {
                    Debug.Log("ouch");
                    other.gameObject.GetComponent<PhotonView>().RPC("GetHurt", RpcTarget.All, Mathf.RoundToInt(GetComponent<Rigidbody>().velocity.magnitude));
                }
            }
        }
    }
}