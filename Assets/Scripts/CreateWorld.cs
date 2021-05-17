using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateWorld : MonoBehaviour
{
    [SerializeField] private LoadingScreen loadingScreen;
    
    public void Create(TMP_Text worldName)
    {
        if(worldName.text == "")
            return;
        
        PlayerPrefs.SetString("world", worldName.text);
    }
}
