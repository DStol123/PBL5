using UnityEngine;

public class ArtificialHorizon : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private void Update()
    {
        TranslateHorizon();
        RotateHorizon();
    }

    private void RotateHorizon()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, -player.transform.rotation.eulerAngles.z);
    }

    private void TranslateHorizon()
    {
        transform.position = Camera.main.WorldToScreenPoint(HorizonWorldCoordinate());
        transform.position = new Vector3
            (
                transform.position.x,
                transform.position.y,
                0f
            );
    }

    private Vector3 HorizonWorldCoordinate()
    {
        Vector3 position = new Vector3
            (
                player.transform.forward.x,
                0f,
                player.transform.forward.z
            );

        if (position.magnitude <= Mathf.Epsilon) return new Vector3(0f, 0f, 99999f);

        position = player.transform.position + position.normalized * 99999f;

        return position;
    }
}
