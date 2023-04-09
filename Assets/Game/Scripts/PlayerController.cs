using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using System.IO;
using System;

public class PlayerController : MonoBehaviour
{
    public Text nickname;
    GameObject[] playerCrosshairs;
    public GameObject[] allCars;
    public GameObject currentDriver;
    public GameObject pauseMenu;
    public GameObject crosshair;
    public Sprite normalCrosshair;
    public Sprite carCrosshair;
    public Sprite pickupableCrosshair;
    public Sprite pickedUpCrosshair;
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
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
    public bool driving;
    private GameObject currentCar;
    private GameObject currentObject;
    private GameObject currentDoor;
    private bool canGrabby;
    private bool grabby;
    public bool thirdPersonViewActive = false;
    public bool isPaused = false;
    public bool isRunning = false;
    public PhotonView pv;
    int screenshotNum = 0;
    public GameObject lobbyController;
    bool holdingGun;

    void Start()
    {
        lobbyController = GameObject.FindGameObjectWithTag("lobbyController");
        rb = GetComponent<Rigidbody>();
        if (pv.Owner != null)
        {
            nickname.text = pv.Owner.NickName;
        }
    }

    void Update()
    {
        allCars = GameObject.FindGameObjectsWithTag("car");
        playerCrosshairs = GameObject.FindGameObjectsWithTag("crosshair");
        // get the horizontal and vertical mouse input
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        if (pv.IsMine)
        {
            if (pauseMenu.GetComponent<PauseMenu>().antiRollEnabled)
            {
                for (int i = 0; i < allCars.Length; i++)
                {
                    allCars[i].GetComponent<AntiRollBar>().enabled = true;
                }
            }
            else if (!pauseMenu.GetComponent<PauseMenu>().antiRollEnabled)
            {
                for (int i = 0; i < allCars.Length; i++)
                {
                    allCars[i].GetComponent<AntiRollBar>().enabled = false;
                }
            }

            if (!isPaused)
            {
                // update the look angles
                lookX += mouseX;
                lookY = Mathf.Clamp(lookY - mouseY, -maxLookAngle, maxLookAngle);
        
                // rotate the player and camera based on the mouse input
                transform.localRotation = Quaternion.Euler(0f, lookX, 0f);
                playerCameraPivot.transform.localRotation = Quaternion.Euler(lookY, 0f, 0f);
            }
        }

        if (pv.IsMine)
        {
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
        
                if (isGrounded && Input.GetKeyDown(KeyCode.Space) && !holdingGun)
                {
                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                }
            }
        }

