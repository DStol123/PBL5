using UnityEngine;

public class FOVChanger : MonoBehaviour
{
    [SerializeField] private GameObject tunnelImage;
    [SerializeField] private CameraMovement movement;
    [SerializeField] private VRInput vrInput;

    public void ChangeFOV(float angle)
    {
        if (tunnelImage == null || !vrInput.InUse)
        {
            tunnelImage.SetActive(false);
            return;
        }
        
        tunnelImage.SetActive (true);

        tunnelImage.transform.localScale = new Vector3(angle / 180f, angle / 180f, angle / 180f);
    }
}
