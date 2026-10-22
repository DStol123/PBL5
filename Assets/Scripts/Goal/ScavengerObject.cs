using UnityEngine;

public class ScavengerObject : MonoBehaviour
{
    public bool active;
    public bool valid;

    public HuntManager HuntManager;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && valid && active)
        {
            active = false;
            HuntManager.ObjectsFound++;
        }
    }
}
