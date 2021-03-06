using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Enemy : LivingEntity
{
    private EnemyMovement enemyMovement;

    public string enemyName;
    public int hp;
    public float damage;
    public float sightRange;
    public float sightAngle;
    public float moveSpeed;

    private void Start() {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    // 추후 수정하기!!
    [PunRPC]
    public void Setup(string enemyName, int hp, int damage, float sightRange, float sightAngle, float moveSpeed) {
        Debug.Log("Enemy.cs: Setup 호출!");
        this.enemyName = enemyName;
        this.hp = hp;
        this.damage = damage;
        this.sightRange = sightRange;
        this.sightAngle = sightAngle;
        this.moveSpeed = moveSpeed;
        enemyMovement = GetComponent<EnemyMovement>();
        enemyMovement.Initialize(damage, sightRange, sightAngle, moveSpeed);
    }

    public void PrintEnemyData()
    {
        Debug.Log("몬스터이름 :: " + enemyName);
        Debug.Log("체력 :: " + hp);
        Debug.Log("공격력 :: " + damage);
        Debug.Log("시야 :: " + sightRange);
        Debug.Log("이동속도 :: " + moveSpeed);
        Debug.Log("---------------------------------------");
    }
}