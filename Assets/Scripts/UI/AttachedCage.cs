using UnityEngine;

public class AttachedCage : MonoBehaviour
{
    private Quaternion startingRotation;

    private void Start()
    {
        startingRotation = transform.rotation;
    }
    // Update is called once per frame
    void Update()
    {
        transform.rotation = startingRotation;
    }
}
