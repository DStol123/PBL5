using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private bool paused;
    private bool lookInverted;
    [SerializeField] private float lookInversionMultiplier;

    public UIManager UIManager
    { get { return uiManager; } }

    public bool Paused
    {
        get { return paused; }
        set {
            if (value) Time.timeScale = 0f;
            else Time.timeScale = 1f;

            paused = value;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public bool LookInverted
    {
        get { return lookInverted; }
        set
        {
            if (value) lookInversionMultiplier = 1;
            else lookInversionMultiplier = -1;

            lookInverted = value;
        }
    }

    public float LookInversionMultiplier
    {
        get { return lookInversionMultiplier; }
    }
}
