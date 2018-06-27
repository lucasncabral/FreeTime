using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof (NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State {Idle, Chasing, Attacking};
    State currentState;

    NavMeshAgent pathfinder;
    LivingEntity targetEntity;
    Transform target;

    float attackDistanceThreshold = .5f;
    float timeBetweenAttacks = 1;

    float nextTimeAttack;
    float myCollisionRadius;
    float targetCollisionRadius;

    public GameObject deathEffect;
    private ScoreKeeper score;

    bool hasTarget;

    // itens
    public Transform[] itens;
    
    float damage = 1;

    public int hitsToKill;
    public float speed;
    public float startingHealthBase;

    void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            Action OnTargetDeathAction = () => OnTargetDeath();
            targetEntity.OnDeath += OnTargetDeathAction;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }

        score = FindObjectOfType<ScoreKeeper>();
        SetCharacteristics(speed, hitsToKill, startingHealthBase);

    }

    // Use this for initialization
    protected override void Start () {
        base.Start();
        
        if(hasTarget) {
            currentState = State.Chasing;
            Action OnTargetDeathAction = () => OnTargetDeath();
            targetEntity.OnDeath += OnTargetDeathAction;
        StartCoroutine(UpdatePath());
        }
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }
	
	// Update is called once per frame
	void Update () {
        if (hasTarget) {
        if(Time.time > nextTimeAttack) {
        float sqrDsToTarget = (target.position - transform.position).sqrMagnitude;
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

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius);
        Vector3 originPosition = transform.position;

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
            transform.position = Vector3.Lerp(originPosition, attackPosition, interpolation);
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

            // Drop Item
            int luckyItem = UnityEngine.Random.Range(0, 20);

            if (luckyItem == 3)
            //if (luckyItem >= 0)
            {
                dropItem();
            }
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void dropItem()
    {
        Instantiate(itens[UnityEngine.Random.Range(0,itens.Length)], this.transform.position, this.transform.rotation);
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
