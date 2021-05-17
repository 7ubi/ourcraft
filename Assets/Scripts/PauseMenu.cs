using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerActionController playerActionController;
    private PlayerController _playerController;
    private PlayerInventory _playerInventory;

    private void Start()
    {
        Cursor.visible = false;
        
        Cursor.lockState = CursorLockMode.Locked;

        _playerController = player.GetComponent<PlayerController>();
        _playerInventory = player.GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        Pause();
    }

    public void Pause()
    {
        playerActionController.enabled = panel.activeInHierarchy;
        _playerController.enabled = panel.activeInHierarchy;
        _playerInventory.enabled = panel.activeInHierarchy;
        panel.SetActive(!panel.activeInHierarchy);
        Time.timeScale = panel.activeInHierarchy ? 0 : 1;
        Cursor.visible = panel.activeInHierarchy;
        Cursor.lockState = panel.activeInHierarchy ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
