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

    [SerializeField] private CaveData caveData;

    public bool initialized;

    private FOVChanger fovChanger;

    private void Start()
    {
        fovChanger = GetComponentInChildren<FOVChanger>();
    }

    private void Update()
    {
        if (!initialized) return;

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
}
