using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SerializationManager
{
    private static string _save = Application.persistentDataPath + "/saves/";
    
    public static void SavePlayerData(string saveName, PlayerInfo saveData)
    {
        var formatter = GetBinaryForrmatter();

        if (!Directory.Exists(_save + saveName))
        {
            Directory.CreateDirectory(_save + saveName);
        }

        var path = _save + saveName + "/player.world";

        var file = File.Create(path);

        formatter.Serialize(file, saveData);
        file.Close();
    }

    public static object LoadPlayerData(string path)
    {
        if (!File.Exists(path))
            return null;

        var formatter = GetBinaryForrmatter();
        var file = File.Open(path, FileMode.Open);
        try
        {
            var save = formatter.Deserialize(file);
            file.Close();

            return save;
        }
        catch
        {
            file.Close();
            return null;
        }
    }
    
    public static void SaveChunckData(string saveName, Vector2 pos, ChunckInfo saveData)
    {
        var formatter = GetBinaryForrmatter();

        if (!Directory.Exists(_save + saveName + "/chuncks"))
        {
            Directory.CreateDirectory(_save + saveName + "/chuncks");
        }

        var path = _save + saveName + "/chuncks/" + (int)pos.x + "-" + (int)pos.y + ".chunck";

        var file = File.Create(path);

        formatter.Serialize(file, saveData);
        file.Close();
    }

    public static object LoadChunckData(string fileName)
    {
        if (!File.Exists(fileName))
            return null;

        var formatter = GetBinaryForrmatter();
        var file = File.Open( fileName, FileMode.Open);
        try
        {
            var save = formatter.Deserialize(file);
            file.Close();

            return save;
        }
        catch
        {
            file.Close();
            return null;
        }
    }
    
    public static void SaveDestroyedData(string saveName, DestroyedInfo saveData)
    {
        var formatter = GetBinaryForrmatter();

        if (!Directory.Exists(_save + saveName))
        {
            Directory.CreateDirectory(_save + saveName);
        }

        var path = _save + saveName + "/destroyed.blocks";

        var file = File.Create(path);

        formatter.Serialize(file, saveData);
        file.Close();
    }

    public static object LoadDestroyedData(string fileName)
    {
        if (!File.Exists(fileName))
            return null;

        var formatter = GetBinaryForrmatter();
        var file = File.Open(fileName, FileMode.Open);
        try
        {
            var save = formatter.Deserialize(file);
            file.Close();

            return save;
        }
        catch
        {
            file.Close();
            return null;
        }
    }

    public static BinaryFormatter GetBinaryForrmatter()
    {
        var formatter = new BinaryFormatter();

        var selector = new SurrogateSelector();

        var vector3Serialization = new Vector3Serialization();
        var quaternionSerialization = new QuaternionSerialization();
        
        selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Serialization);
        selector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternionSerialization);

        formatter.SurrogateSelector = selector;

        return formatter;
    }
}
