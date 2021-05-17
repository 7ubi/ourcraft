using System;
using System.IO;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class SaveManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private worldCreation _worldCreation;
    private PlayerController _playerController;
    private PlayerInventory _playerInventory;
    private string _worldName;

    private void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _playerInventory = player.GetComponent<PlayerInventory>();
        _worldCreation = GetComponent<worldCreation>();
        _worldName = PlayerPrefs.GetString("world");
        if (File.Exists(Application.persistentDataPath + "/saves/" + _worldName + ".data"))
        {
            LoadGame();
        }
        else
        {
            _worldCreation.GenerateFirst();
            SaveGame();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SaveGame();
        }
    }

    public void SaveGame()
    {
        var saveData = new SaveData(_playerController.transform.position, _playerController.Camera.transform.rotation,
            _playerInventory.ItemCount, _playerInventory.ItemIds,
            _worldCreation.Seed, _worldCreation.Chuncks, _worldCreation.Size1, _worldCreation.MAXHeight);
        SerializationManager.Save(_worldName, saveData);
    }

    public void LoadGame()
    {
        var data = SerializationManager.Load(Application.persistentDataPath + "/saves/" + _worldName + ".data") as SaveData;
        
        _playerController.LoadData(data.playerInfo.pos, data.playerInfo.rotation);
        _playerInventory.LoadData(data.playerInfo.itemCount, data.playerInfo.itemIds);
        _worldCreation.LoadData(data.seed, data.chuncksInfos);
    }

    public void MainMenu()
    {
        SaveGame();
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
