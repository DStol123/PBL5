using Unity.VisualScripting;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    /*
     * This script is responsible for the physical movement of the camera within the scene.
     * Control and input is collected in a different script.
     */
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Vector3 acceleration;
    [SerializeField] private float maximumAcceleration;
    [SerializeField] private float maximumSpeed;
    [SerializeField] private Vector3 maximumAngularAcceleration; //Around each axis
    [SerializeField] private Vector3 rotationStrength;
    [SerializeField] private Vector3 angularVelocity;
    [SerializeField] private float breakingAcceleration;
    [SerializeField] private float movementFovMultiplier;
    [SerializeField] private GameObject vrCamera;
    [SerializeField] private Transform cameraOffset;

    private Quaternion headsetRotationAtCalibration;

    [SerializeField] private FOVChanger fovChanger;

    [SerializeField] private CaveData caveData;

    public bool initialized;

    private int distanceCounter;
    private float distanceTotal;

    private void Start()
    {
        distanceCounter = 0;
        distanceTotal = 0;

        if (fovChanger != null) return;

        fovChanger = GetComponentInChildren<FOVChanger>();
    }

    private void Update()
    {
        if (!initialized) return;

        if (Input.GetKeyDown(KeyCode.C)) Calibrate();

        ApplyMovement();
        
        BreakOnNoInput();

        if(fovChanger != null) fovChanger.ChangeFOV(180f / (movementFovMultiplier * Mathf.Abs(rb.linearVelocity.magnitude) + 1f));

        //Debug.Log("Current Speed: " + rb.linearVelocity.magnitude);
    }

    public void Initialize()
    {
        transform.parent.transform.position = new Vector3(gameManager.caveWidth / 2f, gameManager.caveHeight / 2f, gameManager.caveDepth / 2f);

        initialized = true;
    }

    public void StartTimeSeries()
    {
        InvokeRepeating(nameof(SetAverageDistanceToPath), 1f, 1f);
    }

    public void SetAverageDistanceToPath()
    {
        float currentDistance = DistanceFromPath();

        distanceTotal += currentDistance;
        distanceCounter++;

        gameManager.averageDistanceFromPath = distanceTotal / distanceCounter;

        gameManager.Recorder.UpdateHistogram(distanceCounter, currentDistance, gameManager.participant, DistanceAlongPath(), Mathf.Clamp(100f - transform.position.y, 0f, 100f));
    }

    public float DistanceAlongPath()
    {
        var path = gameManager.caveData.pathToBottom;
        if (path == null || path.Count < 2) return 0f;

        float bestDistance = float.MaxValue;
        float bestAlong = 0f;

        float cumulative = 0f;

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 a = path[i - 1].transform.position;
            Vector3 b = path[i].transform.position;

            Vector3 ab = b - a;
            Vector3 ap = transform.position - a;

            float t = Vector3.Dot(ap, ab) / ab.sqrMagnitude;
            float clampedT = Mathf.Clamp01(t);

            Vector3 closestPoint = a + ab * clampedT;
            float dist = (transform.position - closestPoint).sqrMagnitude;

            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestAlong = cumulative + ab.magnitude * clampedT;
            }

            cumulative += ab.magnitude;
        }

        return bestAlong;
    }

    public void Calibrate()
    {
        Quaternion previousRotation = transform.rotation;

        transform.rotation = vrCamera.transform.rotation;

        Quaternion rotationDifference = transform.rotation * Quaternion.Inverse(previousRotation);

        cameraOffset.rotation = rotationDifference * cameraOffset.rotation;
    }

    private void ApplyMovement()
    {
        cameraOffset.localPosition = vrCamera.transform.localPosition * -1f;

        rb.AddForce(vrCamera.transform.rotation * acceleration);

        Quaternion headsetRotation = vrCamera.transform.rotation;

        Vector3 right = headsetRotation * Vector3.right;
        Vector3 up = headsetRotation * Vector3.up;
        Vector3 forward = headsetRotation * Vector3.forward;

        transform.Rotate(right, angularVelocity.x * Time.deltaTime, Space.World);
        transform.Rotate(up, angularVelocity.y * Time.deltaTime, Space.World);
        transform.Rotate(forward, angularVelocity.z * Time.deltaTime, Space.World);

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maximumSpeed);

        //Debug.Log("Adding force: " + acceleration + "\nNew Speed: " + rb.linearVelocity);
    }

    private void BreakOnNoInput()
    {
        Quaternion headsetRotation = vrCamera.transform.rotation;

        Vector3 localVelocity =
            Quaternion.Inverse(headsetRotation) * rb.linearVelocity;

        localVelocity = new Vector3(
            Mathf.Lerp(localVelocity.x, 0f, acceleration.x == 0f ? breakingAcceleration * Time.deltaTime : 0f),
            Mathf.Lerp(localVelocity.y, 0f, acceleration.y == 0f ? breakingAcceleration * Time.deltaTime : 0f),
            Mathf.Lerp(localVelocity.z, 0f, acceleration.z == 0f ? breakingAcceleration * Time.deltaTime : 0f)
        );

        rb.linearVelocity = headsetRotation * localVelocity;
    }

    public float DistanceFromPath()
    {
        Node[] nodes = ClosestNodePairOnPath();

        if (nodes[1] == null) return Mathf.Abs((nodes[0].transform.position - transform.position).magnitude);

        Vector3 line = nodes[1].transform.position - nodes[0].transform.position;

        float distanceAlongLine = Vector3.Dot(line, transform.position - nodes[0].transform.position) / line.sqrMagnitude;
        distanceAlongLine = Mathf.Clamp01(distanceAlongLine);

        Vector3 positionAlongLine = nodes[0].transform.position + line * distanceAlongLine;

        return Mathf.Abs((positionAlongLine - transform.position).magnitude);
    }

    private Node[] ClosestNodePairOnPath()
    {
        Node[] nodes = new Node[2];
        float closest = float.MaxValue;

        Node firstNode = null;

        foreach(Node node in gameManager.caveData.pathToBottom)
        {
            float distance = Mathf.Abs((node.transform.position - transform.position).magnitude);
            if (distance < closest)
            {
                closest = distance;
                firstNode = node;
            }
        }

        nodes[0] = firstNode;

        closest = float.MaxValue;

        Vector3 firstNodeRelativePosition = firstNode.transform.position - transform.position;

        Node secondNode = null;

        foreach (Node node in gameManager.caveData.pathToBottom)
        {
            if (node == firstNode) continue;

            if (!node.InLineOfSight(firstNode)) continue;

            Vector3 relativePos = node.transform.position - transform.position;

            if (Vector3.Dot(firstNodeRelativePosition, relativePos) > 0f) continue;

            if(Mathf.Abs((relativePos).magnitude) < closest)
            {
                closest = Mathf.Abs(relativePos.magnitude);
                secondNode = node;
            }
        }

        nodes[1] = secondNode;

        return nodes;
    }

    public Vector3 Acceleration
    {
        get {  return acceleration; }
        set {  acceleration = Vector3.ClampMagnitude(value, maximumAcceleration); }
    }

    public Vector3 AngularVelocity
    {
        get { return angularVelocity; }
        set
        {
            angularVelocity = new Vector3(
                Mathf.Clamp(value.x * rotationStrength.x, -maximumAngularAcceleration.x, maximumAngularAcceleration.x),
                Mathf.Clamp(value.y * rotationStrength.y, -maximumAngularAcceleration.y, maximumAngularAcceleration.y),
                Mathf.Clamp(value.z * rotationStrength.z, -maximumAngularAcceleration.z, maximumAngularAcceleration.z)
            );
        }
    }

    public float MaximumAcceleration
    {
        get { return maximumAcceleration; }
    }

    public CaveData CaveData
    {
        get { return caveData; }
        set { caveData = value; }
    }
}
