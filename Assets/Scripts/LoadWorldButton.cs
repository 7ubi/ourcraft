using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadWorldButton : MonoBehaviour
{
    private string _worldName;
    [SerializeField] private Button deleteButton;
    private LoadWorld _loadWorld;
    
    private void Start()
    {
        _worldName = GetComponentInChildren<TMP_Text>().text;
        GetComponent<Button>().onClick.AddListener(Clicked);
        deleteButton.onClick.AddListener(Delete);

        _loadWorld = FindObjectOfType<LoadWorld>();
    }

    private void Clicked()
    {
        PlayerPrefs.SetString("world", _worldName);
        
        _loadWorld.Select(gameObject);
        
        //var loadScene = GameObject.Find("Loading Canvas");
        //loadScene.GetComponent<LoadingScreen>().LoadScene("Game");
    }

    private void Delete()
    {
        Destroy(gameObject);
        File.Delete(Application.persistentDataPath + "/saves/" + _worldName + ".data");
    }
}
