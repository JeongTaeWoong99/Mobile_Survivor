using System;
using UnityEngine;

public class ExplosionCheck : MonoBehaviour
{
    private CircleCollider2D circle2D;

    private void Awake()
    {
        circle2D = GetComponent<CircleCollider2D>();
    }

    public void Check(float damage)
    {
        // CircleCollider2D의 월드 좌표에서의 반지름 계산
        float radius = circle2D.radius * transform.lossyScale.x; // 반지름에 스케일 반영

        // OverlapCircle로 Player 레이어 체크
        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, LayerMask.GetMask("Player"));
        
        if (hit != null)
        {
            // Player 레이어가 감지되었을 때
            Player.instance.Hit(damage); // 플레이어에게 데미지 적용
        }
    }
}