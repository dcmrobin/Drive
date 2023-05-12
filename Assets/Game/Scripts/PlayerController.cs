using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public Text nickname;
    public string ncknm;
    public TMP_Text screenspaceHealthNum;
    public TMP_Text coordsText;
    public Slider healthbar;
    public Slider Screenspacehealthbar;
    //GameObject[] playerCrosshairs;
    public GameObject[] playerCanvases;
    public GameObject[] allCars;
    public GameObject[] allGuns;
    public GameObject[] allPlayers;
    public GameObject screenspaceCanvas;
    public GameObject playerModel;
    public GameObject magazineModel;
    public GameObject objTarget;
    public GameObject gunTarget;
    public GameObject pauseMenu;
    public GameObject crosshair;
    public Sprite normalCrosshair;
    public Sprite carCrosshair;
    public Sprite canGetCrosshair;
    public Sprite pickupableCrosshair;
    public Sprite pickedUpCrosshair;
    public Sprite getInCrosshair;
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 7f;
    public float lookSensitivity = 3f;
    public float maxLookAngle = 90f;
    public float groundDistance = 0.1f;
    public float maxHealth = 100;
    public float health;
    public int amountOfMagazines;
    public LayerMask groundMask;
    public LayerMask clickMask;
    public LayerMask camCollideMask;
    public float gunImpactForce = 30;
    /*public LayerMask seatMask;
    public LayerMask pickupableMask;
    public LayerMask doorMask;*/
    RaycastHit hit;

    public GameObject playerCameraPivot;
    private Rigidbody rb;
    public bool isGrounded;
    private float lookX;
    private float lookY;
    public bool driving;
    public bool inBoot;
    public bool inSeat;
    public GameObject currentCar;
    public GameObject currentObject;
    public GameObject currentGun;
    public GameObject currentDoor;
    public bool canGrabby;
    public bool grabby;
    public bool holdingGun;
    public bool thirdPersonViewActive = false;
    public bool isPaused = false;
    public bool isRunning = false;
    public PhotonView pv;
    int screenshotNum = 0;
    public GameObject lobbyController;
    public GameObject gunImactEffect;

    GameObject[] allCurSeats;

    public ExitGames.Client.Photon.Hashtable myProperties;
    void Start()
    {
        myProperties = new ExitGames.Client.Photon.Hashtable();
        health = maxHealth;
        healthbar.maxValue = maxHealth;
        Screenspacehealthbar.maxValue = maxHealth;
        lobbyController = GameObject.FindGameObjectWithTag("lobbyController");
        allPlayers = GameObject.FindGameObjectsWithTag("Player");
        rb = GetComponent<Rigidbody>();
        if (pv.Owner != null)
        {
            nickname.text = pv.Owner.NickName;
        }
        myProperties.Add("health", health);

        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i].GetComponent<PlayerController>().amountOfMagazines > 0 && !allPlayers[i].GetComponent<PlayerController>().magazineModel.activeSelf)
            {
                allPlayers[i].GetComponent<PlayerController>().pv.RPC("ToggleAmmoModel", RpcTarget.All, true);
            }
            else if (allPlayers[i].GetComponent<PlayerController>().amountOfMagazines <= 0 && allPlayers[i].GetComponent<PlayerController>().magazineModel.activeSelf)
            {
                allPlayers[i].GetComponent<PlayerController>().pv.RPC("ToggleAmmoModel", RpcTarget.All, false);
            }
        }
    }

    void OnConnectionFail(DisconnectCause cause)
    {
        Debug.Log(cause);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        pv.RPC("UpdateAmtMags", RpcTarget.All, amountOfMagazines);
        if (currentObject != null)
        {
            currentObject.GetComponent<PhotonView>().RPC("UpdateParent", RpcTarget.All, true, pv.ViewID);
        }
        if (currentGun != null)
        {
            currentGun.GetComponent<PhotonView>().RPC("UpdateParent", RpcTarget.All, true, pv.ViewID);
        }

        if (currentDoor != null)
        {
            if (currentDoor.GetComponent<CarDoor>().status == CarDoor.stat.Closed)
            {
                currentDoor.GetComponent<CarDoor>().status = CarDoor.stat.Open;
                currentDoor.GetComponent<PhotonView>().RPC("UpdateDoorValue", RpcTarget.All, true);
            }
            else if (currentDoor.GetComponent<CarDoor>().status == CarDoor.stat.Open)
            {
                currentDoor.GetComponent<CarDoor>().status = CarDoor.stat.Closed;
                currentDoor.GetComponent<PhotonView>().RPC("UpdateDoorValue", RpcTarget.All, false);
            }
        }

        if (currentCar != null && driving)
        {
            currentCar.GetComponent<PhotonView>().RPC("UpdateDriver", RpcTarget.All, true, pv.ViewID);
        }
    }

    void Update()
    {
        //coordsText.text = "X: " + transform.position.x + "Y: " + transform.position.y + "Z: " + transform.position.z;
        healthbar.value = health;
        Screenspacehealthbar.value = health;
        screenspaceHealthNum.text = health.ToString();
        allCars = GameObject.FindGameObjectsWithTag("car");
        allGuns = GameObject.FindGameObjectsWithTag("gun");
        //playerCrosshairs = GameObject.FindGameObjectsWithTag("crosshair");
        playerCanvases = GameObject.FindGameObjectsWithTag("playerScreenspaceCanvas");
        foreach (GameObject c in playerCanvases)
        {
            if (c != screenspaceCanvas)
            {
                if (pv.IsMine)
                {
                    Destroy(c);
                    pv.RPC("UpdateHealth", RpcTarget.All, health);
                }
            }
        }
        // get the horizontal and vertical mouse input
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        if (pv.IsMine)
        {
            // show the small magazine on the player's belt?
            if (lobbyController != null && lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Multiplayer)
            {
                if (amountOfMagazines > 0 && !magazineModel.activeSelf)
                {
                    pv.RPC("ToggleAmmoModel", RpcTarget.All, true);
                }
                else if (amountOfMagazines <= 0 && magazineModel.activeSelf)
                {
                    pv.RPC("ToggleAmmoModel", RpcTarget.All, false);
                }
            }
            else if (lobbyController != null && lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Singleplayer)
            {
                if (amountOfMagazines > 0 && !magazineModel.activeSelf)
                {
                    magazineModel.SetActive(true);
                }
                else if (amountOfMagazines <= 0 && magazineModel.activeSelf)
                {
                    magazineModel.SetActive(false);
                }
            }

            // anti-roll bars enabled?
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

            // show ammo?
            if (pauseMenu.GetComponent<PauseMenu>().gunAmmoVisible)
            {
                for (int i = 0; i < allGuns.Length; i++)
                {
                    if (allGuns[i].GetComponent<Gun>() != null)
                    {
                        allGuns[i].GetComponent<Gun>().ammoCanvas.SetActive(true);
                    }
                }
            }
            else if (!pauseMenu.GetComponent<PauseMenu>().gunAmmoVisible)
            {
                for (int i = 0; i < allGuns.Length; i++)
                {
                    if (allGuns[i].GetComponent<Gun>() != null)
                    {
                        allGuns[i].GetComponent<Gun>().ammoCanvas.SetActive(false);
                    }
                }
            }

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

            if (!isPaused)
            {
                // update the look angles
                lookX += mouseX;
                lookY = Mathf.Clamp(lookY - mouseY, -maxLookAngle, maxLookAngle);
        
                // rotate the player and camera based on the mouse input
                transform.localRotation = Quaternion.Euler(0f, lookX, 0f);
                playerCameraPivot.transform.localRotation = Quaternion.Euler(lookY, 0f, 0f);

                // execute methods
                pickUp();
                drive();
                door();
                thirdPersonControl();
                gunControl();
                gunReloadControl();
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

        if (pv.IsMine)
        {
            if (health <= 0)
            {
                Destroy(GameObject.Find("theCanvas"));
                Destroy(GameObject.Find("LobbyController"));
                Cursor.lockState = CursorLockMode.None;
                transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                transform.rotation = Quaternion.Euler(90, 0, 0);
                StartCoroutine(Die());
                //PhotonNetwork.Disconnect();
                //SceneManager.LoadScene(0);
            }
        }
    }

    public IEnumerator Die()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(0);
        PhotonNetwork.LeaveRoom();
    }

    public IEnumerator Hurt(Quaternion rotation)
    {
        yield return new WaitForSeconds(0.1f);
        playerCameraPivot.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z - 35);
    }

    void FixedUpdate()
    {
        if (pv.IsMine)
        {
            if (!driving && !inBoot)
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
            if (hit.collider.transform.CompareTag("pickupable") || hit.collider.transform.CompareTag("gun") || hit.collider.transform.CompareTag("gettable") && !driving)
            {
                currentObject = hit.transform.gameObject;
                if (hit.collider.transform.CompareTag("pickupable") || hit.collider.transform.CompareTag("gun"))
                {
                    crosshair.GetComponent<Image>().sprite = pickupableCrosshair;
                }
                else if (hit.collider.transform.CompareTag("gettable"))
                {
                    crosshair.GetComponent<Image>().sprite = canGetCrosshair;
                }
                canGrabby = true;
            }
            else if (!Physics.Raycast(playerCameraPivot.transform.position, playerCameraPivot.transform.forward, out hit, Mathf.Infinity, clickMask) || !hit.collider.transform.CompareTag("pickupable"))
            {
                currentObject = null;
                canGrabby = false;
            }
        }

        // grab the object
        if (canGrabby && currentObject.transform.tag != "gun" && currentObject.transform.tag != "gettable" && currentGun == null)
        {
            if (canGrabby && Input.GetMouseButtonDown(0))
            {
                currentObject.GetComponent<PhotonView>().RequestOwnership();
            }
            if (canGrabby && Input.GetMouseButton(0))
            {
                grabby = true;
                crosshair.GetComponent<Image>().sprite = pickedUpCrosshair;
                currentObject.GetComponent<Rigidbody>().isKinematic = true;
                currentObject.GetComponent<PhotonView>().RPC("UpdateRigidbody", RpcTarget.All, true, pv.ViewID);

                currentObject.transform.parent = objTarget.transform;
                currentObject.transform.position = objTarget.transform.position;
                currentObject.GetComponent<PhotonView>().RPC("UpdateParent", RpcTarget.All, true, pv.ViewID);
                currentObject.GetComponent<PhotonView>().RPC("UpdatePosition", RpcTarget.All, pv.ViewID);
            }
            else if (grabby && Input.GetMouseButtonUp(0) && currentObject != null && currentGun == null)
            {
                grabby = false;
                currentObject.GetComponent<Rigidbody>().isKinematic = false;
                currentObject.GetComponent<PhotonView>().RPC("UpdateRigidbody", RpcTarget.All, false, pv.ViewID);
                currentObject.transform.parent = null;
                currentObject.GetComponent<PhotonView>().RPC("UpdateParent", RpcTarget.All, false, pv.ViewID);
            }
        }
        else if (canGrabby && currentObject.transform.tag == "gun")
        {
            if (canGrabby && Input.GetMouseButtonDown(0))
            {
                currentGun = currentObject;
                currentGun.GetComponent<PhotonView>().RequestOwnership();
                currentGun.transform.parent = gunTarget.transform;
                currentGun.GetComponent<PhotonView>().RPC("UpdateGunParent", RpcTarget.All, true, pv.ViewID);
                currentGun.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            if (canGrabby && Input.GetMouseButton(0) && currentGun != null)
            {
                crosshair.GetComponent<Image>().sprite = pickedUpCrosshair;
                currentGun.GetComponent<Rigidbody>().isKinematic = true;
                currentGun.GetComponent<PhotonView>().RPC("UpdateRigidbody", RpcTarget.All, true, pv.ViewID);
                currentGun.transform.position = gunTarget.transform.position;
                currentGun.GetComponent<PhotonView>().RPC("UpdateGunPosition", RpcTarget.All, pv.ViewID);
                holdingGun = true;
                grabby = true;
            }
            else if (grabby && !Input.GetMouseButton(0) && currentGun != null)
            {
                currentGun.GetComponent<Rigidbody>().isKinematic = false;
                currentGun.GetComponent<PhotonView>().RPC("UpdateRigidbody", RpcTarget.All, false, pv.ViewID);
                currentGun.transform.parent = null;
                currentGun.GetComponent<PhotonView>().RPC("UpdateGunParent", RpcTarget.All, false, pv.ViewID);
                holdingGun = false;
                grabby = false;
                currentGun = null;
            }
        }
        else if (canGrabby && currentObject.transform.tag == "gettable")
        {
            if (canGrabby && Input.GetMouseButtonDown(0))
            {
                currentObject.GetComponent<PhotonView>().RequestOwnership();
                if (lobbyController != null && lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Multiplayer)
                {
                    if (currentObject.GetComponent<PhotonView>().AmOwner)
                    {
                        amountOfMagazines += 1;
                        PhotonNetwork.Destroy(currentObject);
                    }
                }
                else if (lobbyController != null && lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Singleplayer)
                {
                    amountOfMagazines += 1;
                    Destroy(currentObject);
                }
            }
        }

        // drop override
        //if (grabby)
        //{
            if (currentObject != null && Input.GetMouseButtonUp(0) || currentObject != null && driving)
            {
                currentObject.GetComponent<Rigidbody>().isKinematic = false;
                currentObject.GetComponent<PhotonView>().RPC("UpdateRigidbody", RpcTarget.All, false, pv.ViewID);
                currentObject.transform.parent = null;
                currentObject.GetComponent<PhotonView>().RPC("UpdateParent", RpcTarget.All, false, pv.ViewID);
                grabby = false;
                holdingGun = false;
            }
            else if (currentGun != null && !Input.GetMouseButton(0))
            {
                currentGun.GetComponent<Rigidbody>().isKinematic = false;
                currentGun.GetComponent<PhotonView>().RPC("UpdateRigidbody", RpcTarget.All, false, pv.ViewID);
                currentGun.transform.parent = null;
                currentGun.GetComponent<PhotonView>().RPC("UpdateParent", RpcTarget.All, false, pv.ViewID);
                grabby = false;
                holdingGun = false;
                currentGun = null;
            }
        //}
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
                    if (currentCar.GetComponent<SimpleCarController>().currentDriver == null)
                    {
                        inBoot = false;
                        driving = true;
                        inSeat = false;
                        currentCar.GetComponent<PhotonView>().RequestOwnership();
                        currentCar.GetComponent<PhotonView>().RPC("UpdateDriver", RpcTarget.All, true, pv.ViewID);
                        GetComponent<CapsuleCollider>().isTrigger = true;
                        pv.RPC("UpdatePlayerCollider", RpcTarget.All, true);
                        transform.parent = currentCar.transform.Find("driverSeatTarget");
                        GetComponent<Rigidbody>().isKinematic = true;
                        pv.RPC("UpdatePlayerRigidbody", RpcTarget.All, true);
                        transform.position = currentCar.transform.Find("driverSeatTarget").transform.position;
                        currentCar.GetComponent<SimpleCarController>().enabled = true;
                    }
                    else
                    {
                        if (currentCar.transform.Find("seatTarget").transform.childCount == 0)
                        {
                            inBoot = false;
                            driving = false;
                            inSeat = true;
                            GetComponent<CapsuleCollider>().isTrigger = true;
                            pv.RPC("UpdatePlayerCollider", RpcTarget.All, true);
                            transform.parent = currentCar.transform.Find("seatTarget");
                            GetComponent<Rigidbody>().isKinematic = true;
                            pv.RPC("UpdatePlayerRigidbody", RpcTarget.All, true);
                            transform.position = currentCar.transform.Find("seatTarget").transform.position;
                        }
                    }
                }
            }
            else if (hit.collider.transform.CompareTag("bootSeat") && !driving)
            {
                crosshair.GetComponent<Image>().sprite = getInCrosshair;
                if (Input.GetMouseButtonDown(0))
                {
                    pv.RPC("DeactivateHealthbar", RpcTarget.All, false);
                    currentCar = hit.collider.gameObject;
                    driving = false;
                    inBoot = true;
                    inSeat = false;
                    GetComponent<CapsuleCollider>().isTrigger = true;
                    pv.RPC("UpdatePlayerCollider", RpcTarget.All, true);
                    transform.parent = currentCar.transform.Find("bootSeatTarget");
                    GetComponent<Rigidbody>().isKinematic = true;
                    pv.RPC("UpdatePlayerRigidbody", RpcTarget.All, true);
                    transform.position = currentCar.transform.Find("bootSeatTarget").transform.position;
                }
            }
        }
        if (driving && Input.GetKeyDown(KeyCode.E))
        {
            currentCar.transform.Find("wheels").Find("frontLeft").GetComponent<WheelCollider>().brakeTorque = 0;
            currentCar.transform.Find("wheels").Find("frontRight").GetComponent<WheelCollider>().brakeTorque = 0;
            currentCar.transform.Find("wheels").Find("rearLeft").GetComponent<WheelCollider>().brakeTorque = 0;
            currentCar.transform.Find("wheels").Find("rearRight").GetComponent<WheelCollider>().brakeTorque = 0;
            inBoot = false;
            pv.RPC("DeactivateHealthbar", RpcTarget.All, true);
            driving = false;
            inSeat = false;
            currentCar.GetComponent<PhotonView>().RPC("UpdateDriver", RpcTarget.All, false, pv.ViewID);
            transform.parent = null;
            GetComponent<Rigidbody>().isKinematic = false;
            pv.RPC("UpdatePlayerRigidbody", RpcTarget.All, false);
            transform.position += new Vector3(0, 5, 0);
            GetComponent<CapsuleCollider>().isTrigger = false;
            pv.RPC("UpdatePlayerCollider", RpcTarget.All, false);
            currentCar.GetComponent<SimpleCarController>().enabled = false;
            currentCar = null;
        }
        else if (inBoot || inSeat && Input.GetKeyDown(KeyCode.E))
        {
            pv.RPC("DeactivateHealthbar", RpcTarget.All, true);
            driving = false;
            inBoot = false;
            inSeat = false;
            transform.parent = null;
            GetComponent<Rigidbody>().isKinematic = false;
            pv.RPC("UpdatePlayerRigidbody", RpcTarget.All, false);
            transform.position += new Vector3(0, 5, 0);
            GetComponent<CapsuleCollider>().isTrigger = false;
            pv.RPC("UpdatePlayerCollider", RpcTarget.All, false);
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
                    currentDoor.GetComponent<PhotonView>().RequestOwnership();
                    if (currentDoor.GetComponent<CarDoor>().status == CarDoor.stat.Closed)
                    {
                        currentDoor.GetComponent<CarDoor>().status = CarDoor.stat.Open;
                        currentDoor.GetComponent<PhotonView>().RPC("UpdateDoorValue", RpcTarget.All, true);
                    }
                    else if (currentDoor.GetComponent<CarDoor>().status == CarDoor.stat.Open)
                    {
                        currentDoor.GetComponent<CarDoor>().status = CarDoor.stat.Closed;
                        currentDoor.GetComponent<PhotonView>().RPC("UpdateDoorValue", RpcTarget.All, false);
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
            /*foreach (GameObject ch in playerCrosshairs)
            {
                if (ch != crosshair && ch != null)
                {
                    Destroy(ch);
                }
            }*/
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
        if (currentGun != null && currentGun.GetComponent<Gun>() != null && holdingGun && currentGun.GetComponent<Gun>().ammo > 0)
        {
            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;
                if (pv.IsMine)
                {
                    if (lobbyController != null && lobbyController.GetComponent<LobbyController>() != null && lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Multiplayer)
                    {
                        currentGun.GetComponent<PhotonView>().RPC("Shoot", RpcTarget.All, 1);
                        pv.RPC("ShootGun", RpcTarget.All);
                    }
                    else if (lobbyController != null && lobbyController.GetComponent<LobbyController>() != null && lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Singleplayer)
                    {
                        currentGun.GetComponent<Gun>().Shoot(1);
                        ShootGun();
                    }
                }
                if (currentGun != null)
                {
                    if (Physics.Raycast(playerCameraPivot.transform.position, playerCameraPivot.transform.forward, out hit, Mathf.Infinity))
                    {
                        GameObject impactEffect;

                        // get ownership of the target
                        if (hit.transform.GetComponent<PhotonView>() != null && hit.transform.CompareTag("pickupable"))
                        {
                            hit.transform.GetComponent<PhotonView>().RequestOwnership();
                        }

                        // spawn the impact effect
                        if (lobbyController != null && lobbyController.GetComponent<LobbyController>() != null && lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Multiplayer)
                        {
                            impactEffect = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", gunImactEffect.name), hit.point, Quaternion.LookRotation(hit.normal));
                            StartCoroutine(PhotonDestroy(impactEffect));
                        }
                        else if (lobbyController != null && lobbyController.GetComponent<LobbyController>() != null && lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Singleplayer)
                        {
                            impactEffect = Instantiate(gunImactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                            Destroy(impactEffect, 3f);
                        }

                        // add impact force to the target
                        if (hit.rigidbody != null)
                        {
                            hit.rigidbody.AddForce(-hit.normal * gunImpactForce);
                        }

                        // decrease health of the target if it isn't the player
                        if (hit.transform.GetComponent<Damageable>() != null)
                        {
                            if (lobbyController != null && lobbyController.GetComponent<LobbyController>() != null && lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Multiplayer)
                            {
                                hit.transform.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, currentGun.GetComponent<Gun>().damage);
                            }
                            else if (lobbyController != null && lobbyController.GetComponent<LobbyController>() != null && lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Singleplayer)
                            {
                                hit.transform.GetComponent<Damageable>().TakeDamage(currentGun.GetComponent<Gun>().damage);
                            }
                        }

                        // decrease health of the target if it is the player
                        if (hit.transform.CompareTag("Player") && hit.transform.GetComponent<PhotonView>() != null)
                        {
                            if (lobbyController != null && lobbyController.GetComponent<LobbyController>() != null && lobbyController.GetComponent<LobbyController>().gameMode == LobbyController.mode.Multiplayer)
                            {
                                hit.transform.GetComponent<PhotonView>().RPC("GetHurt", RpcTarget.All, currentGun.GetComponent<Gun>().damage);
                                //hit.transform.GetComponent<PlayerController>().GetHurt(currentGun.GetComponent<Gun>().damage);
                                //hit.transform.GetComponent<PhotonView>().Owner.SetCustomProperties(hit.transform.GetComponent<PlayerController>().myProperties);
                                //hit.transform.GetComponent<PlayerController>().healthbar.value = hit.transform.GetComponent<PlayerController>().health;
                            }
                        }
                    }
                }
            }
        }
    }

    public void gunReloadControl()
    {
        if (amountOfMagazines > 0 && currentGun != null)
        {
            int originalAmmo = currentGun.GetComponent<Gun>().maxAmmo;
            if (currentGun.GetComponent<Gun>().ammo <= 0)
            {
                if (currentGun.GetComponent<Gun>().reloadTimer < currentGun.GetComponent<Gun>().timeToReload)
                {
                    if (Input.GetKey(KeyCode.R))
                    {
                        currentGun.GetComponent<Gun>().reloadSlider.gameObject.SetActive(true);
                        currentGun.GetComponent<Gun>().reloadTimer += 0.25f;
                    }
                }
                if (currentGun.GetComponent<Gun>().reloadTimer >= currentGun.GetComponent<Gun>().timeToReload)
                {
                    amountOfMagazines -= 1;
                    currentGun.GetComponent<Gun>().reloadSlider.gameObject.SetActive(false);
                    currentGun.GetComponent<Gun>().ammo = originalAmmo;
                    currentGun.GetComponent<Gun>().reloadTimer = 0;
                }
            }
        }
    }

    IEnumerator PhotonDestroy(GameObject thingToDestroy)
    {
        yield return new WaitForSeconds(3f);
        PhotonNetwork.Destroy(thingToDestroy);
    }

    [PunRPC]
    void GetColor(float r, float g, float b)
    {
        playerModel.GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }

    [PunRPC]
    void UpdatePlayerColor(float r, float g, float b)
    {
        playerModel.GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }

    [PunRPC]
    void ShootGun()
    {
        if (currentGun != null && currentGun.transform.Find("muzzleFlash") != null && currentGun.transform.Find("muzzleFlash").GetComponent<ParticleSystem>() != null)
        {
            currentGun.transform.Find("muzzleFlash").GetComponent<ParticleSystem>().Play();
        }
    }

    [PunRPC]
    public void GetHurt(int amt)
    {
        health -= amt;
        Quaternion origRot = playerCameraPivot.transform.rotation;
        playerCameraPivot.transform.rotation = Quaternion.Euler(origRot.x, origRot.y, origRot.z + 35);
        StartCoroutine(Hurt(origRot));
    }

    [PunRPC]
    void UpdatePlayerRigidbody(bool boolean)
    {
        GetComponent<Rigidbody>().isKinematic = boolean;
    }

    [PunRPC]
    void UpdatePlayerCollider(bool boolean)
    {
        GetComponent<Collider>().isTrigger = boolean;
    }

    [PunRPC]
    void DeactivateHealthbar(bool boolean)
    {
        nickname.gameObject.SetActive(boolean);
        healthbar.gameObject.SetActive(boolean);
    }

    [PunRPC]
    void ToggleAmmoModel(bool boolean)
    {
        magazineModel.gameObject.SetActive(boolean);
    }

    [PunRPC]
    void UpdateAmtMags(int amt)
    {
        amountOfMagazines = amt;
    }

    [PunRPC]
    void UpdateHealth(float hlth)
    {
        health = hlth;
    }
}
