using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LivingEntity : NetworkBehaviour , IDamageable{
    public float startingHealth;

    [SyncVar]
    public float health;

    protected bool dead;

    public event System.Action OnDeath;

    public override void OnStartClient()
    {
        health = startingHealth;
    }
    
    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection) {
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {
        if (!isServer)
        {
            return;
        }
        
        health -= damage;
        if (health <= 0 && !dead) {
            Die();
        }
    }
    
    protected void Die() {
        dead = true;
        if (OnDeath != null)
            OnDeath();
        GameObject.Destroy(gameObject);
    }

    public void getItem()
    {
        health = Mathf.Min(health + 10, startingHealth);
    }
}
