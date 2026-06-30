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
        if (fovChanger != null) return;

        distanceCounter = 0;
        distanceTotal = 0;

        fovChanger = GetComponentInChildren<FOVChanger>();
    }

    private void Update()
    {
        if (!initialized) return;
        
        Calibrate();

        ApplyMovement();
        
        BreakOnNoInput();

        fovChanger.ChangeFOV(180f / (movementFovMultiplier * Mathf.Abs(rb.linearVelocity.magnitude) + 1f));

        //Debug.Log("Current Speed: " + rb.linearVelocity.magnitude);
    }

    public void Initialize()
    {
        transform.position = new Vector3(gameManager.caveWidth / 2f, gameManager.caveHeight + 1f, gameManager.caveDepth / 2f);

        initialized = true;
    }

    public void SetAverageDistanceToPath()
    {
        distanceTotal += DistanceFromPath();
        distanceCounter++;

        gameManager.averageDistanceFromPath = distanceTotal / distanceCounter;

        Invoke("SetAverageDistanceToPath", 1f);
    }

    public void Calibrate()
    {
        Vector3 rotationDifference;

        Vector3 previousRotation = transform.rotation.eulerAngles;

        transform.rotation = vrCamera.transform.rotation;

        rotationDifference = transform.rotation.eulerAngles - previousRotation;

        cameraOffset.rotation = Quaternion.Euler(cameraOffset.rotation.eulerAngles - rotationDifference);
    }

    private void ApplyMovement()
    {
        rb.AddRelativeForce(acceleration);

        transform.Rotate(angularVelocity * Time.deltaTime);


        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maximumSpeed);

        //Debug.Log("Adding force: " + acceleration + "\nNew Speed: " + rb.linearVelocity);
    }

    private void BreakOnNoInput()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        rb.linearVelocity = transform.TransformDirection(new Vector3(
            Mathf.Lerp(localVelocity.x, 0f, acceleration.x == 0f ? breakingAcceleration * Time.deltaTime : 0f),
            Mathf.Lerp(localVelocity.y, 0f, acceleration.y == 0f ? breakingAcceleration * Time.deltaTime : 0f),
            Mathf.Lerp(localVelocity.z, 0f, acceleration.z == 0f ? breakingAcceleration * Time.deltaTime : 0f)
            ));
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
