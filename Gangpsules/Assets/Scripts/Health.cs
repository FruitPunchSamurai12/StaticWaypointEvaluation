using System;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth { get; protected set; }

    public event Action<GameObject,Vector3> onHealthModified;
    public event Action onDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }



    public float GetPercentage()
    {
        return ((float)currentHealth / (float)maxHealth)*100f;
    }

    public virtual void ModifyHealth(int amount,GameObject hitInstigator, Vector3 firedFromPosition)
    {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        if (currentHealth == 0)
            onDeath?.Invoke();
        else
            onHealthModified?.Invoke(hitInstigator,firedFromPosition);
    }


}
