using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadUICycler : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    private int currentUI;
    private bool canCycle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentUI = 0;
        canCycle = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Gamepad.current.buttonNorth.value > 0.5f && canCycle)
        {
            CycleIndex();

            canCycle = false;
            Invoke("EnableCycling", 0.3f);

            ToggleUIByIndex();
        }
    }

    private void CycleIndex()
    {
        if (currentUI < 3) currentUI++;

        else currentUI = 0;
    }

    private void ToggleUIByIndex()
    {
        switch(currentUI)
        {
            case 0:
                uiManager.ToggleCage(false);
                uiManager.ToggleEgocentricMinimap(false);
                uiManager.ToggleExocentricMinimap(false);
                return;

            case 1:
                uiManager.ToggleCage(true);
                uiManager.ToggleExocentricMinimap(false);
                uiManager.ToggleEgocentricMinimap(false);
                return;

            case 2:
                uiManager.ToggleCage(false);
                uiManager.ToggleExocentricMinimap(true);
                return;

            case 3:
                uiManager.ToggleCage(false);
                uiManager.ToggleEgocentricMinimap(true);
                return;
                
            default: 
                return;
        }
    }

    public void EnableCycling()
    {
        canCycle = true;
    }
}
