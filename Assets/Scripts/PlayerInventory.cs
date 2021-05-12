using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private RectTransform selector;
    private int _current = 0;

    private void Update()
    {
        _current = (_current + (int)Input.mouseScrollDelta.y) % 9;
        if (_current < 0)
            _current = 8;
        
        selector.localPosition = new Vector2(-400 + _current * 100, selector.localPosition.y);
    }
}
