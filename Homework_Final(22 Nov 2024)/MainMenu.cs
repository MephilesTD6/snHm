using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //Aiman Naim (22005653)
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("SampleScene");
    }

    //Hamzah Muhsin (22001057)
    public void quitGame()
    {
        Application.Quit();
    }
}
