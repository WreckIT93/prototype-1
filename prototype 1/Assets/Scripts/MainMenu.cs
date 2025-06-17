using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class NewMonoBehaviourScript : MonoBehaviour
{
    public void startGame()
    {
        SceneManager.LoadScene(1);
    }

    public void quitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void loadMainMenu()
    {
        SceneManager.LoadScene(0);
    }



}
