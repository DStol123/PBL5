using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadInput : MonoBehaviour
{
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private bool inUse;

    private Vector3 moveInput;
    private Vector3 lookInput;

    private void Update()
    {
        if (!inUse) return;

        ReadInput();
        ApplyInput();
    }

    private void ReadInput()
    {
        Vector2 leftStickInput = Gamepad.current.leftStick.ReadValue();

        float leftTriggerInput = Gamepad.current.leftTrigger.ReadValue();
        float rightTriggerInput = Gamepad.current.rightTrigger.ReadValue();

        float verticalInput = leftTriggerInput > 0f ? -leftTriggerInput :
            rightTriggerInput > 0f ? rightTriggerInput : 0f;

        moveInput = new Vector3(
            leftStickInput.x,
            leftStickInput.y,
            verticalInput);

        Vector2 rightStickInput = Gamepad.current.rightStick.ReadValue();

        float leftShoulderInput = Gamepad.current.leftShoulder.ReadValue();
        float rightShoulderInput = Gamepad.current.rightShoulder.ReadValue();

        float rollInput = leftShoulderInput > 0f ? -leftShoulderInput :
            rightShoulderInput > 0f ? rightShoulderInput : 0f;

        lookInput = new Vector3(
            rightStickInput.y,
            rightStickInput.x,
            rollInput
            );
    }

    private void ApplyInput()
    {
        cameraMovement.Acceleration = moveInput;
        cameraMovement.AngularVelocity = lookInput;
    }
}
