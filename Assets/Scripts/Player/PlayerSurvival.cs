using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSurvival : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private worldCreation worldCreation;
    private PlayerController _playerController;
    private PlayerInventory _playerInventory;
    
    [Header("Info")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int maxFallRange;
    [SerializeField] private float regenerationTime;
    [SerializeField] private Transform head;
    [SerializeField] private float breathStrength;
    [SerializeField] private int maxAir = 10;

    private Rigidbody _rb;
    private Vector3 _lastPosition;
    
    [Header("UI")]
    [SerializeField] private GameObject heartGameObject;
    [SerializeField] private Transform heartParent;
    [SerializeField] private Transform airParent;
    [SerializeField] private Vector2 startPos;
    [SerializeField] private int xOffset;
    [SerializeField] private int yAirOffset;
    [SerializeField] private GameObject deathScreen;
    
    private readonly List<Image> _hearts = new List<Image>();
    private readonly List<Image> _airBubbles = new List<Image>();
    
    [Header("Sprites")]
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite airBubble;
    
    public int Air { get; set; }

    public float UnderWaterTime { get; private set; }

    public int Health { get; private set; }

    private void Start()
    {
        _lastPosition = transform.position;
        _playerController = GetComponent<PlayerController>();
        _playerInventory = GetComponent<PlayerInventory>();
        
        _rb = GetComponent<Rigidbody>();
        StartCoroutine(RegenerateHealth());
    }

    private void Update()
    {
        //Fall damage
        if (Math.Abs(_rb.velocity.y) < 0.1f)
        {
            if (_lastPosition.y - transform.position.y > maxFallRange)
            {
                LoseHealth((int)(_lastPosition.y - transform.position.y - maxFallRange));
            }
            
            _lastPosition = transform.position;
        }

        if (worldCreation.GetUnderWater(head.transform.position))
        {
            UnderWaterTime += Time.deltaTime;
            if (Air >= 1)
            {
                if (UnderWaterTime >= breathStrength)
                {
                    UnderWaterTime = 0f;
                    Air--;
                    saveManager.SavePlayerData();
                }
            }
            else
            {
                if (UnderWaterTime >= breathStrength)
                {
                    LoseHealth(1);
                    UnderWaterTime = 0f;
                }
            }
            UpdateAirBar();
        }
        else
        {
            Air = maxAir;
            UnderWaterTime = 0f;
            UpdateAirBar();
        }
    }

    private void LoseHealth(int amount)
    {
        Health -= amount;

        if (Health <= 0)
        {
            Death();
            Health = 0;
        }
        
        UpdateHealthBar();
        saveManager.SavePlayerData();
    }

    private void UpdateHealthBar()
    {
        for (var i = 0; i < Health / 2; i++)
        {
            _hearts[i].sprite = fullHeart;
        }

        if (Health % 2 == 1)
        {
            _hearts[Health / 2].sprite = halfHeart;
            for (var i = Health / 2 + 1; i < maxHealth / 2; i++)
            {
                _hearts[i].sprite = emptyHeart;
            }
        }
        else
        {
            for (var i = Health / 2; i < maxHealth / 2; i++)
            {
                _hearts[i].sprite = emptyHeart;
            }
        }
    }

    private void UpdateAirBar()
    {
        airParent.gameObject.SetActive(Air != maxAir);


        for (var i = 0; i < Air; i++)
        {
            _airBubbles[i].sprite = airBubble;
            _airBubbles[i].color = Color.white;
        }

        for (var i = Air; i < maxAir; i++)
        {
            _airBubbles[i].sprite = null;
            _airBubbles[i].color = new Color(255, 255, 255, 0);
        }
    }

    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(regenerationTime);
        if(Health < maxHealth)
            Health += 1;
        UpdateHealthBar();
        saveManager.SavePlayerData();
        StartCoroutine(RegenerateHealth());
    }

    public void LoadData(int health, int air, float underWater)
    {
        Health = health;
        Air = air;
        UnderWaterTime = underWater;
        CreateHealthBar();
        UpdateHealthBar();
        CreateAirBar();
        UpdateAirBar();
    }

    public void SetHealth()
    {
        Health = maxHealth;
        CreateHealthBar();
        UpdateHealthBar();
        CreateAirBar();
        UpdateAirBar();
    }

    private void CreateHealthBar()
    {
        for (var i = 0; i < maxHealth / 2; i++)
        {
            var h = Instantiate(heartGameObject, heartParent, false);

            var hImg = h.GetComponent<Image>();
            
            _hearts.Add(hImg);
            
            h.GetComponent<RectTransform>().localPosition = new Vector3(startPos.x + xOffset * i, startPos.y, 0);
            hImg.sprite = fullHeart;
        }
    }
    
    private void CreateAirBar()
    {
        for (var i = 0; i < maxAir; i++)
        {
            var h = Instantiate(heartGameObject, airParent, false);

            var hImg = h.GetComponent<Image>();
            
            _airBubbles.Add(hImg);
            
            h.GetComponent<RectTransform>().localPosition = new Vector3(startPos.x + xOffset * i, startPos.y + yAirOffset, 0);
            hImg.sprite = null;
            hImg.color = new Color(255, 255, 255, 0);
        }
    }

    private void Death()
    {
        
        _playerController.enabled = false;
        _playerInventory.enabled = false;
        Time.timeScale = 0;
        _playerController.InPause = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        deathScreen.SetActive(true);
        
        _playerInventory.DeathDrop();
        ResetPlayer();
        
        saveManager.SavePlayerData();
    }

    private void ResetPlayer()
    {
        transform.position = new Vector3(0, 0, 0);
        _playerController.SetPos();
        worldCreation.GenerateChunck();
    }

    public void Respawn()
    {
        
        Health = maxHealth;
        _playerController.enabled = true;
        _playerInventory.enabled = true;
        Time.timeScale = 1;
        _playerController.InPause = false;
        deathScreen.SetActive(false);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        UpdateHealthBar();
    }
}
