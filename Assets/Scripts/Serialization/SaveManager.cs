using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class SaveManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private worldCreation _worldCreation;
    private PlayerController _playerController;
    private PlayerInventory _playerInventory;
    private PlayerSurvival _playerSurvival;
    private DayNightCycle _dayNight;
    private string _worldName;

    private List<MeshCreation> _chuncks = new List<MeshCreation>();

    private void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _playerInventory = player.GetComponent<PlayerInventory>();
        _playerSurvival = player.GetComponent<PlayerSurvival>();
        _worldCreation = GetComponent<worldCreation>();
        _dayNight = GetComponent<DayNightCycle>();
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
            _playerSurvival.SetHealth();
            SavePlayerData();
        }
    }

    public void SavePlayerData()
    {
        var saveData = new PlayerInfo(_playerController.transform.position, _playerController.Camera.transform.rotation,
            _playerInventory.ItemCount, _playerInventory.ItemIds, _worldCreation.Seed, _playerSurvival.Health, _dayNight.CurrentTime);
        SerializationManager.SavePlayerData(_worldName, saveData);
    }

    private void LoadPlayerData()
    {
        var data = SerializationManager.LoadPlayerData(Application.persistentDataPath + "/saves/" + _worldName + "/player.world") as PlayerInfo;
        
        _playerController.LoadData(data.pos, data.rotation);
        _playerInventory.LoadData(data.itemCount, data.itemIds);
        _playerSurvival.LoadData(data.health);
        _worldCreation.LoadData(data.seed);
        _dayNight.CurrentTime = data.currentTime;
    }

    public void SaveChunck(Chunck chunck)
    {
        SavePlayerData();
        var pos = chunck.transform.position;
        var data = new ChunckInfo(pos, chunck.BlockIDs, chunck.WaterIDs, chunck.furnaces);
        
        SerializationManager.SaveChunckData(_worldName, new Vector2(pos.x, pos.z), data);
    }

    private void LoadChuncks()
    {
        foreach (var file in System.IO.Directory.GetFiles(Application.persistentDataPath + "/saves/" + _worldName + "/chuncks/"))
        {
            LoadChunck(file);
        }

        foreach (var chunck in _chuncks)
        {
            chunck.Init();
        }
        
        _worldCreation.LoadNearestChuncks();
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
        var wIds = new int[_worldCreation.Size, _worldCreation.MAXHeight, _worldCreation.Size];
        
        for (var i = 0; i < chunckInfo.blockIDs.Length; i++)
        {
            var z = i % _worldCreation.Size;
            var y = (i / _worldCreation.Size) % _worldCreation.MAXHeight;
            var x = i / (_worldCreation.Size * _worldCreation.MAXHeight);

            if (chunckInfo.blockIDs[i] > 200)
            {
                wIds[x, y, z] = (int)(Math.Abs(chunckInfo.blockIDs[i] - 255));
            }
            else
            {
                bIds[x, y, z] = (int) chunckInfo.blockIDs[i];
            }
        }
        var cChunck = c.GetComponent<Chunck>();
        cChunck.worldCreation = _worldCreation;
        cChunck.BlockIDs = bIds;
        cChunck.WaterIDs = wIds;

        _worldCreation.Chuncks.Add(c);
        if(!_worldCreation._chuncks.ContainsKey(new Vector2(c.transform.position.x, c.transform.position.z)))
            _worldCreation._chuncks.Add(new Vector2(c.transform.position.x, c.transform.position.z), c);
        if(!_worldCreation._chuncksChunck.ContainsKey(new Vector2(c.transform.position.x, c.transform.position.z)))
            _worldCreation._chuncksChunck.Add(new Vector2(c.transform.position.x, c.transform.position.z), c.GetComponent<Chunck>());
        
        if (chunckInfo.furnaceInfos != null)
        {
            foreach (var furnace in chunckInfo.furnaceInfos)
            {
                cChunck.furnaces.Add(Vector3Int.FloorToInt(furnace.position), new Furnace(furnace));
            }
        }

        _chuncks.Add(c.GetComponent<MeshCreation>());
    }
    
    public void MainMenu()
    {
        SavePlayerData();
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
