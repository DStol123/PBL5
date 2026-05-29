using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float TimeToLive;
    [SerializeField] private Rigidbody rb;

    private float xVel;
    private float zVel;

    private float period;

    private void Start()
    {
        Invoke("Die", TimeToLive);

        period = Random.Range(0.8f, 1.6f);
    }

    private void FixedUpdate()
    {
        float xVelocity = xVel * Mathf.Sin(period * Time.time);
        float zVelocity = zVel * Mathf.Cos(period * Time.time);

        rb.linearVelocity = new Vector3(xVelocity, rb.linearVelocity.y, zVelocity);
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

    public float XVel
    {
        get { return xVel; }
        set { xVel = value; }
    }

    public float ZVel
    {
        get { return zVel; }
        set { zVel = value; }
    }
}
