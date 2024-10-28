using TMPro;
using UnityEngine;
using System;

public class TimeDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeTitle;

    public void updateTimeTaken(TimeSpan timeTaken)
    {
        timeTitle.text = "Time Taken: " + timeTaken.TotalMilliseconds + " ms";
    }
}


