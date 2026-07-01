using UnityEngine;
using System;
using System.IO;

public class Recording : MonoBehaviour
{
    private string filePath = "newTrialLogs.csv";

    public void WriteTrialLog(string part, int vrExperience, float timeToBottom, float timeToReturn, bool nauseous, float nauseaTime, bool bubbles, bool ah, bool li)
    {
        if(!File.Exists(filePath))
        {
            File.Create(filePath).Close();

            StreamWriter writer = new StreamWriter(filePath, true);

            string Columns = "Participant,Date,Time,VR Experience,Time to Bottom,Time to Return,Time to Become Nauseous,Bubbles,AH,LI,Headache Before,Headache After,Nausea Before,NauseaAfter,DizzinessBefore,DizzinessAfter,NASA Mental,NASA Physical,NASA Temporal,NASA Performance,NASA Effort,NASA Frustration";

            writer.WriteLine(Columns);

            writer.Flush();
            writer.Close();
        }

        StreamWriter sw = new StreamWriter(filePath, true);

        sw.WriteLine($"{part},{DateTime.Today.ToString("yyyy-MM-dd")},{DateTime.Now.TimeOfDay},{vrExperience},{timeToBottom.ToString("0.00")},{timeToReturn.ToString("0.00")},{nauseaTime.ToString("0.00")},{bubbles},{ah},{li}");

        sw.Flush();
        sw.Close();
    }

    public void UpdateHistogram(int index, float distance, string participant, float distanceAlong, float depth)
    {
        string timeSeriesPath = $"{participant}TimeSeries.csv";

        if (!File.Exists(timeSeriesPath))
        {
            File.Create(timeSeriesPath).Close();

            StreamWriter writer = new StreamWriter(timeSeriesPath, true);

            string Columns = "Seconds,Distance From Path,Distance Along Path,Depth";
            writer.WriteLine(Columns);

            writer.Flush();
            writer.Close();
        }

        StreamWriter sw = new StreamWriter(timeSeriesPath, true);

        sw.WriteLine($"{index},{distance.ToString("0.0")},{distanceAlong},{depth}");

        sw.Flush();
        sw.Close();
    }
}
