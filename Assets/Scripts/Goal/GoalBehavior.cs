using NUnit.Framework.Interfaces;
using UnityEngine;

public class GoalBehavior : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private bool reached;

    [SerializeField] private float timeToReach;
    [SerializeField] private float timeElapsed;
    [SerializeField] private float timeAtStart;

    [SerializeField] private bool active;

    [SerializeField] private GoalBehavior returnGoal;
    [SerializeField] private GoalBehavior endGoal;

    [SerializeField] private SphereCollider sphereCollider;

    private void Start()
    {
        if (sphereCollider == null) return;

        sphereCollider.isTrigger = true;
    }

    private void Update()
    {
        if (reached || !active) return;

        timeElapsed = Time.time - timeAtStart;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !active || reached) return;

        reached = true;
        timeToReach = timeElapsed;

        if (returnGoal == null)
        {
            gameManager.TimeToReturn = timeToReach;

            gameManager.WriteResults();

            gameManager.UIManager.GoalText.text = "All Goals Complete.";
            return;
        }

        gameManager.UIManager.GoalText.text = "Goal: Return to the Entrance of the Cave.";

        gameManager.TimeToBottom = timeToReach;
        returnGoal.Active = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || returnGoal != null) return;

        gameManager.UIManager.GoalText.text = "Goal: Touch the Golden Sphere at the Bottom of the Cave.";

        endGoal.Active = true;

        Active = true;
    }

    public bool Active
    {
        get { return active; }
        set { 
            if(!active && value)
            {
                timeAtStart = Time.time;
                reached = false;
            }
            active = value;
        }
    }

    public bool Reached
    {
        get { return reached; }
        set { reached = value; }
    }

    public SphereCollider SphereCollider
    {
        get { return sphereCollider; }
        set { sphereCollider = value; }
    }

    public GoalBehavior ReturnGoal
    {
        get { return returnGoal; }
        set { returnGoal = value; }
    }

    public GameManager GameManager
    {
        get { return gameManager; }
        set { gameManager = value; }
    }

    public GoalBehavior EndGoal
    {
        get { return endGoal; }
        set { endGoal = value; }
    }
}
