using UnityEngine;

public class VRInput : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private MonitorInput monitorInput;
    [SerializeField] public bool inUse;

    private void Start()
    {
        inUse = true;
    }

    void Update()
    {
        if (!inUse) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameManager.UIManager.TogglePause(!gameManager.Paused);
        }

        

        if (monitorInput.InUse)
        {
            monitorInput.ReadInput();
            monitorInput.ApplyInput();
        }
    }

    private void TranslationInput()
    {
        
    }

    private void RotationInput()
    {
        
    }

    public bool InUse
    {
        get { return inUse; }
        set { inUse = value; }
    }
}
