using UnityEngine;

public class UserRotationTracker : MonoBehaviour
{
    [SerializeField] private GameObject userCapsule;
    [SerializeField] private UserRotationTracker counterpart;
    [SerializeField] private bool inverted;
    [SerializeField] private bool disabled;

    private Vector3 startingEulerRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingEulerRotation = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (disabled) return;
        Vector3 additionFromUserRotation = userCapsule.transform.localEulerAngles;
        if (inverted) additionFromUserRotation *= -1f;
        Debug.Log("Inverted: " + inverted);
        Debug.Log("Angle Addition From User Rotation: " + additionFromUserRotation);

        transform.localRotation = Quaternion.Euler(startingEulerRotation + additionFromUserRotation);
    }

    public bool Disabled
    {
        get { return disabled; }
        set { disabled = value; }
    }

    public bool Inverted
    {
        get { return inverted; }
        set { 
            if (value) counterpart.Disabled = true;
            inverted = value;
        }
    }
}
