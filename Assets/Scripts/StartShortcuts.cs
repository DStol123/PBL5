using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StartShortcuts : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] GamepadInput gamepadInput;

    [SerializeField] private float minTimeUntilNextPress;
    private bool canPressBubbleToggle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canPressBubbleToggle = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            HandleBubbleToggleShortcut();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            gameManager.GenerateCave();
            Destroy(this);
        }

        if (!gamepadInput.InUse || Gamepad.all.Count == 0) return;

        if(Gamepad.current.buttonWest.ReadValue() > 0.5f && canPressBubbleToggle)
        {
            HandleBubbleToggleShortcut();
            canPressBubbleToggle = false;

            Invoke("ReenableBubbleToggle", minTimeUntilNextPress);
        }

        if(Gamepad.current.buttonSouth.ReadValue() > 0.5f)
        {
            gameManager.GenerateCave();
            Destroy(this);
        }
    }

    private void HandleBubbleToggleShortcut()
    {
        if(gameManager.bubbleEmitters)
        {
            gameManager.bubbleEmitters = false;

            return;
        }

        gameManager.bubbleEmitters = true;
    }

    public void ReenableBubbleToggle()
    {
        canPressBubbleToggle = true;
    }
}
