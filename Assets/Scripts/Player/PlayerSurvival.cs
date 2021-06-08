using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSurvival : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;
    
    [Header("Info")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int maxFallRange;
    [SerializeField] private float regenerationTime;
    private int _currentHealth;

    public int Health
    {
        get => _currentHealth;
        set => _currentHealth = value;
    }
    
    private Rigidbody _rb;
    private Vector3 _lastPosition;
    
    [Header("UI")]
    [SerializeField] private GameObject heartGameObject;
    [SerializeField] private Transform heartParent;
    [SerializeField] private Vector2 startPos;
    [SerializeField] private int xOffset;
    
    private readonly List<Image> _hearts = new List<Image>();
    
    [Header("Sprites")]
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite fullHeart;
    
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
                }catch{}
            }
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

    public void LoadData(int health)
    {
        _currentHealth = health;
        CreateHealthBar();
        UpdateHealthBar();
    }

    public void SetHealth()
    {
        _currentHealth = maxHealth;
        CreateHealthBar();
        UpdateHealthBar();
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
}
