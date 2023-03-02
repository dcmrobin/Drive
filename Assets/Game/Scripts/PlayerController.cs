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
    public LayerMask clickMask;
    public LayerMask camCollideMask;
    /*public LayerMask seatMask;
    public LayerMask pickupableMask;
    public LayerMask doorMask;*/
    RaycastHit hit;

    public GameObject playerCameraPivot;

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
    public bool thirdPersonViewActive = false;

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
        playerCameraPivot.transform.localRotation = Quaternion.Euler(lookY, 0f, 0f);

        if (!driving)
        {
            // check if the player is grounded
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f, groundMask))
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

        pickUp();
        drive();
        door();
        thirdPersonControl();
        if (!Physics.Raycast(playerCameraPivot.transform.position, playerCameraPivot.transform.forward, out hit, Mathf.Infinity, clickMask) || hit.collider.gameObject.layer == 0 || hit.collider.gameObject.layer == 6 || hit.collider.gameObject.layer == 1 || hit.collider.gameObject.layer == 3)
        {
            crosshair.GetComponent<Image>().sprite = normalCrosshair;
            canGrabby = false;
            currentObject = null;
        }
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
        if (Physics.Raycast(playerCameraPivot.transform.position, playerCameraPivot.transform.forward, out hit, Mathf.Infinity, clickMask))
        {
            if (hit.collider.transform.CompareTag("pickupable") && !driving)
            {
                currentObject = hit.transform.gameObject;
                crosshair.GetComponent<Image>().sprite = pickupableCrosshair;
                canGrabby = true;
            }
            else if (!Physics.Raycast(playerCameraPivot.transform.position, playerCameraPivot.transform.forward, out hit, Mathf.Infinity, clickMask) || !hit.collider.transform.CompareTag("pickupable"))
            {
                currentObject = null;
                canGrabby = false;
            }
        }

        // grab the object
        if (canGrabby && Input.GetMouseButton(0))
        {
            crosshair.GetComponent<Image>().sprite = pickedUpCrosshair;
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.transform.parent = playerCameraPivot.transform.Find("objectTarget");
            currentObject.transform.position = playerCameraPivot.transform.Find("objectTarget").position;
            grabby = true;
        }
        else if (grabby && Input.GetMouseButtonUp(0) && currentObject != null)
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
        if (Physics.Raycast(playerCameraPivot.transform.position, playerCameraPivot.transform.forward, out hit, Mathf.Infinity, clickMask))
        {
            if (hit.collider.transform.CompareTag("seat") && !driving)
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
        if (Physics.Raycast(playerCameraPivot.transform.position, playerCameraPivot.transform.forward, out hit, Mathf.Infinity, clickMask))
        {
            if (hit.collider.transform.CompareTag("door"))
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

    public void thirdPersonControl()
    {
        if (thirdPersonViewActive)
        {
            playerCameraPivot.transform.Find("Camera").transform.localPosition = new Vector3(0, 0, -7);
        }
        else if (!thirdPersonViewActive)
        {
            playerCameraPivot.transform.Find("Camera").transform.localPosition = new Vector3(0, 0, 0);
        }

        if (!thirdPersonViewActive && Input.GetKeyDown(KeyCode.F5))
        {
            thirdPersonViewActive = true;
        }
        else if (thirdPersonViewActive && Input.GetKeyDown(KeyCode.F5))
        {
            thirdPersonViewActive = false;
        }

        while (thirdPersonViewActive && Physics.CheckBox(playerCameraPivot.transform.Find("Camera").transform.position, new Vector3(1, 1, 1), Quaternion.identity, camCollideMask))
        {
            playerCameraPivot.transform.Find("Camera").transform.localPosition += new Vector3(0, 0, 0.1f);
        }
    }
}
