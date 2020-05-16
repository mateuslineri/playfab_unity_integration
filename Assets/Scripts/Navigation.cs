using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigation : MonoBehaviour
{
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void GoToRegister()
    {
        SceneManager.LoadScene(1);
    }

    public void GoToLogin() 
    {
        SceneManager.LoadScene(2);
    }

    public void ExitApplication()
    {
        PlayerPrefs.DeleteAll();
        Application.Quit();
    }
}