        if (pv.IsMine)
        {
            if (!isPaused)
            {
                pickUp();
                drive();
                door();
                thirdPersonControl();
                gunControl();
            }
            handlePause();
        }
        if (pv.IsMine)
        {
            if (!isPaused)
            {
                if (!Physics.Raycast(playerCameraPivot.transform.position, playerCameraPivot.transform.forward, out hit, Mathf.Infinity, clickMask) || hit.collider.gameObject.layer == 0 || hit.collider.gameObject.layer == 6 || hit.collider.gameObject.layer == 1 || hit.collider.gameObject.layer == 3)
                {
                    crosshair.GetComponent<Image>().sprite = normalCrosshair;
                    canGrabby = false;
                    currentObject = null;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (File.Exists(Application.dataPath + "screenshotIncrementer.txt"))
            {
                screenshotNum = Convert.ToInt32(File.ReadAllText(Application.dataPath + "\\screenshotIncrementer.txt"));
            }
            screenshotNum++;
            string path = Application.dataPath;
            string newPath = Path.GetFullPath(Path.Combine(path, @"..\"));
            if (!Directory.Exists(newPath + "Screenshots"))
            {
                Directory.CreateDirectory(newPath + "Screenshots");
            }
            if (!File.Exists(Application.dataPath + "screenshotIncrementer.txt"))
            {
                File.Create(Application.dataPath + "screenshotIncrementer.txt");
                File.AppendAllText(Application.dataPath + "\\screenshotIncrementer.txt", screenshotNum.ToString());
            }
            else
            {
                File.WriteAllText(Application.dataPath + "\\screenshotIncrementer.txt", string.Empty);
                File.AppendAllText(Application.dataPath + "\\screenshotIncrementer.txt", screenshotNum.ToString());
            }
            ScreenCapture.CaptureScreenshot(newPath + "Screenshots\\Screenshot" + screenshotNum + ".png");
        }
    }

    void FixedUpdate()
    {
        if (pv.IsMine)
        {
            if (!driving)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    isRunning = true;
                }
                else if (Input.GetKeyUp(KeyCode.LeftShift))
                {
                    isRunning = false;
                }
                // get the player's forward direction
                Vector3 forward = transform.forward;
                forward.y = 0;
    
                // get the horizontal and vertical input
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");
    
                // normalize the movement input
                Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;
    
                // move the player in the direction they are facing
                if (!isRunning)
                {
                    rb.MovePosition(transform.position + forward * movement.z * walkSpeed * Time.fixedDeltaTime + transform.right * movement.x * walkSpeed * Time.fixedDeltaTime);
                }
                else if (isRunning)
                {
                    rb.MovePosition(transform.position + forward * movement.z * runSpeed * Time.fixedDeltaTime + transform.right * movement.x * runSpeed * Time.fixedDeltaTime);
                }
            }
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
        if (canGrabby && currentObject.transform.GetChild(0).tag != "gun")
        {
            if (canGrabby && Input.GetMouseButton(0))
            {
                crosshair.GetComponent<Image>().sprite = pickedUpCrosshair;
                currentObject.GetComponent<Rigidbody>().isKinematic = true;
                currentObject.GetComponent<PhotonView>().RequestOwnership();
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
        }
        else if (canGrabby && currentObject.transform.GetChild(0).tag == "gun")
        {
            if (canGrabby && Input.GetMouseButton(0))
            {
                crosshair.GetComponent<Image>().sprite = pickedUpCrosshair;
                currentObject.GetComponent<Rigidbody>().isKinematic = true;
                currentObject.GetComponent<PhotonView>().RequestOwnership();
                currentObject.transform.parent = playerCameraPivot.transform.Find("objectTarget");
                currentObject.transform.position = playerCameraPivot.transform.Find("objectTarget").position;
                currentObject.transform.rotation = Quaternion.identity;
                holdingGun = true;
                grabby = true;
            }
            else if (grabby && Input.GetMouseButtonUp(0) && currentObject != null)
            {
                currentObject.GetComponent<Rigidbody>().isKinematic = false;
                currentObject.transform.parent = null;
                holdingGun = false;
                grabby = false;
            }
        }

        // drop override
        if (driving && currentObject != null)
        {
            currentObject.GetComponent<Rigidbody>().isKinematic = false;
            currentObject.transform.parent = null;
            grabby = false;
            holdingGun = false;
        }
    }

    public void drive()
    {
        // check if player is looking at car
        if (Physics.Raycast(playerCameraPivot.transform.position, playerCameraPivot.transform.forward, out hit, Mathf.Infinity, clickMask))
        {
            if (hit.collider.transform.CompareTag("seat") && !driving && currentDriver == null)
            {
                crosshair.GetComponent<Image>().sprite = carCrosshair;
                if (Input.GetMouseButtonDown(0))
                {
                    currentCar = hit.transform.gameObject;
                    currentCar.GetComponent<PhotonView>().RequestOwnership();
                    currentDriver = gameObject;
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
            currentDriver = null;
            transform.parent = null;
            GetComponent<Rigidbody>().isKinematic = false;
            transform.position += new Vector3(0, 5, 0);
            GetComponent<CapsuleCollider>().isTrigger = false;
            currentCar.GetComponent<SimpleCarController>().enabled = false;
            currentCar = null;
        }
        if (driving && Input.GetKey(KeyCode.Space))
        {
            currentCar.transform.Find("wheels").Find("frontLeft").GetComponent<WheelCollider>().brakeTorque = 10000;
            currentCar.transform.Find("wheels").Find("frontRight").GetComponent<WheelCollider>().brakeTorque = 10000;
            currentCar.transform.Find("wheels").Find("rearLeft").GetComponent<WheelCollider>().brakeTorque = 10000;
            currentCar.transform.Find("wheels").Find("rearRight").GetComponent<WheelCollider>().brakeTorque = 10000;
        }
        else if (driving && Input.GetKeyUp(KeyCode.Space))
        {
            currentCar.transform.Find("wheels").Find("frontLeft").GetComponent<WheelCollider>().brakeTorque = 0;
            currentCar.transform.Find("wheels").Find("frontRight").GetComponent<WheelCollider>().brakeTorque = 0;
            currentCar.transform.Find("wheels").Find("rearLeft").GetComponent<WheelCollider>().brakeTorque = 0;
            currentCar.transform.Find("wheels").Find("rearRight").GetComponent<WheelCollider>().brakeTorque = 0;
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

    public void handlePause()
    {
        if (!isPaused && Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            foreach (GameObject ch in playerCrosshairs)
            {
                if (ch != crosshair && ch != null)
                {
                    Destroy(ch);
                }
            }
            isPaused = true;
            pauseMenu.SetActive(true);
        }

        if (!isPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;
        }
        else if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
        }
    }

    public void gunControl()
    {
        if (holdingGun)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RaycastHit hit;
                if (Physics.Raycast(currentObject.transform.GetChild(1).transform.position, currentObject.transform.GetChild(1).transform.forward, out hit, Mathf.Infinity))
                {
                    Debug.Log(hit.collider.transform.name);
                }
            }
        }
    }

    [PunRPC]
    void GetColor(float r, float g, float b)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }

    [PunRPC]
    void UpdatePlayerColor(float r, float g, float b)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }
}
