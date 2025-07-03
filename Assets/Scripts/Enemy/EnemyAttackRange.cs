using System;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyAttackRange : MonoBehaviour
{
    private bool             playerInRange = false; 
    [HideInInspector] public BoxCollider2D    boxCol2D; // 근접(애니메이션 + 충돌 범위 고려)
    [HideInInspector] public CircleCollider2D cirCol2D; // 원거리(공격 가능 범위)

    private void Awake()
    {
        boxCol2D = GetComponent<BoxCollider2D>();
        cirCol2D = GetComponent<CircleCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }
}