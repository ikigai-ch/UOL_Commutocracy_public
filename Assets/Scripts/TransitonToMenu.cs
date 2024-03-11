using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TransitonToMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadNewSceneAfterDelay(12f, "YourSceneNameHere"));
    }

    IEnumerator LoadNewSceneAfterDelay(float delay, string sceneName)
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene("Menu");
    }
}
