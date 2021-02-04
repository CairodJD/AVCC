using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Home : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("On change de page");
        SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex + 1 );

    }

    public void Quit()
    {
        Debug.Log("On quitte le jeu");
        Application.Quit();

    }

}
