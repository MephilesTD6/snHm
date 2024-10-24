using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class fpsDisplay : MonoBehaviour
{
    float fps;
    float updateTimer = 0.2f;

    [SerializeField] TextMeshProUGUI fpsTitle;

    private void updateFPS()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer < 0)
        {
            updateTimer = 0.2f;

            fps = 1f / Time.unscaledDeltaTime;
            fpsTitle.text = "FPS: " + Mathf.Round(fps);
        }
    }

    void Update()
    {
        updateFPS();
    }
}


