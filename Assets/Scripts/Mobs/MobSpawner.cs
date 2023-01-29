using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [Header("Requirements")] 
    private worldCreation _worldCreation;
    
    [Header("Mobs")]
    [SerializeField] private GameObject zombie;

    private void Start()
    {
        _worldCreation = GetComponent<worldCreation>();
    }

    private void Update()
    {
        
    }
}
