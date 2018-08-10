using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour , IDamageable{
    public float startingHealth;
    
    public float health;
    protected bool dead;
    public event System.Action OnDeath;

    public void OnStart()
    {
        if (this.name.Contains("Player")) {
            startingHealth = PlayerPrefs.GetInt("PlayerLifeStart");
        }

        health = startingHealth;
    }
    
    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection) {
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {        
        health -= damage;
        if (health <= 0 && !dead) {
            dead = true;
            if (OnDeath != null)
                OnDeath();
                Die();
        }
    }
    
    void Die()
    {
        if (this.name.Contains("Enemy"))
        {
            GameObject.Destroy(gameObject);
        }
        else
        {
            FindObjectOfType<Spawner>().RemovePlayer(this);
            GameObject.Destroy(gameObject);
            foreach (Enemy e in FindObjectsOfType<Enemy>())
            {
                if (e.targetEntity.Equals(this))
                    e.OnTargetDeath();
            }
        }
    }
    
    public void getItem()
    {
        health = Mathf.Min(health + 10, startingHealth);
    }

    public bool isDead()
    {
        return dead;
    }
}
