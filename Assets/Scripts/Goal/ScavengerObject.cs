using UnityEngine;

public class ScavengerObject : MonoBehaviour
{
    public bool active;
    public bool valid;

    public HuntManager HuntManager;

    private void Start()
    {
        if(HuntManager == null)
        {
            HuntManager = FindFirstObjectByType<HuntManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && valid && active)
        {
            valid = false;
            active = false;
            HuntManager.ObjectsFound++;

            Destroy(gameObject);
        }
    }
}
