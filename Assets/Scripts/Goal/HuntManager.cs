using TMPro;
using UnityEngine;

public class HuntManager : MonoBehaviour
{
    public int ObjectsToFind;
    private int objectsFound;

    [SerializeField] private TMP_Text ScavengerHuntText; 

    private void Start()
    {
        ObjectsFound = 0;
    }

    public int ObjectsFound
    {
        get { return objectsFound; }
        set { 
            objectsFound = value;
            if(objectsFound >= ObjectsToFind && objectsFound != 0)
            {
                ScavengerHuntText.text = "All Objects Found!";
                return;
            }
            ScavengerHuntText.text = "Found: " + objectsFound + "/" + ObjectsToFind;
        }
    }
}
