using UnityEngine;

// 적 화살
public class EnemyBullet : MonoBehaviour
{
    private Rigidbody2D rigid;
    
    private float damage;
    private bool  collisionDisappears;  // 충돌 후, 사라지는지(화살,화염구는 충돌 후, 사라짐 / 슬레쉬는 충돌 후, 사라지지 않음)
    
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // 멀어지면, 비활성화
        // 플레이어와 오브젝트 사이의 거리 계산
        float distance = Vector2.Distance(new Vector2(Player.instance.transform.position.x, Player.instance.transform.position.y),
                                          new Vector2(transform.position.x, transform.position.y));
        if (distance > 50f)
            gameObject.SetActive(false);
    }
    
    public void Init(float damage, Vector3 dir, bool collisionDisappears = true)
    {
        this.damage              = damage;
        this.collisionDisappears = collisionDisappears;
        
        // 화살, 화염구는 발사 O // 파티클은 rigid가 없으니, 발사 X
        if(rigid)
            rigid.velocity = dir * 15f;
    }

    // 적과 충돌
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;
        
        if (collision.CompareTag("Player"))
        {
            Player.instance.Hit(damage);
            if(collisionDisappears) // 충돌 후, 비활성화
                gameObject.SetActive(false); // 오브젝트 풀링으로 관리하기 때문에, 삭제 X
        }
    }
    
    private void OnParticleCollision(GameObject other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        if (other.CompareTag("Player"))
        {
            Player.instance.Hit(damage);
            if(collisionDisappears) // 충돌 후, 비활성화
                gameObject.SetActive(false); // 오브젝트 풀링으로 관리하기 때문에, 삭제 X
        }
    }
}