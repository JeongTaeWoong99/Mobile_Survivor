using System.Collections;
using System.Linq;
using System.Resources;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
    // 컴포넌트
    [HideInInspector] public Rigidbody2D        rigid;
    [HideInInspector] public CapsuleCollider2D  cap2D;
    [HideInInspector] public Animator           anim;
    [HideInInspector] public SpriteRenderer     spriteRen;
    [HideInInspector] public EnemyAttackRange[] enemyAttackRange; // 공격 범위 배열(근접 / 원거리)
    [HideInInspector] public EnemyAimeFunction  enemyAimeFunction;

    // 전역 변수
    private float originCollSizeX; // 스몰 타입 적은 오리진 그대로 사용
    private float originCollSizeY; // 빅 타입 적은 오리진 Y에서 2곱 해서 사용하고, 오프셋은 오리진 Y에서 2나눈 값을 사용. 
    
    [HideInInspector] public bool   isLive;
    [HideInInspector] public string currentClipName = ""; // 현재 재생 애니메이션
    
    // 성능 최적화: 애니메이션 관련 캐싱
    private int lastAnimStateHash = -1;
    private int attackClipCount = 0;
    private readonly Dictionary<GameObject, int> prefabToIDCache = new Dictionary<GameObject, int>();
    
    // 성능 최적화: 상태 플래그들
    private bool isAttacking = false;
    private bool isHitting = false;
    private bool lastFacingRight = true;
    private float hitShaderValue = 0f;
    
    // 성능 최적화: 상태 이펙트 캐싱
    private bool[] lastEffectStates = new bool[4];
    
    // 성능 최적화: 메모리 할당 최소화를 위한 캐시된 변수들
    private Vector3 cachedPosition;
    private Vector3 cachedScale = Vector3.one;
    private Vector3 cachedScaleFlipped = new Vector3(-1, 1, 1);
    private int[] tempAttackIndices = new int[10]; // 최대 10개 공격 타입 가정
    private const float MIN_CREATE_TRANS = -0.5f;
    private const float MAX_CREATE_TRANS = 0.5f;
    
    [HideInInspector] public float currentHealth; // 현재 체력
    private float[] attackRateTimer; // 공격 시간 체크
    private int     strongTintID;    // 히트 쉐이더 메터리얼 아이디
    
    private bool isStun;
    [HideInInspector] public float stunTimer;

    private float delayTime = 1; // 스폰 + 스킬 or 공격 간 딜레이
    private float delayTimer;

    // EnemySpawnData에 따라 초기화 되는 값
    public RuntimeAnimatorController[]           animByID; // ID에 따른, 애니메이션 설정
    [HideInInspector] public EnemyData.EnemyType enemyType;
    //[HideInInspector] public EnemyData.BodyType  bodyType;
    [HideInInspector] public int                 collisionDamage;
    [HideInInspector] public float               maxHealth;
    [HideInInspector] public float[]             attackDamage;
    [HideInInspector] public float[]             attackRate;
    [HideInInspector] public float               attackRange;
    [HideInInspector] public float               speed;
    [HideInInspector] public int[]               minMaxEXP;
    [HideInInspector] public Gold                gold;

    [HideInInspector] public int[] chargingEffectID; // 차징 이펙트 받아오기
    [HideInInspector] public int[] waringEffectID;   // 워링 이펙트 받아오기
    [HideInInspector] public int[] attackEffectID;   // 어택 이펙트 받아오기
    
    // 엘리트 or 보스
    [HideInInspector] public EnemyData.AttackType[] attackType;
    [HideInInspector] public float[]                attackSpeed;

    [HideInInspector] public bool    isEngageBuff;
    [HideInInspector] public float   engageBuffTimer; 
    [HideInInspector] public float[] engageBuffValue; // 지속 시간 0 // 공격 퍼센트 1 // 이속 퍼센트 2
    
    [HideInInspector] public bool    isShieldBuff;
    [HideInInspector] public float   shieldBuffTimer;     // 지속 시간
    [HideInInspector] public float[] shieldBuffValue;     // 지속 시간 0 // 멕스 쉴드 양
    [HideInInspector] public float   currentShieldAmount; // 남은 쉴드 양
    
    // 공통 On-Off 이펙트
    public GameObject   statusEffectGameObject; // 바디 타입에 따라, 이펙트 크기 조정(스몰 0.5 / 빅 1)
    public GameObject[] effectList;
    
    // 공통 생성 오브젝트
    public  GameObject dropItemPrefabs;
    private int        dropItemID;      // 경험치 구술 아이디 받아오기

    private void Awake()
    {
        rigid             = GetComponentInChildren<Rigidbody2D>();
        anim              = GetComponentInChildren<Animator>();
        spriteRen         = GetComponentInChildren<SpriteRenderer>();
        enemyAttackRange  = GetComponentsInChildren<EnemyAttackRange>(true); // 모든 받아오기
        enemyAimeFunction = GetComponentInChildren<EnemyAimeFunction>();
        
        cap2D           = GetComponentInChildren<CapsuleCollider2D>();
        originCollSizeX = cap2D.size.x;
        originCollSizeY = cap2D.size.y;
        
        strongTintID   = Shader.PropertyToID("_StrongTintFade");
        
        // 성능 최적화: prefab ID 캐시 생성
        CachePrefabIDs();
        
        // 성능 최적화: 이펙트 상태 배열 초기화
        lastEffectStates = new bool[effectList.Length];
    }

    private void FixedUpdate()
    {
        // 성능 최적화: 애니메이션 상태가 변경될 때만 클립 이름 갱신
        if (anim != null)
        {
            int currentStateHash = anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
            if (currentStateHash != lastAnimStateHash)
            {
                lastAnimStateHash = currentStateHash;
                AnimatorClipInfo[] clipInfo = anim.GetCurrentAnimatorClipInfo(0);
                if (clipInfo.Length > 0)
                {
                    currentClipName = clipInfo[0].clip.name;
                    // 성능 최적화: 상태 플래그 업데이트
                    isAttacking = currentClipName.Contains("Attack");
                    isHitting = currentClipName.Equals("Hit");
                }
            }
        }
        
        // 성능 최적화: 히트 쉐이더 값이 0보다 클 때만 처리
        if (hitShaderValue > 0)
        {
            hitShaderValue -= Time.fixedDeltaTime;
            if (hitShaderValue <= 0)
            {
                hitShaderValue = 0;
                spriteRen.material.SetFloat(strongTintID, 0);
            }
            else
            {
                spriteRen.material.SetFloat(strongTintID, hitShaderValue);
            }
        }
        
        // 상태 이펙트(살아 있을 때 -> 상태에 따라 OnOFF // 죽었을 때 -> 모두 다, 끄기)
        StatusEffect();

        if (!GameManager.instance.isLive || !isLive || isStun)
            return;
        
        Move();
        Attack();
    }

    // 성능 최적화: prefab ID 캐시 생성
    private void CachePrefabIDs()
    {
        prefabToIDCache.Clear();
        
        // 안전성 검사: PoolManager 초기화 확인
        if (PoolManager.instance == null || PoolManager.instance.prefabs == null)
        {
            Debug.LogError("Enemy: PoolManager가 초기화되지 않았습니다. 캐시 생성을 건너뜁니다.");
            return;
        }
        
        for (int i = 0; i < PoolManager.instance.prefabs.Length; i++)
        {
            // null 프리팹 체크 (모바일에서 중요!)
            if (PoolManager.instance.prefabs[i] != null)
            {
                prefabToIDCache[PoolManager.instance.prefabs[i]] = i;
            }
            else
            {
                Debug.LogWarning($"Enemy: PoolManager.prefabs[{i}]가 null입니다. 건너뜁니다.");
            }
        }
        
        Debug.Log($"Enemy: prefabToIDCache 초기화 완료. {prefabToIDCache.Count}개 등록됨.");
    }

    // 성능 최적화: 공격 클립 수 캐싱
    private void CacheAttackClipCount()
    {
        attackClipCount = 0;
        if (anim.runtimeAnimatorController != null)
        {
            foreach (var clip in anim.runtimeAnimatorController.animationClips)
            {
                if (clip.name.Contains("Attack"))
                    attackClipCount++;
            }
        }
    }

    // OnEnable -> Init
    public void Init(EnemyData data)
    {
        // 안전성 검사: 기본 검증
        if (data == null)
        {
            Debug.LogError("Enemy: Init - EnemyData가 null입니다!");
            return;
        }
        
        // 안전성 검사: PoolManager 재확인 (모바일에서 중요!)
        if (PoolManager.instance == null || PoolManager.instance.prefabs == null)
        {
            Debug.LogError("Enemy: PoolManager가 아직 초기화되지 않았습니다. 1프레임 후 재시도합니다.");
            StartCoroutine(DelayedInit(data));
            return;
        }
        
        // prefabToIDCache가 비어있으면 다시 생성 (모바일 안전성)
        if (prefabToIDCache.Count == 0)
        {
            Debug.LogWarning("Enemy: prefabToIDCache가 비어있습니다. 다시 생성합니다.");
            CachePrefabIDs();
        }
        
        // [Header("----- Info -----")]
        // 안전성 검사: 애니메이션 컨트롤러 배열 확인
        if (animByID == null || data.enemyID < 0 || data.enemyID >= animByID.Length || animByID[data.enemyID] == null)
        {
            Debug.LogError($"Enemy: animByID[{data.enemyID}]가 유효하지 않습니다. Inspector에서 확인해주세요.");
            return;
        }
        
        anim.runtimeAnimatorController = animByID[data.enemyID];
        enemyType                      = data.enemyType;
        
        // 성능 최적화: 공격 클립 수 캐싱
        CacheAttackClipCount();
        
        switch (data.bodyType)
        {
            // 스몰 바디타입 == 오리진 사용
            case EnemyData.BodyType.small:
                cap2D.size   = new Vector2(originCollSizeX, originCollSizeY);
                cap2D.offset = new Vector2(0, 0);
                statusEffectGameObject.transform.localScale = Vector3.one / 2f;
                break;
            // 빅 바디타입 == 늘려주기
            case EnemyData.BodyType.big:
                cap2D.size   = new Vector2(originCollSizeX, originCollSizeY * 2f);
                cap2D.offset = new Vector2(0, originCollSizeY / 2f);
                statusEffectGameObject.transform.localScale = Vector3.one;
                break;
        }

        // [Header("----- Value -----")] 
        collisionDamage = data.collisionDamage;
        maxHealth       = data.maxHealth;
        currentHealth   = maxHealth;
        
        // 보스 체력 초기화 확인 로그
        if (enemyType == EnemyData.EnemyType.Boss)
        {
            Debug.Log($"보스 초기화: 체력 {currentHealth}/{maxHealth} 설정 완료");
        }
        
        // 안전성 검사: 배열들 null 체크
        if (data.attackDamage != null)
            attackDamage = data.attackDamage;
        else
        {
            Debug.LogWarning($"Enemy: '{data.name}'의 attackDamage가 null입니다. 기본값 사용.");
            attackDamage = new float[] { 10f }; // 기본값
        }
        
        if (data.attackRate != null)
        {
            attackRate = data.attackRate;
            attackRateTimer = new float[data.attackRate.Length];
        }
        else
        {
            Debug.LogWarning($"Enemy: '{data.name}'의 attackRate가 null입니다. 기본값 사용.");
            attackRate = new float[] { 2000f }; // 기본값 (2초)
            attackRateTimer = new float[1];
        }
        
        if (enemyType == EnemyData.EnemyType.Boss)  // 보스는 바로 스킬 사용 가능하도록 함.
        {
            for (int i = 0; i < attackRate.Length; i++)
                attackRateTimer[i] = attackRate[i]/1000f - delayTime; // 시작 공격 or 스킬 1초 딜레이
        }
        
        attackRange     = data.attackRange;
        speed           = data.speed;
        
        // 안전성 검사: EXP 배열
        if (data.minMaxEXP != null && data.minMaxEXP.Length >= 2)
            minMaxEXP = data.minMaxEXP;
        else
        {
            Debug.LogWarning($"Enemy: '{data.name}'의 minMaxEXP가 유효하지 않습니다. 기본값 사용.");
            minMaxEXP = new int[] { 1, 5 }; // 기본값
        }
        
        gold = data.gold;

        // 타입에 따른 공격 세팅 초기화
        if (enemyType is EnemyData.EnemyType.Elite or EnemyData.EnemyType.Boss)
        {
            enemyAttackRange[0].gameObject.SetActive(true);
            enemyAttackRange[1].gameObject.SetActive(true);
            enemyAttackRange[1].cirCol2D.radius = attackRange;
            
            attackType      = data.attackType;
            attackSpeed     = data.attackMoveSpeed;
            
            // 광폭화 정보 받아오기
            engageBuffValue = data.enrageBuffValue;
            
            // 쉴드 정보 받아오기
            shieldBuffValue = data.shieldBuffValue;
        }
        else if (enemyType is EnemyData.EnemyType.Melee)
            enemyAttackRange[0].gameObject.SetActive(true);
        else if (enemyType is EnemyData.EnemyType.Range)
        {
            enemyAttackRange[1].gameObject.SetActive(true);
            enemyAttackRange[1].cirCol2D.radius = attackRange; // 원거리 공격 범위 초기화
        }

        // 안전성 검사: 이펙트 ID 검색 (모바일 중요!)
        if (data.waringEffect != null)
        {
            waringEffectID = new int[data.waringEffect.Length];
            for (int num = 0; num < data.waringEffect.Length; num++)
            {
                if (data.waringEffect[num] != null && prefabToIDCache.TryGetValue(data.waringEffect[num], out int id))
                {
                    waringEffectID[num] = id;
                }
                else
                {
                    waringEffectID[num] = -1; // 찾지 못한 경우 기본값
                    Debug.LogWarning($"Enemy: waringEffect[{num}]을 PoolManager에서 찾을 수 없습니다.");
                }
            }
        }
        else
        {
            waringEffectID = new int[0]; // 빈 배열
        }
        
        if (data.attackEffect != null)
        {
            attackEffectID = new int[data.attackEffect.Length];
            for (int num = 0; num < data.attackEffect.Length; num++)
            {
                if (data.attackEffect[num] != null && prefabToIDCache.TryGetValue(data.attackEffect[num], out int id))
                {
                    attackEffectID[num] = id;
                }
                else
                {
                    attackEffectID[num] = -1; // 찾지 못한 경우 기본값
                    Debug.LogWarning($"Enemy: attackEffect[{num}]을 PoolManager에서 찾을 수 없습니다.");
                }
            }
        }
        else
        {
            attackEffectID = new int[0]; // 빈 배열
        }
        
        // 드랍 아이템 ID 받아오기
        if (dropItemPrefabs != null && prefabToIDCache.TryGetValue(dropItemPrefabs, out int dropID))
        {
            dropItemID = dropID;
        }
        else
        {
            dropItemID = 1; // 기본값: EXP 구슬 (일반적으로 1번 인덱스)
            if (dropItemPrefabs == null)
                Debug.LogWarning("Enemy: dropItemPrefabs가 null입니다. 기본값 사용.");
            else
                Debug.LogWarning("Enemy: dropItemPrefabs를 PoolManager에서 찾을 수 없습니다. 기본값 사용.");
        }
    }

    // 지연된 초기화 (PoolManager 초기화 대기)
    private System.Collections.IEnumerator DelayedInit(EnemyData data)
    {
        // 1프레임 대기
        yield return null;
        
        // 재시도
        if (PoolManager.instance != null && PoolManager.instance.prefabs != null)
        {
            Debug.Log("Enemy: PoolManager 초기화 완료. Init 재시도.");
            Init(data);
        }
        else
        {
            Debug.LogError("Enemy: PoolManager 초기화 실패. Enemy를 비활성화합니다.");
            gameObject.SetActive(false);
        }
    }

    // OnEnable -> Init
    private void OnEnable()
    {
        isLive                 = true;
        cap2D.enabled          = true;
        rigid.simulated        = true;
        spriteRen.sortingOrder = 2;

        // 체력 초기화 (중요: 오브젝트 풀에서 재사용 시 체력 복원)
        if (maxHealth > 0)
        {
            currentHealth = maxHealth;
            
            // 보스 체력 복원 확인 로그
            if (enemyType == EnemyData.EnemyType.Boss)
            {
                Debug.Log($"보스 재활성화: 체력 {currentHealth}/{maxHealth} 복원 완료");
            }
        }

        foreach (var effectLists in effectList)
            effectLists.SetActive(false);
        
        hitShaderValue = 0;
        spriteRen.material.SetFloat(strongTintID, 0);
        
        // 성능 최적화: 이펙트 상태 초기화
        for (int i = 0; i < lastEffectStates.Length; i++)
            lastEffectStates[i] = false;
            
        // 버프 상태 초기화
        isEngageBuff         = false;
        engageBuffTimer      = 0;
        isShieldBuff         = false;
        shieldBuffTimer      = 0;
        currentShieldAmount  = 0;
        stunTimer            = 0;
        isStun               = false;
        
        // 애니메이션 상태 초기화
        isAttacking = false;
        isHitting   = false;
        
        // 애니메이션 속도 정상화
        if (anim != null)
            anim.speed = 1;
    }

    private void Move()
    {
        // 성능 최적화: 상태 플래그 사용
        if (isHitting || isAttacking)
            return;
        
        // 런타임 안전성 검사: Player.instance null 체크
        if (Player.instance == null || Player.instance.rb2D == null)
        {
            Debug.Log("Enemy: Move - Player.instance가 null입니다. 이동을 중단합니다.");
            return;
        }
        
        // 이동
        Vector2 dirVec = Player.instance.rb2D.position - rigid.position;
        Vector2 nextVec;
        if(isEngageBuff)
            nextVec = dirVec.normalized * (speed * engageBuffValue[2]/100 * Time.fixedDeltaTime);
        else
            nextVec = dirVec.normalized * (speed * Time.fixedDeltaTime);
        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity  = Vector2.zero;
        
        // 성능 최적화: 방향이 바뀔 때만 localScale 변경 + Vector3 할당 최소화
        bool shouldFaceRight = Player.instance.rb2D.position.x >= rigid.position.x;
        if (shouldFaceRight != lastFacingRight)
        {
            lastFacingRight = shouldFaceRight;
            gameObject.transform.localScale = shouldFaceRight ? cachedScale : cachedScaleFlipped;
        }
    }

    private void Attack()
    {
        // 쿨타임 초기화
        for (int i = 0; i < attackRateTimer.Length; i++)
            attackRateTimer[i] += Time.fixedDeltaTime;
            
        // 공격 or 스킬 간 딜레이 1초
        delayTimer -= Time.fixedDeltaTime;

        // 성능 최적화: 상태 플래그 사용
        if (isAttacking)
        {
            // 어택이 끝난 후, 쿨타임이 돌아가도록 함.
            // 해당되는 공격의 쿨타임 감소 멈추기...
            attackRateTimer[AnimAttackNum()] = 0f;
            rigid.velocity                   = Vector2.zero;
            delayTimer                       = delayTime;
        }
        // 공격 중 X
        else if (enemyType != EnemyData.EnemyType.NonAttack && delayTimer < 0)
        {
            // 일반 적(공격 종류가 1가지)
            if (enemyType != EnemyData.EnemyType.Elite && enemyType != EnemyData.EnemyType.Boss)
            {
                // 성능 최적화: 캐시된 공격 클립 수 사용
                if (attackClipCount > 0)
                {
                    int randomIndex = Random.Range(0, attackClipCount);

                    // 선택된 랜덤 공격이, 아직 쿨타임 중이라면, 공격 X
                    if (attackRateTimer[randomIndex] < attackRate[randomIndex] / 1000f)
                        return;

                    // 공격 범위 체크
                    if (enemyType == EnemyData.EnemyType.Melee && enemyAttackRange[0].IsPlayerInRange())
                        anim.SetTrigger($"Attack{randomIndex}");
                    else if (enemyType == EnemyData.EnemyType.Range && enemyAttackRange[1].IsPlayerInRange())
                        anim.SetTrigger($"Attack{randomIndex}");
                }
            }
            // 엘리트(공격 종류가 여러가지)
            else
            {
                // 사용할 수 있는 스킬이 있는지 확인(-1은 해당하는 공격 타입이 없는 경우)
                int randomSkillNum = GetAttackTypeIndex(EnemyData.AttackType.Skill);
                int randomMeleeNum = GetAttackTypeIndex(EnemyData.AttackType.Melee);
                int randomRangeNum = GetAttackTypeIndex(EnemyData.AttackType.Range);
                
                // 스킬 : 1순위
                if (randomSkillNum != -1 && attackRateTimer[randomSkillNum] > attackRate[randomSkillNum] / 1000f)
                    anim.SetTrigger($"Attack{randomSkillNum}");
                // 근접 일반 공격 : 2순위
                else if (randomMeleeNum != -1 && attackRateTimer[randomMeleeNum] > attackRate[randomMeleeNum] / 1000f && enemyAttackRange[0].IsPlayerInRange())
                    anim.SetTrigger($"Attack{randomMeleeNum}");
                // 원거리 일반 공격 : 3순위
                else if (randomRangeNum != -1 && attackRateTimer[randomRangeNum] > attackRate[randomRangeNum] / 1000f && enemyAttackRange[1].IsPlayerInRange())
                    anim.SetTrigger($"Attack{randomRangeNum}");
            }
        }
    }

    // 성능 최적화: List 생성을 배열로 교체하여 메모리 할당 제거
    private int GetAttackTypeIndex(EnemyData.AttackType findType)
    {
        int validCount = 0;
        
        // 유효한 인덱스들을 캐시된 배열에 저장
        for (int i = 0; i < attackType.Length; i++)
        {
            if (attackType[i] == findType)
            {
                tempAttackIndices[validCount] = i;
                validCount++;
                if (validCount >= tempAttackIndices.Length) break; // 오버플로우 방지
            }
        }

        if (validCount == 0)
            return -1;

        int randomIndex = Random.Range(0, validCount);
        return tempAttackIndices[randomIndex];
    }

    // 현재 어택 넘버 받아오기
    public int AnimAttackNum()
    {
        string attackClipName = currentClipName;
        // "Attack"을 분리하고 숫자만 얻어옴
        string attackNum = attackClipName.Replace("Attack", ""); // "Attack0" -> "0"
        int.TryParse(attackNum, out int attackIndex);
        return attackIndex; // 존재하면 공격 번호 리턴    
    }
    
    private IEnumerator KnockBack() // 공통
    {
        // 확실하게, 뒤로 밀리게 하기 위해, 2번 기다리기.(Hit로 애니메이션 전환 되고, 작동되게 하기 위해...!)
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
    
        // 런타임 안전성 검사: Player.instance null 체크
        if (Player.instance == null || Player.instance.transform == null)
        {
            Debug.Log("Enemy: KnockBack - Player.instance가 null입니다. 넉백을 중단합니다.");
            yield break;
        }
        
        Vector3 playerPos = Player.instance.transform.position;
        Vector3 dirVec    = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 2, ForceMode2D.Impulse);
    }
    
    // 성능 최적화: 상태 이펙트 관리 (상태 변경 시에만 SetActive 호출)
    private void StatusEffect()
    {
        // 광폭화 체크
        engageBuffTimer -= Time.fixedDeltaTime;
        bool currentEngageBuff = engageBuffTimer > 0;
        if (isEngageBuff != currentEngageBuff)
        {
            isEngageBuff = currentEngageBuff;
            if (lastEffectStates[0] != isEngageBuff)
            {
                effectList[0].SetActive(isEngageBuff);
                lastEffectStates[0] = isEngageBuff;
            }
        }
        
        // 스턴 체크
        stunTimer -= Time.fixedDeltaTime;
        bool currentStun = stunTimer > 0;
        Debug.Log(stunTimer);
        if (isStun != currentStun)
        {
            Debug.Log("스턴");
            isStun = currentStun;
            if (lastEffectStates[1] != isStun)
            {
                effectList[1].SetActive(isStun);
                lastEffectStates[1] = isStun;
            }
            anim.speed = isStun ? 0 : 1;
        }
        else
        {
            Debug.Log("스턴 아님");
        }
        
        // 쉴드 체크
        shieldBuffTimer -= Time.fixedDeltaTime;
        bool currentShieldBuff = shieldBuffTimer > 0 && currentShieldAmount > 0;
        if (isShieldBuff != currentShieldBuff)
        {
            isShieldBuff = currentShieldBuff;
            if (lastEffectStates[2] != isShieldBuff)
            {
                effectList[2].SetActive(isShieldBuff);
                lastEffectStates[2] = isShieldBuff;
            }
        }
        
        // 차징 체크
        bool currentCharging = enemyAimeFunction.isCharging;
        if (lastEffectStates[3] != currentCharging)
        {
            effectList[3].SetActive(currentCharging);
            lastEffectStates[3] = currentCharging;
            if (currentCharging)
                effectList[3].transform.position = enemyAimeFunction.chargingToEffectPos.position;
        }
    }
    
    // 성능 최적화: Vector3 할당 최소화
    private void ExpBeadCreate()
    {
        // 안전성 검사
        if (dropItemID < 0)
        {
            Debug.LogError($"Enemy: ExpBeadCreate - dropItemID({dropItemID})가 유효하지 않습니다.");
            return;
        }
        
        // 경험치 구슬 생성 (위치 설정 후 활성화)
        cachedPosition = transform.position;
        cachedPosition.x += Random.Range(MIN_CREATE_TRANS, MAX_CREATE_TRANS);
        cachedPosition.y += Random.Range(MIN_CREATE_TRANS, MAX_CREATE_TRANS);
        
        GameObject expBeadObj = PoolManager.instance.GetWithPosition(dropItemID, cachedPosition);
        if (expBeadObj != null)
        {
            DropItem dropItemComponent = expBeadObj.GetComponent<DropItem>();
            if (dropItemComponent != null && minMaxEXP != null && minMaxEXP.Length >= 2)
            {
                dropItemComponent.EXP_BeadInit(minMaxEXP[0],minMaxEXP[1]);
            }
        }
    }

    // 성능 최적화: Vector3 할당 최소화
    private void GoldCreate()
    {
        // 안전성 검사
        if (dropItemID < 0)
        {
            Debug.LogError($"Enemy: GoldCreate - dropItemID({dropItemID})가 유효하지 않습니다.");
            return;
        }
        
        int randomDropNum = 0;
        if(gold.minMaxDrop != null && gold.minMaxDrop.Length == 2)
            randomDropNum  = Random.Range(gold.minMaxDrop[0], gold.minMaxDrop[1]);
        
        int randomDropRate = gold.dropRate;

        if (Random.Range(0, 101) < randomDropRate)
        {
            for (int i = 0; i < randomDropNum; i++)
            {
                // 골드 생성 (위치 설정 후 활성화)
                cachedPosition = transform.position;
                cachedPosition.x += Random.Range(MIN_CREATE_TRANS, MAX_CREATE_TRANS);
                cachedPosition.y += Random.Range(MIN_CREATE_TRANS, MAX_CREATE_TRANS);
                
                GameObject goldObj = PoolManager.instance.GetWithPosition(dropItemID, cachedPosition);
                if (goldObj != null)
                {
                    DropItem dropItemComponent = goldObj.GetComponent<DropItem>();
                    if (dropItemComponent != null)
                    {
                        dropItemComponent.GoldInit();
                    }
                }
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet") || !isLive)
            return;

        PlayerBullet  playerBullet  = collision.GetComponent<PlayerBullet>();
        SkillFunction skillFunction = collision.GetComponent<SkillFunction>();
        if (playerBullet)
        {
            // 보스 일반 데미지 로그
            if (enemyType == EnemyData.EnemyType.Boss)
            {
                Debug.Log($"보스 일반 데미지: {playerBullet.damage}, 체력: {currentHealth} -> {currentHealth - playerBullet.damage}");
            }
            
            if (isShieldBuff)
            {
                float remainDamage = currentShieldAmount - playerBullet.damage;
                if (remainDamage > 0) // 쉴드 남음
                {
                    currentShieldAmount -= playerBullet.damage;
                    //Debug.Log("쉴드 상태 히트. 남음.");
                }
                else
                {
                    currentHealth      += remainDamage; // 초과한 -값 더해주기
                    currentShieldAmount = 0;            // 남은 쉴드 없음
                    //Debug.Log("쉴드 상태 히트. 초과.");
                }
            }
            else
            {
                currentHealth -= playerBullet.damage;
                // Debug.Log("일반 히트");
            }

        }
        else if (skillFunction)
        {
            int isBossBoost;
            if (enemyType == EnemyData.EnemyType.Boss && skillFunction.isBossDamageBoost) 
                isBossBoost = 2;
            else 
                isBossBoost = 1;
                
            float finalDamage = skillFunction.damage * isBossBoost;
            
            // 보스 데미지 로그
            if (enemyType == EnemyData.EnemyType.Boss)
            {
                Debug.Log($"보스 스킬 데미지: {skillFunction.damage} x {isBossBoost} = {finalDamage}, 체력: {currentHealth} -> {currentHealth - finalDamage}");
            }
            
            if (isShieldBuff)
            {
                float remainDamage = currentShieldAmount - finalDamage;
                if (remainDamage > 0) // 쉴드 남음
                {
                    currentShieldAmount -= finalDamage;
                    //Debug.Log("쉴드 상태 히트. 남음.");
                }
                else
                {
                    currentHealth      += remainDamage; // 초과한 -값 더해주기
                    currentShieldAmount = 0;            // 남은 쉴드 없음
                    //Debug.Log("쉴드 상태 히트. 초과.");
                }
            }
            else
            {
                currentHealth -= finalDamage;
                //Debug.Log("일반 히트");
            }
            
            // 성능 최적화: 상태 플래그 사용
            if(!isAttacking)
                stunTimer = skillFunction.stunTime;
            //Debug.Log(skillFunction.damage + " / " + skillFunction.stunTime);
        }

        // 성능 최적화: 히트 쉐이더 값 설정
        hitShaderValue = 0.2f;
        //StartCoroutine(KnockBack());

        // 채력 남음.
        if (currentHealth > 0)
        {
            // 성능 최적화: 상태 플래그 사용
            if (!isAttacking && enemyType != EnemyData.EnemyType.Boss)
                anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.EnemyHit);
        }
        // 체력 없음.
        else
        {
            // 보스 사망 로그
            if (enemyType == EnemyData.EnemyType.Boss)
            {
                Debug.Log($"보스 사망: 체력 {currentHealth}/{maxHealth}");
            }
            
            isLive          = false;
            anim.speed      = 1; // 애니메이션 속도 조절
            cap2D.enabled   = false;
            rigid.simulated = false;
            spriteRen.sortingOrder = 1;
            anim.SetTrigger("Dead");
            GameManager.instance.currentKill++;

            // 드랍 아이템 생성
            if (enemyType != EnemyData.EnemyType.Boss)
            {
                ExpBeadCreate(); // EXP 100% 드랍
                GoldCreate();    // GOLD 확률 드랍 랜덤 반복
            }
            // 보스 골드 바로 더하기
            else
                GameManager.instance.currentGold += 5 * gold.minMaxDrop[1];
            
            // 버프 관리
            isEngageBuff    = false;
            engageBuffTimer = 0;
            isShieldBuff    = false;
            shieldBuffTimer = 0;
            stunTimer       = 0;
            foreach (var VARIABLE in effectList)
                VARIABLE.SetActive(false);

            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }
}