using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ButtonClicked()
    {
        SceneManager.LoadScene("Level1");
    }


    public void BackToMenuButtonClicked()
    {
        SceneManager.LoadScene("Menu");
    }
}
