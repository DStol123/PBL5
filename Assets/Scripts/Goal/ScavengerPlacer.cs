using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class ScavengerPlacer : MonoBehaviour
{
    [SerializeField] private ScavengerObject[] AllObjects;
    [SerializeField] private List<GameObject> SHobjectsToPlace;
    [SerializeField] private int ObjectsToHuntFor;
    [SerializeField] private ScavengerObject[] active;
    [SerializeField] private int placed;

    private HuntManager huntManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        placed = 0;
        active = new ScavengerObject[SHobjectsToPlace.Count];
    }

    // Update is called once per frame
    void Start()
    {
        huntManager.ObjectsToFind = ObjectsToHuntFor;
    }

    public void PlaceObject(Vector3 position, Quaternion rotation)
    {
        if (SHobjectsToPlace.Count == 0) return;

        placed++;

        GameObject soObject = Instantiate(SHobjectsToPlace[0], position, rotation);

        ScavengerObject so = soObject.GetComponent<ScavengerObject>();

        active[placed] = so;

        so.HuntManager = huntManager;

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

    public HuntManager HuntManager
    {
        get { return huntManager; }
        set { huntManager = value; }
    }
}
