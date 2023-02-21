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
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float lookSensitivity = 3f;
    public float maxLookAngle = 90f;
    public float groundDistance = 0.1f;
    public LayerMask groundMask;
    RaycastHit hit;

    public Camera playerCamera;

    private Rigidbody rb;
    private bool isGrounded;
    private float lookX;
    private float lookY;
    private bool driving;
    private GameObject currentCar;

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
        transform.rotation = Quaternion.Euler(0f, lookX, 0f);
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

        // check if player is looking at car
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 10f, 7) && !driving)
        {
            crosshair.GetComponent<Image>().sprite = carCrosshair;
            if (Input.GetMouseButtonDown(0))
            {
                currentCar = hit.transform.gameObject;
                driving = true;
                GetComponent<CapsuleCollider>().isTrigger = true;
                transform.parent = currentCar.transform;
                GetComponent<Rigidbody>().isKinematic = true;
                transform.position = hit.transform.position;
                currentCar.GetComponent<SimpleCarController>().enabled = true;
            }
        }
        else
        {
            crosshair.GetComponent<Image>().sprite = normalCrosshair;
        }
        if (driving && Input.GetKeyDown(KeyCode.E))
        {
            driving = false;
            transform.parent = null;
            GetComponent<Rigidbody>().isKinematic = false;
            transform.position += new Vector3(0, 2, 0);
            GetComponent<CapsuleCollider>().isTrigger = false;
            currentCar.GetComponent<SimpleCarController>().enabled = false;
            currentCar = null;
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
}
