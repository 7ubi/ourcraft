using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSurvival : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private worldCreation worldCreation;
    
    [Header("Info")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int maxFallRange;
    [SerializeField] private float regenerationTime;
    [SerializeField] private Transform head;
    [SerializeField] private float breathStrength;
    [SerializeField] private int maxAir = 10;
    private int _currentAir;
    private float _underWaterTime;
    private int _currentHealth;
    
    private Rigidbody _rb;
    private Vector3 _lastPosition;
    
    [Header("UI")]
    [SerializeField] private GameObject heartGameObject;
    [SerializeField] private Transform heartParent;
    [SerializeField] private Transform airParent;
    [SerializeField] private Vector2 startPos;
    [SerializeField] private int xOffset;
    [SerializeField] private int yAirOffset;
    
    private readonly List<Image> _hearts = new List<Image>();
    private readonly List<Image> _airBubbles = new List<Image>();
    
    [Header("Sprites")]
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite airBubble;
    
    public int Air
    {
        get => _currentAir;
        set => _currentAir = value;
    }

    public float UnderWaterTime => _underWaterTime;

    public int Health => _currentHealth;

    private void Start()
    {
        _lastPosition = transform.position;
        
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
            _underWaterTime += Time.deltaTime;
            if (_currentAir >= 1)
            {
                if (_underWaterTime >= breathStrength)
                {
                    _underWaterTime = 0f;
                    _currentAir--;
                    saveManager.SavePlayerData();
                }
            }
            else
            {
                if (_underWaterTime >= breathStrength)
                {
                    LoseHealth(1);
                    _underWaterTime = 0f;
                }
            }
            UpdateAirBar();
        }
        else
        {
            _currentAir = maxAir;
            _underWaterTime = 0f;
            UpdateAirBar();
            saveManager.SavePlayerData();
        }
    }

    private void LoseHealth(int amount)
    {
        _currentHealth -= amount;
        UpdateHealthBar();
        saveManager.SavePlayerData();
    }

    private void UpdateHealthBar()
    {
        for (var i = 0; i < _currentHealth / 2; i++)
        {
            _hearts[i].sprite = fullHeart;
        }

        if (_currentHealth % 2 == 1)
        {
            _hearts[_currentHealth / 2].sprite = halfHeart;
            for (var i = _currentHealth / 2 + 1; i < maxHealth / 2; i++)
            {
                _hearts[i].sprite = emptyHeart;
            }
        }
        else
        {
            for (var i = _currentHealth / 2; i < maxHealth / 2; i++)
            {
                try
                {
                    _hearts[i].sprite = emptyHeart;
                }
                catch
                {
                    // ignored
                }
            }
        }
    }

    private void UpdateAirBar()
    {
        airParent.gameObject.SetActive(_currentAir != maxAir);


        for (var i = 0; i < _currentAir; i++)
        {
            _airBubbles[i].sprite = airBubble;
            _airBubbles[i].color = Color.white;
        }

        for (var i = _currentAir; i < maxAir; i++)
        {
            _airBubbles[i].sprite = null;
            _airBubbles[i].color = new Color(255, 255, 255, 0);
        }
    }

    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(regenerationTime);
        if(_currentHealth < maxHealth)
            _currentHealth += 1;
        UpdateHealthBar();
        saveManager.SavePlayerData();
        StartCoroutine(RegenerateHealth());
    }

    public void LoadData(int health, int air, float underWater)
    {
        _currentHealth = health;
        _currentAir = air;
        _underWaterTime = underWater;
        CreateHealthBar();
        UpdateHealthBar();
        CreateAirBar();
        UpdateAirBar();
    }

    public void SetHealth()
    {
        _currentHealth = maxHealth;
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
}
