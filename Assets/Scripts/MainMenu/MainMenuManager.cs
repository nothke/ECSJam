using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    public Slider loadingSlider;
    private Scene activeScene;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadSceneString(string scene)
    {
        StartCoroutine(LoadScene(scene, 0f));
    }

    //public void LoadSceneInt(int scene, float waitTime = 0f)
    //{
    //    StartCoroutine(LoadScene(scene, waitTime));
    //}
    public void UnloadGameScene()
    {
        StartCoroutine(UnloadScene());
    }

    private IEnumerator LoadScene(string i, float waitTime = 0f)
    {
        yield return new WaitForSeconds(waitTime);
        AsyncOperation async = new AsyncOperation();
        async = SceneManager.LoadSceneAsync(i, LoadSceneMode.Additive);
        while (!async.isDone)
        {
            yield return null;
        }
        activeScene = SceneManager.GetSceneByName(i);
        SceneManager.SetActiveScene(activeScene);


    }

    public IEnumerator UnloadScene()
    {
        if (activeScene.name != "")
        {
            AsyncOperation async = new AsyncOperation();
            int i = activeScene.buildIndex;
            async = SceneManager.UnloadSceneAsync(i);
            while (!async.isDone)
            {
                yield return null;
            }
            activeScene = SceneManager.GetSceneByBuildIndex(0);
            SceneManager.SetActiveScene(activeScene);

        }
    }

    public void ExitGame() {
        Application.Quit();
    }

}
