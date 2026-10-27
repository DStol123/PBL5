using Unity.Hierarchy;
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

    [SerializeField] private float emitterDensity;

    [SerializeField] private int vrExperience;

    [SerializeField] private Recording recorder;

    [SerializeField] private GameObject caveGenerator;

    [SerializeField] private Image loadingBar;

    [SerializeField] private CameraMovement cameraMovement;

    [SerializeField] private HuntManager huntManager;

    private float timeAtLoad;

    public float caveFillPercent;
    public int caveWidth;
    public int caveHeight;
    public int caveDepth;
    public string caveSeed;
    public float maxTunnelLength;
    public float maxRadius;
    public float minRadius;
    public bool bubbleEmitters;
    public float minTunnelLength;
    public float averageDistanceFromPath;

    public float pathLength;
    public int pathNodes;

    [SerializeField] private int maxNodeNeighbors;

    public string participant;

    public CaveData caveData;

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

    public void NauseaCall()
    {
        if(timeToBottom == 0f)
        {
            timeToBottom = 9999f;
            timeToReturn = 9999f;
        }
        else if(timeToReturn == 0f)
        {
            timeToReturn = 9999f;
        }

        recorder.WriteTrialLog(participant, vrExperience, timeToBottom, timeToReturn, true, Time.time - timeAtLoad, bubbleEmitters, uiManager.ArtificialHorizon.activeInHierarchy, false);

        QuitGame();
    }

    [ContextMenu("Generate Cave")]
    public void GenerateCave()
    {
        GameObject go = Instantiate(caveGenerator);

        caveData = go.GetComponentInChildren<CaveData>();
        caveData.Dimensions = new Vector3(caveWidth, caveHeight, caveDepth);
        caveData.LoadingBar = loadingBar;
        caveData.GameManager = this;
        caveData.MaxNodeNeighbors = maxNodeNeighbors;

        caveData.InitializeVertices();

        RandomWalker rw = go.GetComponentInChildren<RandomWalker>();

        rw.GameManager = this;
        rw.Seed = caveSeed;
        rw.MaxLength = maxTunnelLength;
        rw.minSectionLength = minTunnelLength;
        rw.MaxRadius = maxRadius;
        rw.ScavengerManager.HuntManager = huntManager;

        rw.Initialize();

        cameraMovement.CaveData = caveData;
        cameraMovement.Initialize();
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

    public CameraMovement CameraMovement
    { 
        get { return cameraMovement; }
        set {  cameraMovement = value; }
    }

    public void WriteResults()
    {
        recorder.WriteTrialLog(participant, vrExperience, timeToBottom, timeToReturn, false, 0f, bubbleEmitters, uiManager.ArtificialHorizon.activeInHierarchy, false); ;
    }

    public Recording Recorder
    {
        get { return recorder; }
        set { recorder = value; }
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

    public string MaxLength
    {
        get { return maxTunnelLength.ToString(); }
        set { maxTunnelLength = float.Parse(value); }
    }

    public string MaxRadius
    {
        get { return maxRadius.ToString(); }
        set { maxRadius = float.Parse(value); }
    }

    public string MinRadius
    {
        get { return minRadius.ToString(); }
        set { minRadius = float.Parse(value); }
    }

    public float VRExperience
    {
        get { return (float)vrExperience; }
        set { vrExperience = Mathf.CeilToInt(value); }
    }

    public float TimeAtLoad
    {
        get { return timeAtLoad; }
        set { timeAtLoad = value; }
    }

    public bool BubbleEmitters
    {
        get { return bubbleEmitters; }
        set { bubbleEmitters = value; }
    }

    public float EmitterDensity
    {
        get { return emitterDensity; }
    }

    public HuntManager HuntManager
    {
        get { return huntManager; }
    }
}
