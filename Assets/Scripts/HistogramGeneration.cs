using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class HistogramGeneration : MonoBehaviour
{
    private Dictionary<int, int> histogram; //Times Visited: Occurrences
    public CaveData CaveData;

    public void GenerateHistogram()
    {

    }

    public void FillData()
    {
        if (CaveData == null || CaveData.activeNodes == null) return;

        foreach(Node node in CaveData.activeNodes)
        {
            int timesVisited = node.timesVisited;

            if (histogram.ContainsKey(timesVisited))
            {
                histogram[timesVisited]++;
                continue;
            }

            histogram.Add(timesVisited, 1);
        }

        for(int i = 0; i <= MaxValue(); i++)
        {
            if (histogram.ContainsKey(i)) continue;

            histogram.Add(i, 0);
        }
    }

    private int MaxValue()
    {
        int maxValue = 0;

        foreach(int value in histogram.Keys)
        {
            if (value <= maxValue) continue;

            maxValue = value;
        }

        return maxValue;
    }
}
