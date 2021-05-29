using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryCell : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text text;

    public int Id { get; set; }
    public int Count { get; set; }
    public int LastIndex { get; set; }
    
    private RectTransform _rectTransform;
    private CanvasScaler _scaler;
    

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _scaler = GetComponentInParent<CanvasScaler>();
    }

    private void Update()
    {
        if (!inventory.InInventory) return;

        _rectTransform.anchoredPosition = new Vector2(Input.mousePosition.x * _scaler.referenceResolution.x / Screen.width,
            Input.mousePosition.y * _scaler.referenceResolution.y / Screen.height);
    }

    public void SetSprite(Sprite icon, int id, int count, int lastIndex)
    {
        image.sprite = icon;
        image.color = new Color(255, 255, 255, 100);
        Id = id;
        Count = count;
        text.text = "" + count;
        LastIndex = lastIndex;
    }

    public void Reset()
    {
        image.sprite = null;
        image.color = new Color(255, 255, 255, 0);
        text.text = "";
        Id = 0;
        Count = 0;
        LastIndex = 0;
    }

    public void UpdateText()
    {
        text.text = Count <= 0 ? "": "" + Count;
    }

    public void ResetToOriginalPos()
    {
        if (Id == 0) return;
        inventory.SetItem(LastIndex, Id, Count);
        Reset();
    }
}
