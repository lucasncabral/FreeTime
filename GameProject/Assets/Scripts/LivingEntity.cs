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
            return;
        
        health -= damage;
        if (health <= 0 && !dead) {
            dead = true;
            if (OnDeath != null)
                OnDeath();

            
                CmdDie();
        }
        
    }

    [Command]
    void CmdDie()
    {
        if (this.name.Contains("Enemy"))
        {
            //   GameObject.Destroy(gameObject);
            NetworkServer.Destroy(gameObject);
        }
        else
        {
            FindObjectOfType<Spawner>().RemovePlayer(this);
            foreach (Enemy e in FindObjectsOfType<Enemy>())
            {
                if (e.targetEntity.Equals(this))
                    e.CmdOnTargetDeath();
            }
            RpcDie();
        }
    }
    
    [ClientRpc]
    void RpcDie() {
            this.GetComponent<MeshRenderer>().enabled = false;
            this.GetComponent<Rigidbody>().isKinematic = true;
            this.tag = "Untagged";
            this.GetComponent<CapsuleCollider>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
            this.GetComponent<PlayerController>().enabled = false;
                    //GameObject.Destroy(gameObject);
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
