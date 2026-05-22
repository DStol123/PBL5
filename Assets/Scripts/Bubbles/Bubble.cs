using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float TimeToLive;
    [SerializeField] private Rigidbody rb;

    private void Start()
    {
        Invoke("Die", TimeToLive);
    }

    public void Die()
    {
        Destroy(this.gameObject);
    }

    public Rigidbody RB
    {
        get { return rb; }
        set { rb = value; }
    }
}
