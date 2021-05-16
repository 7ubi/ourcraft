using System;
using System.IO;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Serialization;


public class SaveManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private worldCreation _worldCreation;
    private PlayerController _playerController;
    private PlayerInventory _playerInventory;

    private void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _playerInventory = player.GetComponent<PlayerInventory>();
        _worldCreation = GetComponent<worldCreation>();
        
        if (File.Exists(Application.persistentDataPath + "/saves/ourcraft.data"))
        {
            LoadGame();
        }
        else
        {
            _worldCreation.GenerateFirst();
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
        SerializationManager.Save("ourcraft", saveData);
    }

    public void LoadGame()
    {
        var data = SerializationManager.Load(Application.persistentDataPath + "/saves/ourcraft.data") as SaveData;
        
        _playerController.LoadData(data.playerInfo.pos, data.playerInfo.rotation);
        _playerInventory.LoadData(data.playerInfo.itemCount, data.playerInfo.itemIds);
        _worldCreation.LoadData(data.seed, data.chuncksInfos);
    }
}
