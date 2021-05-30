using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        var asyncLoad = SceneManager.LoadSceneAsync("Scenes/Game");
        
        while (!asyncLoad.isDone)
        {
            UpdateUI(asyncLoad.progress);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void UpdateUI(float progress)
    {
        progressBar.value = progress;
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}
