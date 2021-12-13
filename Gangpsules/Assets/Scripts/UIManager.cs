using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    GameObject ammoPanel;
    [SerializeField]
    TextMeshProUGUI currentAmmoInMagazine;

    [SerializeField]
    TextMeshProUGUI currentAmmoOutOfMagazine;

    [SerializeField]
    Image cooldownImage;
    [SerializeField]
    Image[] hearts = new Image[3];
    [SerializeField]
    Sprite fullHeart;
    [SerializeField]
    Sprite halfHeart;
    [SerializeField]
    Sprite emptyHeart;

    [SerializeField]
    TextMeshProUGUI killCountCounter;

    [SerializeField]
    GameObject gameOverPanel;

    bool reloading = false;
    float reloadTime = 0f;
    float timeReloadStarted;
    int killCount = 0;
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        cooldownImage.enabled = false;
        ammoPanel.SetActive(true);
        killCountCounter.SetText(0.ToString());
        gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (reloading)
        {
            float timeSinceStarted = Time.time - timeReloadStarted;
            float percentage = timeSinceStarted / reloadTime;
            cooldownImage.fillAmount = 1 - percentage;
            if(percentage>=1)
            {
                cooldownImage.enabled = false;
                reloading = false;
            }
        }
    }

    public void EquipedAGun(WeaponAmmo ammo)
    {
        ammoPanel.SetActive(true);
        UpdateAmmo(ammo);
    }

    public void StartReload(float _reloadTime)
    {
        cooldownImage.enabled = true;
        cooldownImage.fillAmount = 1;
        reloadTime = _reloadTime;
        reloading = true;
        timeReloadStarted = Time.time;

    }

    public void UpdatePlayerHealth(int currentHealth)
    {
        switch(currentHealth)
        {
            case 0:
                hearts[0].sprite = emptyHeart;
                hearts[1].sprite = emptyHeart;
                hearts[2].sprite = emptyHeart;
                break;
            case 1:
                hearts[0].sprite = halfHeart;
                hearts[1].sprite = emptyHeart;
                hearts[2].sprite = emptyHeart;
                break;
            case 2:
                hearts[0].sprite = fullHeart;
                hearts[1].sprite = emptyHeart;
                hearts[2].sprite = emptyHeart;
                break;
            case 3:
                hearts[0].sprite = fullHeart;
                hearts[1].sprite = halfHeart;
                hearts[2].sprite = emptyHeart;
                break;
            case 4:
                hearts[0].sprite = fullHeart;
                hearts[1].sprite = fullHeart;
                hearts[2].sprite = emptyHeart;
                break;
            case 5:
                hearts[0].sprite = fullHeart;
                hearts[1].sprite = fullHeart;
                hearts[2].sprite = halfHeart;
                break;
            case 6:
                hearts[0].sprite = fullHeart;
                hearts[1].sprite = fullHeart;
                hearts[2].sprite = fullHeart;
                break;
        }
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void UpdateAmmo(WeaponAmmo weaponAmmo)
    {
        currentAmmoInMagazine.SetText(weaponAmmo.CurrentMagazineBullets.ToString());
        currentAmmoOutOfMagazine.SetText(weaponAmmo.TotalBulletsRemaining.ToString());
    }

    public void EnemyKilled()
    {
        killCount++;
        killCountCounter.SetText(killCount.ToString());
    }

    public void Reset()
    {
        killCount = 0;
        killCountCounter.SetText(killCount.ToString());
        gameOverPanel.SetActive(false);
    }

    public void OnClickPlayAgain()
    {
        GameManager.Instance.StartGame();
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
