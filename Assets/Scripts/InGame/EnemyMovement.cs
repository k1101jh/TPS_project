using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyMovement : MonoBehaviour
{
    NavMeshAgent navMeshAgent;

    private bool isMove;
    private bool isAttacking;
    private bool alreadyAttack;
    private Vector3 destination;

    [SerializeField] float damage;
    [SerializeField] float sightAngle;
    [SerializeField] float sightRange;
    [SerializeField] float attackRange;
    [SerializeField] float attackForeDelay;
    [SerializeField] float attackBackDelay;
    [SerializeField] LayerMask layerMask;

    public LivingEntity targetEntity;
    public Transform eyes;
    private float lastAttackTime;

    private EnemyHealth enemyHealth;

    [SerializeField]
    private bool isTargetInAttackRange = false;

    private bool hasTarget {
        get {
            if(targetEntity != null && !targetEntity.dead) {
                return true;
            }

            return false;
        }
    }

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if(!PhotonNetwork.IsMasterClient) return;
        lastAttackTime = Time.time;
        StartCoroutine(UpdatePath());
    }
    
    public void Initialize(float damage, float sightRange, float sightAngle, float moveSpeed)
    {
        navMeshAgent.updateRotation = false;
        this.damage = damage;
        this.sightRange = sightRange;
        this.sightAngle = sightAngle;
        this.attackRange = 2.0f;
        this.attackForeDelay = 1.0f;
        this.attackBackDelay = 1.5f;
        navMeshAgent.speed = moveSpeed;
    }

    private void FixedUpdate() {
        if(!PhotonNetwork.IsMasterClient) return;

        UpdateRotation();
        CheckTargetInAttackRange();
        UpdateAttack();
    }

    private void UpdateRotation() {
        if(targetEntity != null) {
            Vector3 direction = (targetEntity.transform.position - transform.position).normalized;
            Quaternion q = Quaternion.LookRotation(direction);
            transform.rotation = q;
        }
    }

    private void CheckTargetInAttackRange() {
        if(targetEntity != null) {
            if(Vector3.Distance(targetEntity.transform.position, transform.position) <= attackRange) {
                isTargetInAttackRange = true;
            } else {
                isTargetInAttackRange = false;
            }
        } else {
            isTargetInAttackRange = false;
        }
    }

    private void UpdateAttack() {
        if(!PhotonNetwork.IsMasterClient) {
            return;
        }

        if(isTargetInAttackRange) {
            if(!enemyHealth.dead && Time.time >= lastAttackTime + attackForeDelay) {
                lastAttackTime = Time.time;
                Attack(targetEntity);
            }
        }
    }

    // ?????? ??????
    private IEnumerator UpdatePath() {
        while(!enemyHealth.dead) {
            // ????????? ?????? ?????? NavMeshAgent??? ??????
            if(hasTarget && !isTargetInAttackRange) {
                navMeshAgent.isStopped = false;
                Vector3 distance = targetEntity.transform.position - transform.position;
                navMeshAgent.SetDestination(targetEntity.transform.position - (distance.normalized * attackRange * 0.8f));
                //navMeshAgent.SetDestination(targetEntity.transform.position);
            } else {
                navMeshAgent.isStopped = true;

                // ?????? ????????? LivingEntity?????? ??????
                Collider[] colliders = Physics.OverlapSphere(transform.position, sightRange, layerMask);
                foreach (Collider collider in colliders) {
                    LivingEntity livingEntity = collider.GetComponent<LivingEntity>();

                    if(livingEntity != null && !livingEntity.dead) {
                        Transform targetTransform = collider.transform;
                        Vector3 direction = (targetTransform.position - transform.position).normalized;
                        float angle = Vector3.Angle(direction, transform.forward);

                        // ?????? LivingEntity??? ?????? ?????? ?????? ???????????? ???????????? ??????
                        if(angle < sightAngle * 0.5f) {
                            targetEntity = livingEntity;
                            break;
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    // private void Chase()
    // {
    //     Vector3 targetPosition = targetEntity.transform.position;
    //     Vector3 distance = targetPosition - transform.position;

    //     if(distance.magnitude-0.1 <= attackRange)
    //     {
    //         Debug.Log("Start Attack");
    //         isAttacking = true;
    //         return;
    //     }

    //     Vector3 direction = distance.normalized;
    //     Vector3 dest = targetPosition - (direction * attackRange);
    //     SetDestination(dest);
    // }

    private void Attack(LivingEntity attackTarget) {
        Collider targetCollider = attackTarget.GetComponent<Collider>();
        Vector3 hitPoint = targetCollider.ClosestPoint(transform.position);
        Vector3 hitNormal = transform.position - targetCollider.transform.position;

        attackTarget.OnDamage(damage, hitPoint, hitNormal);
        Debug.Log("Attack!", attackTarget);
    }

    private void Roam()
    {
        //?????? ????????? ???????????????.
    }
}