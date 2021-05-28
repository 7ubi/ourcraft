using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadWorld : MonoBehaviour
{
    [SerializeField] private Button world;
    [SerializeField] private GameObject canvas;
    [SerializeField] private LoadingScreen loadingScreen;
    private GameObject _selected;
    
    private void Start()
    {
        foreach (var file in System.IO.Directory.GetFiles(Application.persistentDataPath + "/saves"))
        {
            var w = Instantiate(world.gameObject, this.transform);
            w.GetComponentInChildren<TMP_Text>().text = Path.GetFileNameWithoutExtension(file);
        }
    }

    public void Select(GameObject button)
    {
        if (_selected != null)
        {
            _selected.GetComponent<Image>().color = new Color(255, 255, 255, 100);
        }
        _selected = button;
        
        _selected.GetComponent<Image>().color = new Color(0, 100, 0, 100);
    }

    public void Load()
    {
        if (_selected == null)
            return;
        loadingScreen.gameObject.SetActive(true);
        canvas.SetActive(false);
        loadingScreen.LoadScene();
    }
}
