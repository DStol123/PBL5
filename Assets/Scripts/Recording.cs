using UnityEngine;
using System;
using System.IO;

public class Recording : MonoBehaviour
{
    private string filePath = "trialLogs.csv";

    public void WriteTrialLog(int vrExperience, float timeToBottom, float timeToReturn)
    {
        if(!File.Exists(filePath))
        {
            File.Create(filePath).Close();

            StreamWriter writer = new StreamWriter(filePath, true);

            string Columns = "Date,Time,VR Experience,Time to Bottom,Time to Return";

            writer.WriteLine(Columns);

            writer.Flush();
            writer.Close();
        }

        StreamWriter sw = new StreamWriter(filePath, true);

        sw.WriteLine($"{DateTime.Today.ToString("yyyy-MM-dd")},{DateTime.Now.TimeOfDay},{vrExperience},{timeToBottom},{timeToReturn}");

        sw.Flush();
        sw.Close();
    }
}
