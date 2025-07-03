using UnityEngine;

public class DropItem : MonoBehaviour
{
    // 공통
    private SpriteRenderer spriteRenderer;
    private Animator       animator;
    
    private enum DropType {EXP, Gold}
    private DropType itemDropType;

    [HideInInspector] public bool isFollow; 
    public float followSpeed = 5f;
    
    // 경험치 구술
    public float     mergeCheckRadius;
    public LayerMask interactionLayer;
    
    private Vector3 baseSize = new Vector3(1f, 1f, 1f);
    
    [HideInInspector] public int minEXP;
    [HideInInspector] public int maxEXP;

    // 골드

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator       = GetComponent<Animator>();
    }

    private void Update()
    {
        // 런타임 안전성 검사: Player.instance null 체크
        if (isFollow)
        {
            if (Player.instance == null || Player.instance.transform == null)
            {
                Debug.Log("DropItem: Player.instance가 null입니다. 따라가기를 중단합니다.");
                isFollow = false;
                return;
            }
            FollowPlayer();
        }
    }
    
    // 공통
    private void OnEnable()
    {
        isFollow = false;
        
        // 오브젝트 풀링 재활용 시 완전 초기화
        transform.localScale = baseSize;
        
        // 위치 초기화 (충돌 방지를 위해 원점으로 설정 - 이후 정확한 위치로 설정됨)
        if (transform.position.magnitude > 100f) // 너무 멀리 있는 경우에만 초기화
        {
            transform.position = Vector3.zero;
        }
        
        // 경험치/골드 값 초기화
        minEXP = 0;
        maxEXP = 0;
        
        // 애니메이션 초기화 (기본 상태로)
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
    
    // 경험치 구술 초기화
    public void EXP_BeadInit(int minEXP, int maxEXP)
    {
        itemDropType = DropType.EXP;

        this.minEXP = minEXP;
        this.maxEXP = maxEXP;

        animator.Play("Exp");
        
        // 주변 ExpBead 탐색 및 병합
        MergeNearbyExpBeads();

        // 경험치에 따라 크기 조절
        float sizeMultiplier = GetSizeMultiplier(this.maxEXP);
        transform.localScale = baseSize * sizeMultiplier;
    }

    private void MergeNearbyExpBeads()
    {
        // mergeCheckRadius 범위 안의 모든 콜라이더 찾기
        Collider2D[] nearbyBeads = Physics2D.OverlapCircleAll(transform.position, mergeCheckRadius, interactionLayer);
        
        foreach (var beadCollider in nearbyBeads)
        {
            // 자신은 제외
            if (beadCollider.gameObject == gameObject) 
                continue;

            // 런타임 안전성 검사: null 체크 추가
            if (beadCollider == null || !beadCollider.gameObject.activeInHierarchy)
                continue;

            // ExpBead 스크립트를 가진 오브젝트인지 확인(+ 따라가는 중 아님)
            DropItem otherBead = beadCollider.GetComponent<DropItem>();
            if (otherBead != null & !otherBead.isFollow)
            {
                // 다른 오브젝트의 minEXP와 maxEXP 값을 현재 오브젝트에 합산
                minEXP += otherBead.minEXP;
                maxEXP += otherBead.maxEXP;

                // 다른 오브젝트 비활성화
                otherBead.gameObject.SetActive(false);
            }
        }
    }

    private float GetSizeMultiplier(int exp)
    {
        if (exp >= 1    && exp <= 9)    return 1.0f;
        if (exp >= 10   && exp <= 49)   return 1.1f;
        if (exp >= 50   && exp <= 99)   return 1.2f;
        if (exp >= 100  && exp <= 199)  return 1.3f;
        if (exp >= 200  && exp <= 499)  return 1.4f;
        if (exp >= 500  && exp <= 999)  return 1.5f;
        if (exp >= 1000 && exp <= 1999) return 1.6f;
        if (exp >= 2000 && exp <= 4999) return 1.7f;
        //if (exp >= 5000 && exp <= 9999) return 1.8f;
        if (exp >= 5000) return 1.8f;
        
        return 1.0f;
    }
    
    public void GoldInit()
    {
        itemDropType = DropType.Gold;
        
        animator.Play("Gold");
        
        transform.localScale = baseSize;
    }
    
    // 공통
    private void FollowPlayer()
    {
        // 런타임 안전성 검사: Player.instance 재확인 (Update에서 한번 체크했지만 안전을 위해)
        if (Player.instance == null || Player.instance.transform == null)
        {
            isFollow = false;
            return;
        }
        
        // 플레이어 위치로 부드럽게 이동
        Vector3 targetPosition = Player.instance.transform.position;
        transform.position     = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            // 런타임 안전성 검사: 매니저들 null 체크
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Acquisition);
        
            // 경험치 구술
            if(itemDropType == DropType.EXP)
            {
                if (GameManager.instance != null)
                    GameManager.instance.GetExp(minEXP, maxEXP);
            }
            // 골드
            else if(itemDropType == DropType.Gold)
            {
                if (GameManager.instance != null)
                    GameManager.instance.currentGold += 5;
            }
            
            gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, mergeCheckRadius);
    }
}