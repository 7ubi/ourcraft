using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadWorld : MonoBehaviour
{
    [SerializeField] private Button world;
    
    
    private void Start()
    {
        foreach (var file in System.IO.Directory.GetFiles(Application.persistentDataPath + "/saves"))
        {
            var w = Instantiate(world.gameObject, this.transform);
            w.GetComponentInChildren<TMP_Text>().text = Path.GetFileNameWithoutExtension(file);
        }
    }
}
