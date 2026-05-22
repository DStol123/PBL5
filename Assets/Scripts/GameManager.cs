using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private bool paused;
    private bool lookInverted;
    [SerializeField] private float lookInversionMultiplier;

    [SerializeField] private float timeToBottom;
    [SerializeField] private float timeToReturn;

    [SerializeField] private int vrExperience;

    [SerializeField] private Recording recorder;

    [SerializeField] private GameObject caveGenerator;

    [SerializeField] private Image loadingBar;

    public float caveFillPercent;
    public int caveWidth;
    public int caveHeight;
    public int caveDepth;
    public string caveSeed;

    private void Start()
    {

    }

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

    public void GenerateCave()
    {
        GameObject go = Instantiate(caveGenerator);

        RandomWalker rw = go.GetComponentInChildren<RandomWalker>();

        rw.GameManager = this;
        rw.Seed = caveSeed;

        CaveData cd = go.GetComponentInChildren<CaveData>();
        cd.Dimensions = new Vector3(caveWidth, caveHeight, caveDepth);
        cd.LoadingBar = loadingBar;
        cd.GameManager = this;

        cd.InitializeVertices();

        rw.Initialize();

        uiManager.EnableMenu(3);
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

    public void WriteResults()
    {
        
        recorder.WriteTrialLog(vrExperience, timeToBottom, timeToReturn);
    }

    public float TimeToReturn
    {
        get { return timeToReturn; }
        set { timeToReturn = value; }
    }

    public float TimeToBottom
    {
        get { return timeToBottom; }
        set { timeToBottom = value; }
    }

    public string CaveFill
    {
        get { return caveFillPercent.ToString(); }
        set { caveFillPercent = float.Parse(value); }
    }

    public string Width
    {
        get { return caveWidth.ToString(); }
        set { caveWidth = int.Parse(value); }
    }
    public string Height
    {
        get { return caveHeight.ToString(); }
        set { caveHeight = int.Parse(value); }
    }
    public string Depth
    {
        get { return caveDepth.ToString(); }
        set { caveDepth = int.Parse(value); }
    }

    public string Seed
    {
        get { return caveSeed; }
        set { caveSeed = value; }
    }
}
