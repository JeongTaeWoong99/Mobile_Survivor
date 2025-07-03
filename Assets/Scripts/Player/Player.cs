using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player instance;
    
    // 컴포넌트
    [HideInInspector] public Rigidbody2D       rb2D;
    [HideInInspector] public CapsuleCollider2D cap2D;
    [HideInInspector] public SpriteRenderer    spriteRen;
    [HideInInspector] public Animator          anim;
    
    private int     collisionTouchPerDamage;
    private int     strongTintID;           // 히트 쉐이더
    private bool    isResurrection;
    
    // 전역
    public Vector2 inputVec;
    
    [HideInInspector] public bool isSkillProgress;      // 스킬 실행 중
    
    // 상태
    public GameObject[] effectList;
    
    [HideInInspector] public bool isActiveBuff;
    [HideInInspector] public int  activeBuffCount; // 활성화 중 인, 버프 수
    //[HideInInspector] public float[] engageBuffValue;  // 지속 시간 0 // 공격 퍼센트 1 // 이속 퍼센트 2
    
    void Awake()
    {
        instance = this;
    
        rb2D      = GetComponent<Rigidbody2D>();
        cap2D     = GetComponent<CapsuleCollider2D>();
        spriteRen = GetComponentInChildren<SpriteRenderer>();
        anim      = GetComponentInChildren<Animator>();

        //hands = GetComponentsInChildren<Hand>();
    }

    private void Start()
    {
        strongTintID = Shader.PropertyToID("_StrongTintFade");
        spriteRen.material.SetFloat(strongTintID,0);
        
        foreach (var effectLists in effectList)
            effectLists.SetActive(false);
    }

    void FixedUpdate()
    {
        // 히트 쉐이더(감소)
        if(spriteRen.material.GetFloat(strongTintID)>0)
            spriteRen.material.SetFloat(strongTintID,spriteRen.material.GetFloat(strongTintID) - Time.fixedDeltaTime);
        
        // 탐지 
        DetectNearbyExpBeads();
        
        if (!GameManager.instance.isLive || isSkillProgress)
            return;

        // 상태
        StatusEffect();
        
        // 물리 이동
        //Debug.Log((speed  * GameManager.instance.moveSpeedInPer / 100f));
        Vector2 nextVec = inputVec.normalized * ((GameManager.instance.baseSpeed * GameManager.instance.moveSpeedInPer / 100f) * Time.fixedDeltaTime);
        rb2D.MovePosition(rb2D.position + nextVec);
        
        // 사망
        if (GameManager.instance.playerCurrentHealth < 0) 
        {
            // 부활
            if (CharacterPassive.PowerUp2() && !isResurrection)
            {
                isResurrection = true;
                GameManager.instance.playerCurrentHealth = GameManager.instance.playerCurrentMaxHealth / 2;
                effectList[1].SetActive(true);
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Resurrection);
                return;
            }
                
            // 플레이어의 자식 오브젝트 Spawner, Hand Left, Hand Right 비활성화
            // for (int index = 2; index < transform.childCount; index++)
            //     transform.GetChild(index).gameObject.SetActive(false);
            
            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }
    }
    
    void LateUpdate()
    {
        if (!GameManager.instance.isLive || isSkillProgress)
            return;

        // inputVec의 크기에 따라, Run <-> Stand 애니메이션 전환
        anim.SetFloat("Speed", inputVec.magnitude);
        
        // inputVec.x가 입력 중 일 때, 값에 따라서, 좌우 반전
        if (inputVec.x != 0)
            spriteRen.flipX = inputVec.x < 0;   // true Or false
    }    
    
    // 게임 시작시 초기화
    public void Init()
    {
        // Player가 활성화 되면, 선택된 캐릭터의 ID에 맞춰서, 스피드와 애니메이션 설정.
        //speed                         *= CharacterPassive.MoveSpeed;             // 스피드 초기화
        anim.runtimeAnimatorController = GameManager.instance.animCon[GameManager.instance.playerId]; // 애니메이션 초기화
    }
    
    public void Hit(float damage)
    {
        if (!GameManager.instance.isLive || isSkillProgress)
            return;
    
        GameManager.instance.playerCurrentHealth -= damage;
        spriteRen.material.SetFloat(strongTintID,0.2f);
        //Debug.Log(damage);
    }
    
    private void DetectNearbyExpBeads()
    {
        Collider2D[] nearbyBeads = Physics2D.OverlapCircleAll(transform.position, GameManager.instance.baseDetectionRadius * (GameManager.instance.acquisitionRangeInPer / 100), 
                                                                  GameManager.instance.expBeadLayer);
        //Debug.Log(GameManager.instance.baseDetectionRadius * (GameManager.instance.acquisitionRangeInPer / 100));
        foreach (var beadCollider in nearbyBeads)
        {
            DropItem dropItem = beadCollider.GetComponent<DropItem>();
            if (dropItem != null && !dropItem.isFollow)
                dropItem.isFollow = true;
        }
    }
    
    // 상태 이펙트 관리
    private void StatusEffect()
    {
        // 버프류 이펙트(활성화 중인, 버프류가 1개 이상이면, 이펙트가 켜지도록 함.)
        isActiveBuff = activeBuffCount > 0;
        effectList[0].SetActive(isActiveBuff);
        
        // // 스턴 체크
        // stunTimer -= Time.fixedDeltaTime;
        // isStun     = stunTimer > 0;
        // effectList[1].SetActive(isStun);
        // anim.speed = isStun ? 0 : 1;
        //
        // // 쉴드 체크
        // shieldBuffTimer -= Time.fixedDeltaTime;
        // if (shieldBuffTimer > 0 && currentShieldAmount > 0)
        //     isShieldBuff = true;
        // else
        //     isShieldBuff = false;
        // effectList[2].SetActive(isShieldBuff);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!GameManager.instance.isLive || isSkillProgress)
            return;

        // 적과 닿으면, 바로 한번 데미지 입히기
        if (col.gameObject.CompareTag("Enemy"))
        {
            int per = col.gameObject.GetComponent<Enemy>().collisionDamage;
            GameManager.instance.playerCurrentHealth -= per; // 체력 바로 빼기
            collisionTouchPerDamage                  += per; // 초당 데미지 더하기
            
            spriteRen.material.SetFloat(strongTintID,0.2f);
        }
        // 함정
        else if (col.gameObject.CompareTag("Pitfall"))
        {
            int per = (int)col.gameObject.GetComponent<Pitfall>().collisionDamage;
            GameManager.instance.playerCurrentHealth -= per; // 체력 바로 빼기
            collisionTouchPerDamage                  += per; // 초당 데미지 더하기
            
            spriteRen.material.SetFloat(strongTintID,0.2f);
            //Debug.Log("충돌");
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (!GameManager.instance.isLive || isSkillProgress)
            return;
        
        // 적과 닿고 있으면, 지속적으로 피 감소.(중첩값에 따라 떨어트리기)
        if (col.gameObject.CompareTag("Enemy"))
        {
            GameManager.instance.playerCurrentHealth -= Time.deltaTime * collisionTouchPerDamage;
            
            spriteRen.material.SetFloat(strongTintID,0.2f);
        }
        // 함정
        else if (col.gameObject.CompareTag("Pitfall"))
        {
            GameManager.instance.playerCurrentHealth -= Time.deltaTime * collisionTouchPerDamage;
            
            spriteRen.material.SetFloat(strongTintID,0.2f);
           // Debug.Log("스테이");
        }
    }
    
    private void OnCollisionExit2D(Collision2D col)
    {
        if (!GameManager.instance.isLive || isSkillProgress)
            return;
        
        // 떨어진 적의 enemyTouchPerDamage 빼주기
        if (col.gameObject.CompareTag("Enemy"))
        {
            int per = col.gameObject.GetComponent<Enemy>().collisionDamage;
            collisionTouchPerDamage -= per; // 초당 데미지 빼기
        }
        // 함정
        else if (col.gameObject.CompareTag("Pitfall"))
        {
            int per = (int)col.gameObject.GetComponent<Pitfall>().collisionDamage;
            collisionTouchPerDamage -= per; // 초당 데미지 빼기
            //Debug.Log("나감");
        }
    }
}

