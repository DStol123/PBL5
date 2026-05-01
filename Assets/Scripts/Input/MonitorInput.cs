using UnityEngine;

public class MonitorInput : MonoBehaviour
{
    /*
     * This script is responsible for collecting input for camera movement in the case of a keyboard and mouse setup.
     */
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private bool inUse;
    [SerializeField] private float translationAcceleration;

    private Vector3 translationInput;
    private Vector3 rotationInput;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        if (!inUse) return;

        ReadInput();
        ApplyInput();
    }

    private void ReadInput()
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

    private void ApplyInput()
    {
        cameraMovement.Acceleration = translationInput * translationAcceleration;
        cameraMovement.AngularVelocity = rotationInput;
    }

    public bool Enabled
    {
        get { return inUse; }
        set { inUse = value; }
    }
}
