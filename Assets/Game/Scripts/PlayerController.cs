using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public GameObject crosshair;
    public Sprite normalCrosshair;
    public Sprite carCrosshair;
    public Sprite pickupableCrosshair;
    public Sprite pickedUpCrosshair;
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float lookSensitivity = 3f;
    public float maxLookAngle = 90f;
    public float groundDistance = 0.1f;
    public LayerMask groundMask;
    public LayerMask seatMask;
    public LayerMask pickupableMask;
    public LayerMask doorMask;
    RaycastHit hit;

    public Camera playerCamera;

    private Rigidbody rb;
    private bool isGrounded;
    private float lookX;
    private float lookY;
    private bool driving;
    private GameObject currentCar;
    private GameObject currentObject;
    private GameObject currentDoor;
    private bool canGrabby;
    private bool grabby;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // get the horizontal and vertical mouse input
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        // update the look angles
        lookX += mouseX;
        lookY = Mathf.Clamp(lookY - mouseY, -maxLookAngle, maxLookAngle);

        // rotate the player and camera based on the mouse input
        transform.localRotation = Quaternion.Euler(0f, lookX, 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(lookY, 0f, 0f);

        if (!driving)
        {
            // check if the player is grounded
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f, groundMask))
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
    
            if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        drive();
        pickUp();
        door();
    }

    void FixedUpdate()
    {
        if (!driving)
        {
            // get the player's forward direction
            Vector3 forward = transform.forward;
            forward.y = 0;

            // get the horizontal and vertical input
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // normalize the movement input
            Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

            // move the player in the direction they are facing
            rb.MovePosition(transform.position + forward * movement.z * moveSpeed * Time.fixedDeltaTime + transform.right * movement.x * moveSpeed * Time.fixedDeltaTime);
        }
    }

    public void pickUp()
    {
        // check if player is looking at pickupable
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, Mathf.Infinity, pickupableMask) && !driving)
        {
            currentObject = hit.transform.gameObject;
            crosshair.GetComponent<Image>().sprite = pickupableCrosshair;
            canGrabby = true;
        }
        else if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, Mathf.Infinity, seatMask))
        {
            currentObject = null;
            crosshair.GetComponent<Image>().sprite = normalCrosshair;
            canGrabby = false;
        }

        // grab the object
        if (canGrabby && Input.GetMouseButton(0))
        {
            crosshair.GetComponent<Image>().sprite = pickedUpCrosshair;
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.transform.parent = playerCamera.transform.Find("objectTarget");
            currentObject.transform.position = playerCamera.transform.Find("objectTarget").position;
            grabby = true;
        }
        else if (grabby && Input.GetMouseButtonUp(0))
        {
            currentObject.GetComponent<Rigidbody>().isKinematic = false;
            currentObject.transform.parent = null;
            grabby = false;
        }

        // drop override
        if (driving && currentObject != null)
        {
            currentObject.GetComponent<Rigidbody>().isKinematic = false;
            currentObject.transform.parent = null;
            grabby = false;
        }
    }

    public void drive()
    {
        // check if player is looking at car
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, Mathf.Infinity, seatMask) && !driving)
        {
            crosshair.GetComponent<Image>().sprite = carCrosshair;
            if (Input.GetMouseButtonDown(0))
            {
                currentCar = hit.transform.gameObject;
                driving = true;
                GetComponent<CapsuleCollider>().isTrigger = true;
                transform.parent = currentCar.transform.Find("seatTarget");
                GetComponent<Rigidbody>().isKinematic = true;
                transform.position = currentCar.transform.Find("seatTarget").transform.position;
                currentCar.GetComponent<SimpleCarController>().enabled = true;
            }
        }
        else if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, Mathf.Infinity, pickupableMask))
        {
            crosshair.GetComponent<Image>().sprite = normalCrosshair;
        }
        if (driving && Input.GetKeyDown(KeyCode.E))
        {
            driving = false;
            transform.parent = null;
            GetComponent<Rigidbody>().isKinematic = false;
            transform.position += new Vector3(0, 5, 0);
            GetComponent<CapsuleCollider>().isTrigger = false;
            currentCar.GetComponent<SimpleCarController>().enabled = false;
            currentCar = null;
        }
    }

    public void door()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, Mathf.Infinity, doorMask))
        {
            crosshair.GetComponent<Image>().sprite = pickupableCrosshair;
            if (Input.GetMouseButtonDown(0))
            {
                currentDoor = hit.collider.transform.parent.gameObject;
                if (currentDoor.GetComponent<CarDoor>().status == CarDoor.stat.Closed)
                {
                    currentDoor.GetComponent<CarDoor>().status = CarDoor.stat.Open;
                }
                else if (currentDoor.GetComponent<CarDoor>().status == CarDoor.stat.Open)
                {
                    currentDoor.GetComponent<CarDoor>().status = CarDoor.stat.Closed;
                }
            }
        }
    }
}
