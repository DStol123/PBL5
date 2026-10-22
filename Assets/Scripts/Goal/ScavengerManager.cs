using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class ScavengerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> SHobjects;
    public int[] active;
    public int currentIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SHobjects = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaceObject(int index, Vector3 position, Quaternion rotation)
    {
        Instantiate(SHobjects[index], position, rotation);
        if (currentIndex + 1 >= active.Length) return;

        currentIndex++;
    }
}
