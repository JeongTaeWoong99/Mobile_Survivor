using System;
using UnityEngine;

// 근접 or 원거리 등등
public class PlayerBullet : MonoBehaviour
{
    // 컴포넌트
    [HideInInspector] public Rigidbody2D      rigid;
    [HideInInspector] public BoxCollider2D    box2D;
    [HideInInspector] public CircleCollider2D circle2D;
    
    // 공통
    [HideInInspector] public float damage;
    [HideInInspector] public int   maxPen; // 관통력 (-100은 회전 근접 무기 같은, 무한 지속 무기들)
    [HideInInspector] public float range;

    [HideInInspector] public int remainPen;
    
    // 히트 이펙트(폭발로 데미지를 주거나, 그냥 히트 이펙트로 쓰거나)
    public GameObject hitEffect;
    private int   hitEffectId;
    private float hitEffectDamage;
    
    // 조건 발사(주먹)
    public  bool         isConditionalShoot; // 만들어지고, 조건에 따라 발사
    private bool         isShoot;
    
    private int          enemyLayerMask;
    private Collider2D[] targets;        // 서클 안에 들어온, 모든 타겟(FixedUpdate 갱신)
    private Transform    nearestTarget;  // 제일 가까운 타겟
    
    void Awake()
    {
        rigid    = GetComponent<Rigidbody2D>();
        box2D    = GetComponent<BoxCollider2D>();
        circle2D = GetComponent<CircleCollider2D>();
        
        int enemyLayer = 6;
        enemyLayerMask = 1 << enemyLayer;
    }

    private void Start()
    {
        // 안전성 검사: PoolManager와 prefabs 배열 확인
        if (PoolManager.instance == null || PoolManager.instance.prefabs == null)
        {
            Debug.Log("PlayerBullet: PoolManager가 초기화되지 않았습니다.");
            return;
        }
        
        if (hitEffect == null)
        {
            hitEffectId = -1; // 히트 이펙트가 없음을 표시
            return;
        }
        
        // hitEffect가 PoolManager의 prefabs에 있는지 확인
        for (int index = 0; index < PoolManager.instance.prefabs.Length; index++) 
        {
            if (PoolManager.instance.prefabs[index] != null && hitEffect == PoolManager.instance.prefabs[index]) 
            {
                hitEffectId = index;
                return;
            }
        }
        
        // 히트 이펙트를 찾지 못한 경우
        hitEffectId = -1;
        Debug.Log($"PlayerBullet: 히트 이펙트 '{hitEffect.name}'를 PoolManager에서 찾을 수 없습니다.");
    }

    private void FixedUpdate()
    {
        // 런타임 안전성 검사: Player.instance null 체크
        if (Player.instance == null || Player.instance.transform == null)
        {
            Debug.Log("PlayerBullet: Player.instance가 null입니다. 투사체를 비활성화합니다.");
            gameObject.SetActive(false);
            return;
        }
        
        // 일반 발사체 비활성화(플레이어와 불릿의 거리)
        float distance = Vector2.Distance(Player.instance.transform.position, transform.position);
        if (!isConditionalShoot)
        {
            if(distance > 50f)
                gameObject.SetActive(false);
        }
        // 조건 발사체 비활성화(불릿과 타겟의 거리)
        else if (isConditionalShoot && isShoot)
        {   
            if(distance > 10f)
                gameObject.SetActive(false);
        }
        
        // 조건 발사
        if (isConditionalShoot && !isShoot)
        {   
            targets = Physics2D.OverlapCircleAll(transform.position,range,enemyLayerMask); // 전체 타겟 갱싱
            nearestTarget = GetNearest();
            if (nearestTarget != null)
                ConditionalShot(nearestTarget);
        }
    }
    
    // 공통 초기화
    public void Init(float damage, int maxPen, float range,Vector3 dir, bool initShoot, float hitEffectDamage, float initShootVelocity = 15)
    {
        this.damage          = damage;
        this.maxPen          = maxPen;
        remainPen            = maxPen;
        this.range           = range;
        this.hitEffectDamage = hitEffectDamage;

        // 이동(관통으로 체크. -100은 근접 무기류) && 바로 발사
        if (initShoot)
            rigid.velocity = dir * initShootVelocity;
    }
    
    private void OnEnable()
    {
        remainPen = maxPen;
        
        // 오브젝트 풀링 재활용 시 완전 초기화
        if (rigid != null)
        {
            rigid.velocity = Vector2.zero;
            rigid.angularVelocity = 0f;
        }
        
        // 회전 초기화
        transform.rotation = Quaternion.identity;
        
        // 조건 발사(주먹) 관련 초기화
        if (!isConditionalShoot) 
            return;
        
        targets       = null;
        nearestTarget = null;
        isShoot       = false;
        box2D.enabled = false;
        if(rigid)
            Destroy(rigid);
    }

