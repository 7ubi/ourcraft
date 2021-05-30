using System;
using System.IO;
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
        if (Directory.Exists(Application.persistentDataPath + "/saves/" + _worldName))
        {
            LoadPlayerData();
            LoadChuncks();
        }
        else
        {
            _worldCreation.GenerateFirst();
            _playerController.SetPos();
            SavePlayerData();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SavePlayerData();
        }
    }

    public void SavePlayerData()
    {
        var saveData = new PlayerInfo(_playerController.transform.position, _playerController.Camera.transform.rotation,
            _playerInventory.ItemCount, _playerInventory.ItemIds, _worldCreation.Seed);
        SerializationManager.SavePlayerData(_worldName, saveData);
    }

    private void LoadPlayerData()
    {
        var data = SerializationManager.LoadPlayerData(Application.persistentDataPath + "/saves/" + _worldName + "/player.world") as PlayerInfo;
        
        _playerController.LoadData(data.pos, data.rotation);
        _playerInventory.LoadData(data.itemCount, data.itemIds);
        _worldCreation.LoadData(data.seed);
    }

    public void SaveChunck(Chunck chunck)
    {
        SavePlayerData();
        var pos = chunck.transform.position;
        var data = new ChunckInfo(pos, chunck.BlockIDs, chunck.WaterIDs);
        
        SerializationManager.SaveChunckData(_worldName, new Vector2(pos.x, pos.z), data);
    }

    private void LoadChuncks()
    {
        foreach (var file in System.IO.Directory.GetFiles(Application.persistentDataPath + "/saves/" + _worldName + "/chuncks/"))
        {
            LoadChunck(file);
        }
    }
    
    private void LoadChunck(string file)
    {
        var data = SerializationManager.LoadChunckData(file) as ChunckInfo;

        ConstructChunck(data);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void ConstructChunck(ChunckInfo chunckInfo)
    {
        
        var c = Instantiate(_worldCreation.chunckGameObject, chunckInfo.pos, Quaternion.identity);
    
        var m = c.GetComponent<MeshCreation>();
        m.worldCreation = _worldCreation;

        var bIds = new int[_worldCreation.Size, _worldCreation.MAXHeight, _worldCreation.Size];

        for (var i = 0; i < chunckInfo.blockIDs.Length; i++)
        {
            var z = i % _worldCreation.Size;
            var y = (i / _worldCreation.Size) % _worldCreation.MAXHeight;
            var x = i / (_worldCreation.Size * _worldCreation.MAXHeight);

            bIds[x, y, z] = (int)chunckInfo.blockIDs[i];
        }
        var cChunck = c.GetComponent<Chunck>();
        cChunck.BlockIDs = bIds;
        if (chunckInfo.waterIDs != null)
        {
            var wIds = new int[_worldCreation.Size, _worldCreation.MAXHeight, _worldCreation.Size];
            for (var i = 0; i < chunckInfo.waterIDs.Length; i++)
            {
                var z = i % _worldCreation.Size;
                var y = (i / _worldCreation.Size) % _worldCreation.MAXHeight;
                var x = i / (_worldCreation.Size * _worldCreation.MAXHeight);
                wIds[x, y, z] = (int)chunckInfo.waterIDs[i];
            }
            cChunck.WaterIDs = wIds;
        }

        _worldCreation.Chuncks.Add(c);
        _worldCreation._chuncks.Add(new Vector2(c.transform.position.x, c.transform.position.z), c);
        _worldCreation._chuncksChunck.Add(new Vector2(c.transform.position.x, c.transform.position.z), c.GetComponent<Chunck>());
    
        c.GetComponent<MeshCreation>().GenerateMesh();
        
    }
    
    public void MainMenu()
    {
        SavePlayerData();
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
