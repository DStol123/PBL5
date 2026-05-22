using UnityEngine;

public class VRInput : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] public bool inUse;
    
    void Update()
    {
        if (!inUse) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameManager.UIManager.TogglePause(!gameManager.Paused);
        }

        OVRInput.Update();

        TranslationInput();
        RotationInput();
    }

    private void TranslationInput()
    {
        Vector3 translationInput = Vector3.zero;

        Vector2 mainThumbstickAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        float verticalInput = OVRInput.Get(OVRInput.Button.One) ? 1f : OVRInput.Get(OVRInput.Button.Two) ? -1f : 0f;

        translationInput = new Vector3(
            mainThumbstickAxis.x,
            verticalInput,
            mainThumbstickAxis.y
            );

        translationInput *= cameraMovement.MaximumAcceleration;

        cameraMovement.Acceleration = translationInput;
    }

    private void RotationInput()
    {
        Vector3 rotationInput = Vector3.zero;

        Vector2 secondaryThumbstickAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        float rollInput = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) ? 1f : OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) ? -1f : 0f;

        rotationInput = new Vector3(
            gameManager.LookInversionMultiplier * secondaryThumbstickAxis.y,
            secondaryThumbstickAxis.x,
            rollInput
            );

        cameraMovement.AngularVelocity = rotationInput;
    }

    public bool InUse
    {
        get { return inUse; }
        set { inUse = value; }
    }
}
