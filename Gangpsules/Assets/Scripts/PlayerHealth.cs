using System;
using UnityEngine;

public class PlayerHealth:Health
{
    public const int playerMaxHealth = 6;

    public event Action<int> onPlayerTookDamage;
    public event Action<int> onPlayerRecoveredHealth;

    private void Awake()
    {
        currentHealth = playerMaxHealth;
    }

    public override void ModifyHealth(int amount, GameObject hitInstigator, Vector3 firedFromPosition)
    {
        if(amount<0)
        {
            currentHealth--;
            currentHealth = Mathf.Clamp(currentHealth, 0, playerMaxHealth);
            onPlayerTookDamage?.Invoke(currentHealth);
        }
        else if (amount>0)
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, playerMaxHealth);
            onPlayerRecoveredHealth?.Invoke(currentHealth);
        }
        
        
    }

    public void Reset()
    {
        currentHealth = playerMaxHealth;
    }
}