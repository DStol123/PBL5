using UnityEngine;

public class UserRotationTracker : MonoBehaviour
{
    [SerializeField] private GameObject userCapsule;
    private Vector3 startingEulerRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingEulerRotation = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Euler(startingEulerRotation + userCapsule.transform.eulerAngles);
    }
}
