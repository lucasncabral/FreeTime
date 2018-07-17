using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

[RequireComponent (typeof (NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State {Idle, Chasing, Attacking};
    State currentState;

    NavMeshAgent pathfinder;
    
    float attackDistanceThreshold = .5f;
    float timeBetweenAttacks = 1;

    float nextTimeAttack;
    
    public LivingEntity targetEntity;

    [SyncVar]
    public NetworkInstanceId targetNetId;

    [SyncVar]
    public Transform target;

    float myCollisionRadius;
    float targetCollisionRadius;

    public GameObject deathEffect;
    private ScoreKeeper score;

    [SyncVar]
    public bool hasTarget;

    // itens
    public Transform[] itens;
    
    float damage = 1;

    public int hitsToKill;
    public float speed;
    public float startingHealthBase;

    public GameObject playerProx;
    GameObject[] players;

    void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();
        
        players = GameObject.FindGameObjectsWithTag("Player");
        playerProx = players[0];
        float distanceMin = Vector3.Distance(players[0].transform.position, this.transform.position);
        for(int i = 1; i < players.Length;i++)
        {
            float newDistance = Vector3.Distance(players[i].transform.position, this.transform.position);
            if(newDistance < distanceMin)
            {
                distanceMin = newDistance;
                playerProx = players[i];
            }
        }


        if (playerProx != null)
        {
            hasTarget = true;

            target = playerProx.transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetNetId = targetEntity.netId;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }

        score = FindObjectOfType<ScoreKeeper>();
        SetCharacteristics(speed, hitsToKill, startingHealthBase);
    }
    
    public void Start(){
        if (hasTarget) {
            currentState = State.Chasing;
           Action OnTargetDeathAction = () => OnTargetDeath();
           targetEntity.OnDeath += OnTargetDeathAction;
            StartCoroutine(UpdatePath());
        }
    }

    void OnTargetDeath()
    {
        hasTarget = false;

        players = GameObject.FindGameObjectsWithTag("Player");
        
        int numIndex = Array.IndexOf(players, playerProx);

        players = players.Where((val, idx) => idx != numIndex).ToArray();

        if (players.Length > 0)
        {
            playerProx = players[0];
            float distanceMin = Vector3.Distance(players[0].transform.position, this.transform.position);
            for (int i = 1; i < players.Length; i++)
            {
                Debug.Log("Teste");
                float newDistance = Vector3.Distance(players[i].transform.position, this.transform.position);
                if (newDistance < distanceMin)
                {
                    distanceMin = newDistance;
                    playerProx = players[i];
                }
            }
            hasTarget = true;

            target = playerProx.transform;
            targetEntity = target.GetComponent<LivingEntity>();
            targetNetId = targetEntity.netId;
            
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
        else
        {
            currentState = State.Idle;
       }
    }
	
	// Update is called once per frame
	void Update () {
        if (hasTarget) {
        if(Time.time > nextTimeAttack) {
                try
                {
                   Vector3 teste = target.position;
                } catch(Exception ex)
                {
                    try
                    {
                        target = ClientScene.FindLocalObject(targetNetId).transform;
                    }
                    catch (Exception ex1)
                    {
                        return;
                    }
                }

        float sqrDsToTarget = (target.position - this.transform.position).sqrMagnitude;
        if(sqrDsToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)){
                nextTimeAttack = Time.time + timeBetweenAttacks;
                StartCoroutine(Attack());
             }
            }
        }
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 dirToTarget = (target.position - this.transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius);
        Vector3 originPosition = this.transform.position;

        float attackSpeed = 3;
        float percent = 0;
        bool hasApplieDamage = false;

        while (percent <= 1) {

            if(percent >=.5f && !hasApplieDamage)
            {
                hasApplieDamage = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = 4 * (-Mathf.Pow(percent, 2) + percent);
            this.transform.position = Vector3.Lerp(originPosition, attackPosition, interpolation);
            yield return null;
        }

        pathfinder.enabled = true;
        currentState = State.Chasing;
    }

    IEnumerator UpdatePath() {
        float refreshRate = .25f;
        while (hasTarget) {
            if(currentState == State.Chasing) {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
            if(!dead) 
                pathfinder.SetDestination(targetPosition);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
    
    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if(damage >= health) {
            if (score != null)
            {
                score.OnEnemyKilled();
            }
            Destroy(Instantiate(deathEffect, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, 2);



            CmdDropItem();
            
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    [Command]
    void CmdDropItem()
    {
        // Drop Iten
        int luckyItem = UnityEngine.Random.Range(0, 30);

        if (luckyItem == 3)
        //if (luckyItem >= 0)
        {
            Transform item = Instantiate(itens[UnityEngine.Random.Range(0, itens.Length)], this.transform.position, this.transform.rotation);
            NetworkServer.Spawn(item.gameObject);
        }

    }
    

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float enemyHealth)
    {
        pathfinder.speed = moveSpeed;
        if (hasTarget)
        {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }

        startingHealth = enemyHealth;

    }
}
