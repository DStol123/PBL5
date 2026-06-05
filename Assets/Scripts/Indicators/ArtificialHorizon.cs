using UnityEngine;

public class ArtificialHorizon : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Camera cam;

    [SerializeField] private RectTransform horzCanvas;
    [SerializeField] private RectTransform horizonRect;

    [SerializeField] private float pixelsPerDegree;

    private void FixedUpdate()
    {
        TranslateHorizon();
        RotateHorizon();
    }

    private void RotateHorizon()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, -player.transform.rotation.eulerAngles.z);
    }

    private void TranslateHorizon()
    {
        float pitch = Mathf.DeltaAngle(0f, player.transform.eulerAngles.x);
        float roll = Mathf.DeltaAngle(0f, player.transform.eulerAngles.z);

        float offset = pitch * pixelsPerDegree;

        float rollRad = roll * Mathf.Deg2Rad;

        horizonRect.anchoredPosition = new Vector2(
            offset * Mathf.Sin(rollRad),
            offset * Mathf.Cos(rollRad)
        );
    }
}
