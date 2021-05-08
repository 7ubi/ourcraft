using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class block : MonoBehaviour
{
    private world_creation _worldCreation;

    [SerializeField] private GameObject top;
    [SerializeField] private GameObject bot;
    [SerializeField] private GameObject lef;
    [SerializeField] private GameObject rig;
    [SerializeField] private GameObject fro;
    [SerializeField] private GameObject bac;
    
    private void Start()
    {
        _worldCreation = GameObject.FindObjectOfType<world_creation>();
        GetNeighbourBlock();
    }

    private void GetNeighbourBlock()
    {
        
    }

    public GameObject Top
    {
        get => top;
        set => top = value;
    }

    public GameObject Bot
    {
        get => bot;
        set => bot = value;
    }

    public GameObject Lef
    {
        get => lef;
        set => lef = value;
    }

    public GameObject Rig
    {
        get => rig;
        set => rig = value;
    }

    public GameObject Fro
    {
        get => fro;
        set => fro = value;
    }

    public GameObject Bac
    {
        get => bac;
        set => bac = value;
    }
}
