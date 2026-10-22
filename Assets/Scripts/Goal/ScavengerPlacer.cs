using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class ScavengerPlacer : MonoBehaviour
{
    [SerializeField] private ScavengerObject[] AllObjects;
    [SerializeField] private List<GameObject> SHobjectsToPlace;
    [SerializeField] private int ObjectsToHuntFor;
    private ScavengerObject[] active;
    private int placed;

    private HuntManager huntManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        placed = 0;
        active = new ScavengerObject[SHobjectsToPlace.Count];

        huntManager.ObjectsToFind = ObjectsToHuntFor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaceObject(Vector3 position, Quaternion rotation)
    {
        if (SHobjectsToPlace.Count == 0) return;

        GameObject soObject = Instantiate(SHobjectsToPlace[0], position, rotation);

        ScavengerObject so = soObject.GetComponent<ScavengerObject>();

        active[placed] = so;

        so.HuntManager = huntManager;

        placed++;

        SHobjectsToPlace.RemoveAt(0);
    }

    public int TotalScavengerObjects
    {
        get { return active.Length; }
    }

    public int Placed
    {
        get { return placed; }
    }
}
