using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SerializationManager 
{
    public static void Save(string saveName, SaveData saveData)
    {
        var formatter = GetBinaryForrmatter();

        if (!Directory.Exists(Application.persistentDataPath + "/saves"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/saves");
        }

        var path = Application.persistentDataPath + "/saves/" + saveName + ".data";

        var file = File.Create(path);

        formatter.Serialize(file, saveData);
        file.Close();
    }

    public static object Load(string path)
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
