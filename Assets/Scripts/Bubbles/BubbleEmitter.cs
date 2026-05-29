using UnityEngine;

public class BubbleEmitter : MonoBehaviour
{
    [SerializeField] private GameObject bubble;
    [SerializeField] private float emissionRate;

    private void Start()
    {
        EmitBubble();
    }
    private void EmitBubble()
    {
        GameObject go = Instantiate(bubble);
        go.transform.position = transform.position;

        Bubble bub = go.GetComponent<Bubble>();

        Vector3 randomVelocity = new Vector3(
            0f,
            Random.Range(1f, 2f),
            0f
            );

        bub.XVel = Random.Range(0f, 1f);
        bub.ZVel = Random.Range(0f, 1f);

        bub.RB.linearVelocity = randomVelocity;

        Invoke("EmitBubble", 1f / emissionRate);
    }
}
