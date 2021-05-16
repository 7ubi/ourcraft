using System;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;


public class SaveManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private PlayerController _playerController;
    private PlayerInventory _playerInventory;

    private void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _playerInventory = player.GetComponent<PlayerInventory>();
        
        if (File.Exists(Application.persistentDataPath + "/saves/ourcraft.data"))
        {
            LoadGame();
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
            _playerInventory.ItemCount, _playerInventory.ItemIds);
        SerializationManager.Save("ourcraft", saveData);
    }

    public void LoadGame()
    {
        var data = SerializationManager.Load(Application.persistentDataPath + "/saves/ourcraft.data") as SaveData;
        
        _playerController.LoadData(data.playerInfo.pos, data.playerInfo.rotation);
        _playerInventory.LoadData(data.playerInfo.itemCount, data.playerInfo.itemIds);
    }
}