    private void ConditionalShot(Transform targetTrans)
    {
        // 런타임 안전성 검사: 타겟이 여전히 유효한지 확인
        if (targetTrans == null || !targetTrans.gameObject.activeInHierarchy)
        {
            Debug.Log("PlayerBullet: ConditionalShot - 타겟이 null이거나 비활성화되었습니다.");
            nearestTarget = null; // 타겟 초기화
            return;
        }
        
        isShoot       = true;
        box2D.enabled = true;
        
        Vector3 targetPos  = targetTrans.position;
        Vector3 dir        = targetPos - transform.position;
        dir                = dir.normalized;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        
        if (!rigid)
        {
            rigid = gameObject.AddComponent<Rigidbody2D>();
            rigid.gravityScale = 0f;
            rigid.AddForce(dir * 25  ,ForceMode2D.Impulse);
        }
    }
    
    private Transform GetNearest()
    {
        Transform result = null;
        float     diff   = 100;

        // 런타임 안전성 검사: targets 배열이 null이거나 비어있는 경우
        if (targets == null || targets.Length == 0)
        {
            return null;
        }

        foreach (var target in targets) 
        {
            // 런타임 안전성 검사: 타겟이 null이거나 비활성화된 경우 건너뛰기
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                Debug.Log("PlayerBullet: GetNearest - 죽은 적을 타겟에서 제외합니다.");
                continue;
            }
            
            // 런타임 안전성 검사: Transform 컴포넌트 null 체크
            if (target.transform == null)
            {
                Debug.Log("PlayerBullet: GetNearest - 타겟의 Transform이 null입니다.");
                continue;
            }
            
            // 화면에 보이지 않는 적 제외
            if (!IsTargetVisible(target.gameObject))
            {
                continue;
            }
            
            Vector3 myPos     = transform.position;
            Vector3 targetPos = target.transform.position;
            float   curDiff   = Vector3.Distance(myPos, targetPos); // Distance 측정
            
            // 거리값을 비교하여, 교체
            if (curDiff < diff) 
            {
                diff   = curDiff;
                result = target.transform;
            }
        }
        return result;
    }
    
    /// <summary>
    /// 타겟이 화면에 보이는지 확인하는 메서드
    /// </summary>
    private bool IsTargetVisible(GameObject target)
    {
        // 런타임 안전성 검사: Camera.main null 체크
        if (Camera.main == null)
        {
            Debug.LogError("PlayerBullet: IsTargetVisible - Camera.main이 null입니다.");
            return false;
        }
        
        // 타겟의 중심점이 화면 범위 안에 있는지 확인
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(target.transform.position);
        
        // 화면 가장자리에서 약간의 마진을 두고 체크 (0.05 ~ 0.95, 0.1 ~ 0.9)
        if (viewportPoint.x < 0.05f || viewportPoint.x > 0.95f ||
            viewportPoint.y < 0.1f || viewportPoint.y > 0.9f ||
            viewportPoint.z < 0) // 카메라 앞에 있어야 함
        {
            return false; // 화면에 보이지 않음
        }
        
        return true; // 화면에 보임
    }
    
    // 적과 충돌
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") || maxPen == -100)
            return;
        
        remainPen--;
        if (remainPen <= 0)
        {
            // 히트 이펙트가 있으면, 생성
            if (hitEffect && hitEffectId >= 0)
            {
                // 런타임 안전성 검사: PoolManager 확인
                if (PoolManager.instance != null)
                {
                    GameObject hitEffectObj = PoolManager.instance.GetWithPosition(hitEffectId, transform.position);
                    if (hitEffectObj != null)
                    {
                        Transform hitEffectClone = hitEffectObj.transform;
                        
                        // 불릿 있는지 확인(-> 없으면, 그냥 데미지 없는 히트 이펙트)
                        PlayerBullet hitEffectBullet = hitEffectClone.GetComponent<PlayerBullet>();
                        if (hitEffectBullet != null)
                            hitEffectBullet.Init(hitEffectDamage, -100, 0, Vector3.zero, false, 0f);
                    }
                }
            }
        
            if(rigid)
                rigid.velocity = Vector2.zero;
            gameObject.SetActive(false);    // 오브젝트 풀링으로 관리하기 때문에, 삭제 X

            if (isConditionalShoot)
                targets = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
