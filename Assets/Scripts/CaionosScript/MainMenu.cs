using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.SceneManagement;



public class NewBehaviourScript : MonoBehaviour
{



    public void PlayGame()

    {

        SceneManager.LoadSceneAsync(1);

    }



    public void QuitGame()

    {

        Debug.Log("QUIT!");

        Application.Quit();

    }


}