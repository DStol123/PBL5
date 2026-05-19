using UnityEngine;
using System;
using System.IO;

public class Recording : MonoBehaviour
{
    [SerializeField] private string filePath;

    public void WriteTrialLog(int vrExperience, float timeToBottom, float timeToReturn)
    {
        string contents = $"Date: {DateTime.Today}\nExperience in VR: {vrExperience}\nTime to end of cave: {timeToBottom.ToString("0.00")}\nTime to return: {timeToReturn.ToString("0.00")}";

        try
        {
            File.WriteAllText($"{DateTime.Now}.txt", contents);
        }
        catch(UnauthorizedAccessException)
        {
            Debug.Log("Cannot write log file: Unauthorized Access");
        }
        catch (IOException ioEx)
        {
            Console.WriteLine($"I/O Error: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
