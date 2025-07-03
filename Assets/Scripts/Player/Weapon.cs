using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Weapon : MonoBehaviour
{
    // 스크립터블 오브젝트 데이터에 따라 초기화
    [HideInInspector] public int               weaponId;
    [HideInInspector] public ItemData.ItemType itemType;
    [HideInInspector] public int[]             bulletId;
    
    [HideInInspector] public float baseDamage;
    [HideInInspector] public float damageInPer;
    [HideInInspector] public float finalCalculationDamage;
    
    [HideInInspector] public float baseRate;
    [HideInInspector] public float rateInPer;
    [HideInInspector] public float finalCalculationRatePlus;
    [HideInInspector] public float finalCalculationRateMinus;
    
    [HideInInspector] public float baseRange;
    [HideInInspector] public float rangeInPer;
    
    [HideInInspector] public int baseCount;
    [HideInInspector] public int countIn;
    
    [HideInInspector] public int basePenetration;
    [HideInInspector] public int penetrationIn;
    
    // 전역
    private float     attackTimer;
    private Coroutine attackCo;
    
    // 스킬
    [HideInInspector] public SkillData skillData;   // 스킬 정보 받아와야 할
    [HideInInspector] public int       skillObjectId;     
    
    [HideInInspector] public int  explosiveArrowPer;       // 폭발화살 확률
    [HideInInspector] public bool shortenedLaunchInterval; // 속사 액티브 사용 시, 발사 간견 단축에 사용

    [HideInInspector] public int  skillCount;

    // range안 가까운적 찾기
    private int          enemyLayerMask;
    private Collider2D[] targets;        // 서클 안에 들어온, 모든 타겟(FixedUpdate 갱신)
    private Transform    nearestTarget;  // 제일 가까운 타겟
    
    // 조건 발사체 관리
    private List<BulletData> conditionalBulletList;
    private List<float>      conditionalBulletTimerList;
    [System.Serializable]
    public class BulletData
    {
        public Transform  bullet;
        public Vector3    originalPosition;
        public Quaternion originalRotation;
    }
    
    private void Awake()
    {
        damageInPer   = 100f;
        rateInPer     = 100f;
        rangeInPer    = 100f;
        countIn       = 0;
        penetrationIn = 0;
        
        int enemyLayer = 6;
        enemyLayerMask = 1 << enemyLayer;
        
        conditionalBulletList      = new List<BulletData>();
        conditionalBulletTimerList = new List<float>();
    }

    private void Update()
    {
        // 런타임 안전성 검사: GameManager 확인
        if (GameManager.instance == null || !GameManager.instance.isLive)
        {
            if (attackCo != null)
            {
                StopCoroutine(attackCo);
                attackCo = null;
            }
            return;
        }

        switch (weaponId) 
        {
            // 주먹 (Rate 감소율 적용 -> (baseRate * 100f / rateInPer))
            case 0:
                ReActivateBullet();
                break;
            // 활 (Rate 감소율 적용 -> (baseRate * 100f / rateInPer))
            case 1:
                attackTimer += Time.deltaTime;
                // 런타임 안전성 검사: nearestTarget이 여전히 유효한지 확인
                if (attackTimer > finalCalculationRateMinus && nearestTarget != null && nearestTarget.gameObject.activeInHierarchy)
                {
                    attackTimer = 0f;
                    attackCo = StartCoroutine(BowFire(nearestTarget));
                }
                break;  
            // 창 (Rate 감소율 적용 -> (baseRate * 100f / rateInPer))
            case 2:
                attackTimer += Time.deltaTime;
                if (attackTimer > finalCalculationRateMinus)
                {
                    attackTimer = 0f;
                    
                    SpearShoot();
                }
                break;
            // 칼 (Rate 증가율 적용 -> (baseRate * rateInPer / 100f))
            case 3:
                transform.Rotate(Vector3.back * (finalCalculationRatePlus * Time.deltaTime));
                break;
        }
    }
    
    private void FixedUpdate()
    {   
        // 런타임 안전성 검사: GameManager 확인
        if (GameManager.instance == null || !GameManager.instance.isLive)
            return;
            
        // 공통으로 공격을 관리하는 경우
        switch (weaponId) 
        {
            // 활 (자신 기준 거리 체크)
            case 1:
                targets       = Physics2D.OverlapCircleAll(transform.position,baseRange * rangeInPer / 100f,enemyLayerMask); // 전체 타겟 갱싱
                nearestTarget = GetNearest();                                                                                            // 가장 가까운 타겟 갱신
                break;
        }
    }
    
    public void Init(ItemData data)
    {
        // 안전성 검사: 기본 검증
        if (data == null)
        {
            Debug.LogError("Weapon: Init - ItemData가 null입니다!");
            return;
        }
        
        name                    = "Weapon " + data.name;     // 아이티
        transform.parent        = Player.instance.transform; // 인스펙터 위치
        transform.localPosition = Vector3.zero;              // 무기 위치
        weaponId                = data.itemId;               // 아이템 ID
        itemType                = data.itemType;             // 아이템 타입
        
        baseDamage      = data.baseDamage * CharacterPassive.AllDamage * CharacterPassive.MeleeDamage(itemType) * CharacterPassive.RangeDamage(itemType); 
        baseRate        = data.baseRate;
        baseCount       = data.baseCount + CharacterPassive.PowerUp9();
        if(data.basePenetration != -100) 
            basePenetration = data.basePenetration + CharacterPassive.PowerUp3();
        else
            basePenetration = data.basePenetration;
        baseRange = data.baseRange;
        
        // 안전성 검사: PoolManager와 배열들 확인
        if (PoolManager.instance == null || PoolManager.instance.prefabs == null)
        {
            Debug.LogError("Weapon: PoolManager가 초기화되지 않았습니다.");
            return;
        }
        
        if (data.bullet == null || data.bullet.Length == 0)
        {
            Debug.LogError($"Weapon: '{data.name}'의 bullet 배열이 비어있습니다.");
            return;
        }
        
        // 프리팹 아이디 받아오기(찾기 자동화)
        bulletId = new int[data.bullet.Length];
        for (int i = 0; i < bulletId.Length; i++)
        {
            bulletId[i] = -1; // 초기값을 -1로 설정 (찾지 못했음을 표시)
        }
        
        for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
        {
            if (PoolManager.instance.prefabs[index] == null) continue;
            
            for (int num = 0; num < data.bullet.Length; num++)
            {
                if (data.bullet[num] != null && data.bullet[num] == PoolManager.instance.prefabs[index])
                {
                    bulletId[num] = index;
                    break;
                }
            }
        }
        
        // bulletId 검증
        for (int i = 0; i < bulletId.Length; i++)
        {
            if (bulletId[i] == -1)
            {
                Debug.LogWarning($"Weapon: '{data.name}'의 bullet[{i}]를 PoolManager에서 찾을 수 없습니다.");
            }
        }
        
        CalculateAndDeployment();

        // Hand Set
        // Hand hand = Player.instance.hands[(int)data.itemType];  // 0번 1번 각각 자리 찾아가기
        // hand.spriteRen.sprite = data.hand;
        // hand.gameObject.SetActive(true);
    }
    
    public void WeaponLevelUp(float damageInPer,float rateInPer,  float rangeInPer,int countIn, int penetrationIn)
    {
        this.damageInPer   += damageInPer;
        this.rateInPer     += rateInPer;  
        this.rangeInPer    += rangeInPer;   
        this.countIn       += countIn;      
        this.penetrationIn += penetrationIn;
        
        CalculateAndDeployment();
        
        // Hand Set
        // Hand hand = Player.instance.hands[(int)data.itemType];  // 0번 1번 각각 자리 찾아가기
        // hand.spriteRen.sprite = data.hand;
        // hand.gameObject.SetActive(true);
    }
    
    // 엑세 or 스킬 적용
    public void AccessoriesAndSkillToWeaponApply()
    {
        CalculateAndDeployment();

        // Hand Set
        // Hand hand = Player.instance.hands[(int)data.itemType];  // 0번 1번 각각 자리 찾아가기
        // hand.spriteRen.sprite = data.hand;
        // hand.gameObject.SetActive(true);
    }
    
    // 최종값을 계산하고, 세팅된 최종값 + 초기화된 값들로 배치 및 수치값 다시 세팅해주기
    private void CalculateAndDeployment()
    {
        finalCalculationDamage    = baseDamage * (damageInPer / 100f     ) * (GameManager.instance.totalAttackDamageInPer / 100f);
        finalCalculationRatePlus  = baseRate   * (rateInPer   / 100f     ) * (GameManager.instance.totalAttackRateInPer   / 100f);
        finalCalculationRateMinus = baseRate   * (100f        / rateInPer) * (100f                                        / GameManager.instance.totalAttackRateInPer);
        
        if (attackCo != null)
            StopCoroutine(attackCo);
        
        // 주먹 배치
        if (weaponId == 0)
            ArmDeployment();
        // 칼 배치
        else if (weaponId == 3)
            SwordDeployment();
    }

    // 풀 시스템 사용 X(재활용 필요가 없음.)
    private void ArmDeployment()
    {   
        // 안전성 검사
        if (bulletId == null || bulletId.Length == 0 || bulletId[0] < 0)
        {
            Debug.LogError("Weapon: ArmDeployment - bulletId가 유효하지 않습니다.");
            return;
        }
        
        if (PoolManager.instance == null || PoolManager.instance.prefabs == null)
        {
            Debug.LogError("Weapon: ArmDeployment - PoolManager가 초기화되지 않았습니다.");
            return;
        }
        
        if (bulletId[0] >= PoolManager.instance.prefabs.Length)
        {
            Debug.LogError($"Weapon: ArmDeployment - bulletId[0]({bulletId[0]})이 prefabs 배열 범위를 벗어났습니다.");
            return;
        }
        
        if (PoolManager.instance.prefabs[bulletId[0]] == null)
        {
            Debug.LogError($"Weapon: ArmDeployment - prefabs[{bulletId[0]}]이 null입니다.");
            return;
        }
        
        int forCount = baseCount + countIn + skillCount;
        
        // 조건부 총알 리스트와 타이머 리스트 초기화
        conditionalBulletList.Clear();
        conditionalBulletTimerList.Clear();
        
        for (int index = 0; index < forCount; index++)
        {
            Transform bullet = null;

            // 기존 총알 재사용
            if (index < transform.childCount)
            {
                bullet = transform.GetChild(index);
                bullet.gameObject.SetActive(true);
            }
            // 새 총알 생성(풀 X)
            else if (index >= transform.childCount)
            {
                //bullet = PoolManager.instance.Get(prefabId).transform;
                GameObject newBullet = Instantiate(PoolManager.instance.prefabs[bulletId[0]],transform);
                bullet               = newBullet.transform;
                bullet.parent        = transform;  // 부모를 현재 객체로 설정
                //Debug.Log($"{index}번 새 총알 생성: {bullet.name}");
            }
            bullet.name += index;
            
            // 위치 및 회전 초기화
            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            // 회전 및 위치 설정
            Vector3 rotVec = Vector3.forward * (360 * index) / forCount;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up, Space.World);
            
            // 수치 초기화
            PlayerBullet bulletComponent = bullet.GetComponent<PlayerBullet>();
            if (bulletComponent != null)
            {
                // 주먹의 range를 화면 크기에 맞게 제한 (최대 5 유닛으로 제한)
                float adjustedRange = Mathf.Min(baseRange * rangeInPer / 100f, 5f);
                bulletComponent.Init(finalCalculationDamage, basePenetration + penetrationIn, adjustedRange, Vector3.forward,false,0);
            }
            
            // 총알 데이터 생성
            BulletData bulletData = new BulletData
            {
                bullet           = bullet,
                originalPosition = bullet.localPosition,
                originalRotation = bullet.localRotation
            };
            
            // 조건부 총알 리스트 및 타이머 리스트에 추가
            conditionalBulletList.Add(bulletData);
            conditionalBulletTimerList.Add(0);
        }
        
        // 사용되지 않는 총알은 비활성화
        for (int index = forCount; index < transform.childCount; index++)
            transform.GetChild(index).gameObject.SetActive(false);
    }
    
    private void ReActivateBullet()     
    {
        for (int i = 0; i < conditionalBulletList.Count; i++)
        {
            // 조건 발사체가 활성화 되있으면, continue
            if (conditionalBulletList[i].bullet.gameObject.activeSelf)
            {
                conditionalBulletTimerList[i] = 0f;
                continue;
            }
            
            // 조건 발사체가 비활성화 되어 있으면, 시간을 체크하여, 다시 활성화
            conditionalBulletTimerList[i] += Time.deltaTime;
            if (conditionalBulletTimerList[i] > baseRate * 100f / rateInPer)
            {
                conditionalBulletList[i].bullet.localPosition = conditionalBulletList[i].originalPosition; // 원래 위치로 이동
                conditionalBulletList[i].bullet.localRotation = conditionalBulletList[i].originalRotation; // 원래 회전으로 복원
                conditionalBulletList[i].bullet.gameObject.SetActive(true);                                // 다시 활성화
            }
        }
    }
    
    // 풀 시스템 사용
    private IEnumerator BowFire(Transform shootTrans)
    {   
        // 런타임 안전성 검사: 발사 전 타겟 재확인
        if (shootTrans == null || !shootTrans.gameObject.activeInHierarchy)
        {
            Debug.Log("Weapon: BowFire - 타겟이 사라져서 발사를 중단합니다.");
            attackCo = null;
            yield break;
        }
        
        // 런타임 안전성 검사: PoolManager 확인
        if (PoolManager.instance == null)
        {
            Debug.Log("Weapon: BowFire - PoolManager가 null입니다.");
            attackCo = null;
            yield break;
        }
        
        int      forCount = baseCount + countIn + skillCount;
        Vector3  targetPos;
        Vector3  dir;
        int      randomRange;
        
        for (int index = 0; index < forCount; index++)
        {
            // 런타임 안전성 검사: 루프 중에도 타겟이 유효한지 확인
            if (shootTrans == null || !shootTrans.gameObject.activeInHierarchy)
            {
                Debug.Log("Weapon: BowFire - 발사 중 타겟이 사라졌습니다.");
                break;
            }
            
            targetPos   = shootTrans.position;
            dir         = targetPos - transform.position;
            dir         = dir.normalized;
            // 랜덤 방향값 적용
            dir.x += Random.Range(-0.1f, 0.1f);
            dir.y += Random.Range(-0.1f, 0.1f);
            
            randomRange = Random.Range(0, 100);
            
            GameObject bulletObj = null;
            Transform  bullet    = null;
            
            // 일반 화살
            if (randomRange >= explosiveArrowPer)
            {
                // 런타임 안전성 검사: bulletId 유효성 확인
                if (bulletId != null && bulletId.Length > 0 && bulletId[0] >= 0)
                {
                    bulletObj = PoolManager.instance.GetWithPosition(bulletId[0], transform.position);
                    if (bulletObj != null)
                    {
                        bullet = bulletObj.transform;
                        PlayerBullet bulletComponent = bullet.GetComponent<PlayerBullet>();
                        if (bulletComponent != null)
                        {
                            bulletComponent.Init(finalCalculationDamage, basePenetration + penetrationIn, 0 ,dir,true,0f); // 원거리 무기의 Count는 관통 횟수
                        }
                    }
                }
            }
            // 폭발 화살
            else
            {
                if (skillObjectId >= 0)
                {
                    bulletObj = PoolManager.instance.GetWithPosition(skillObjectId, transform.position);
                    if (bulletObj != null)
                    {
                        bullet = bulletObj.transform;
                        PlayerBullet bulletComponent = bullet.GetComponent<PlayerBullet>();
                        if (bulletComponent != null && skillData != null && skillData.skillRatePer != null && skillData.skillRatePer.Count >= 2)
                        {
                            bulletComponent.Init(finalCalculationDamage * (skillData.skillRatePer[0] / 100), 1, 0 ,dir,true,finalCalculationDamage * (skillData.skillRatePer[1] / 100)); 
                        }
                    }
                }
            }
            
            if (bullet != null)
            {
                bullet.rotation  = Quaternion.FromToRotation(Vector3.up, dir);
            }
            
            yield return new WaitForSeconds(0.05f);
        }
        
        attackCo = null; // 코루틴 완료 표시
    }
    
    // 풀 시스템 사용
    private void SpearShoot()
    {
        // 런타임 안전성 검사: PoolManager 확인
        if (PoolManager.instance == null)
        {
            Debug.Log("Weapon: SpearShoot - PoolManager가 null입니다.");
            return;
        }
        
        // 런타임 안전성 검사: bulletId 유효성 확인
        if (bulletId == null || bulletId.Length == 0 || bulletId[0] < 0)
        {
            Debug.Log("Weapon: SpearShoot - bulletId가 유효하지 않습니다.");
            return;
        }
        
        int forCount = baseCount + countIn + skillCount;
        
        for (int index = 0; index < forCount; index++)
        {
            // 360도를 갯수로 나누어 각도 계산 (90도 오프셋 추가)
            float angle = (360f * index / forCount) + 90f;
            float radian = angle * Mathf.Deg2Rad;
            
            // 방향 벡터 계산
            Vector3 dir = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0f).normalized;
            
            // PoolManager에서 창 총알 가져오기
            GameObject bulletObj = PoolManager.instance.GetWithPosition(bulletId[0], transform.position);
            if (bulletObj != null)
            {
                Transform bullet = bulletObj.transform;
                PlayerBullet bulletComponent = bullet.GetComponent<PlayerBullet>();
                
                if (bulletComponent != null)
                {
                    // 창 총알 초기화 (원거리 무기처럼 날아가는 형태로)
                    bulletComponent.Init(finalCalculationDamage, basePenetration + penetrationIn, 0, dir, true, 0f);
                }
                
                // 회전 설정
                bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
            }
        }
    }
    
    // 풀 시스템 사용 X(재활용 필요가 없음.)
    
    private void SwordDeployment()
    {
        int forCount = baseCount + countIn + skillCount;
        
        for (int index = 0; index < forCount; index++) 
        {
            Transform bullet = null;

            // 기존 총알 재사용
            if (index < transform.childCount)
            {
                bullet = transform.GetChild(index);
                bullet.gameObject.SetActive(true);
            }
            // 새 총알 생성(풀 X)
            else if (index >= transform.childCount)
            {
                GameObject newBullet = Instantiate(PoolManager.instance.prefabs[bulletId[0]],transform);
                bullet               = newBullet.transform;
                bullet.parent        = transform;               // 부모를 현재 객체로 설정
            }
            bullet.name += index;
            
            // 위치 및 회전 초기화
            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;
            
            // 회전 초기화
            Vector3 rotVec = Vector3.forward * (360 * index) / forCount;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * (baseRange * rangeInPer / 100f), Space.World); // 생성 위치 변경(Range에 따라 회전 범위 증가)
            
            // 수치 초기화
            bullet.GetComponent<PlayerBullet>().Init(finalCalculationDamage, -100, 0 ,Vector3.zero,false,0); // -100 is Infinity 관통력(근접 무기는 Count를 관통력으로 사용하지 않음.)
        }
        
        // 사용되지 않는 총알은 비활성화
        for (int index = forCount; index < transform.childCount; index++)
            transform.GetChild(index).gameObject.SetActive(false);
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
                Debug.Log("Weapon: GetNearest - 죽은 적을 타겟에서 제외합니다.");
                continue;
            }
            
            // 런타임 안전성 검사: Transform 컴포넌트 null 체크
            if (target.transform == null)
            {
                Debug.Log("Weapon: GetNearest - 타겟의 Transform이 null입니다.");
                continue;
            }
            
            // 화면에 보이지 않는 적 제외
            if (!IsTargetVisibleInWeapon(target.gameObject))
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
    /// 타겟이 화면에 보이는지 확인하는 메서드 (Weapon용)
    /// </summary>
    private bool IsTargetVisibleInWeapon(GameObject target)
    {
        // 런타임 안전성 검사: Camera.main null 체크
        if (Camera.main == null)
        {
            Debug.LogError("Weapon: IsTargetVisibleInWeapon - Camera.main이 null입니다.");
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
}