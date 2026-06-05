using UnityEngine;

public class MonitorInput : MonoBehaviour
{
    /*
     * This script is responsible for collecting input for camera movement in the case of a keyboard and mouse setup.
     */
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] public bool inUse;
    [SerializeField] private bool mouseEnabled;
    [SerializeField] private float translationAcceleration;
    [SerializeField] private GameObject goalIndicator;

    private Vector3 translationInput;
    private Vector3 rotationInput;

    private bool pPressed;
    private bool initialized;

    private void Start()
    {
        pPressed = false;
    }
    private void Update()
    {
        Initialize();

        if (!inUse || !initialized) return;

        ReadInput();
        ApplyInput();

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (pPressed) return;

            Debug.Log("Setting goal text visibility: " + !goalIndicator.activeInHierarchy);
            bool activeInHeirarchy = goalIndicator.activeInHierarchy;
            goalIndicator.SetActive(!activeInHeirarchy);

            pPressed = true;
            Invoke("SetPPressed", 0.1f);
        }
    }

    private void Initialize()
    {
        if (initialized || !cameraMovement.initialized) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        initialized = true;
    }

    public void ReadInput()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            rotationInput = Vector3.zero;
            gameManager.UIManager.TogglePause(!gameManager.Paused);
        }

        if (gameManager.Paused) return;

        translationInput = new Vector3
            (
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Jump"),
            Input.GetAxis("Vertical")
            );

        //Debug.Log("Translation Input: " + translationInput);

        if (!mouseEnabled) return;

        float rollInput = 0f;
        if (Input.GetKey(KeyCode.Q)) rollInput = 1f;
        else if (Input.GetKey(KeyCode.E)) rollInput = -1f;

        rotationInput = new Vector3
            (
            gameManager.LookInversionMultiplier * Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X"),
            rollInput
            );
    }

    private void SetPPressed()
    {
        pPressed = false;
    }

    public void ApplyInput()
    {
        cameraMovement.Acceleration = translationInput * translationAcceleration;
        cameraMovement.AngularVelocity = rotationInput;
    }

    public bool InUse
    {
        get { return inUse; }
        set { inUse = value; }
    }
}
