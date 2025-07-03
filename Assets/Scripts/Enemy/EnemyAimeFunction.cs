using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAimeFunction : MonoBehaviour
{
    private Enemy enemy;
    
    [HideInInspector] public Transform chargingToEffectPos; // 차징 이펙트 위치와 슬레쉬 등등 이펙트의 위치에 사용
    
    // 경계선
    public  GameObject[]     alterLine;
    private int[]            alterLineID;
    private List<GameObject> createAlterLines;
    
    private bool isAttackMove = false;
    
    private bool isRushMove   = false;
    private int  originCollisionDamage;
    
    [HideInInspector] public Vector3 fixedFireDirectionVec3; // 고정된, 플레이어를 바라보는 방향
    [HideInInspector] public Vector3 fixedPlayerVec3;        // 고정된, 플레이어 위치
    
    [HideInInspector] public bool isCharging = false;
    
    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        
        chargingToEffectPos = transform.GetChild(3);
        
        // 경고선 아이디 받아오기(풀 X, DATA X)
        createAlterLines = new List<GameObject>();                       // 초기화
        alterLineID      = new int[PoolManager.instance.prefabs.Length]; // 길이만큼 만들어 주기
        for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
        {
            for (int num = 0; num < alterLine.Length; num++)
            {
                if (alterLine[num] == PoolManager.instance.prefabs[index])
                {
                    alterLineID[num] = index;
                    break;
                }
            }
        }
    }

    private void OnEnable()
    {
        isAttackMove = false;
        isRushMove   = false;
        isCharging   = false;
    }

    private void FixedUpdate()
    {
        if(!enemy.isLive)
            return;
        
        // 공격 이동
        if (isAttackMove)
        {
            if (enemy.gameObject.transform.localScale.x == 1)
                enemy.rigid.MovePosition(enemy.rigid.position + (Vector2.right * enemy.attackSpeed[enemy.AnimAttackNum()]/100f));
            else
                enemy.rigid.MovePosition(enemy.rigid.position + (Vector2.left * enemy.attackSpeed[enemy.AnimAttackNum()]/100f));
        }
        
        // 스킬 이동
        if (isRushMove)
        {
            // 돌진 이동(고정된 위치값으로)
            Vector2 nextVec = fixedFireDirectionVec3 * (enemy.attackSpeed[enemy.AnimAttackNum()]/100f);
            enemy.rigid.MovePosition(enemy.rigid.position + nextVec);
        }
    }

    private void FixedFirePosition()
    {
        var position = Player.instance.transform.position;

        // 고정된 바라보는 방향
        Vector3 targetPos = position;
        Vector3 dir       = targetPos - transform.position;

        // 고정된 방향
        fixedFireDirectionVec3 = dir.normalized;
        // 고정된 위치
        fixedPlayerVec3 = position;
    }
    
    private void MeleeDamage() // 근접 공격
    {
        // 런타임 안전성 검사: AudioManager 확인
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Slash);
    
        // 슬레쉬 이펙트가 있으면, 생성(0번은 적 프리팹)
        if (enemy.attackEffectID[enemy.AnimAttackNum()] != 0)
        {
            // 런타임 안전성 검사: PoolManager 확인
            if (PoolManager.instance != null)
            {
                GameObject meleeEffectObj = PoolManager.instance.GetWithPosition(enemy.attackEffectID[enemy.AnimAttackNum()], chargingToEffectPos.position);
                if (meleeEffectObj != null)
                {
                    Transform meleeEffectPrefabs = meleeEffectObj.transform;
                    
                    Vector3 newScale = meleeEffectPrefabs.localScale;
                    if (enemy.gameObject.transform.localScale.x == -1)
                        newScale.x = -Mathf.Abs(newScale.x);
                    else
                        newScale.x = Mathf.Abs(newScale.x);
                    meleeEffectPrefabs.localScale = newScale;
                }
            }
        }
        
        // 범위 안에 플레이어가 있으면, 데미지를 주면 됨.
        if (enemy.enemyAttackRange[0].IsPlayerInRange())
        {
            // 런타임 안전성 검사: Player.instance 확인
            if (Player.instance != null)
            {
                if(enemy.isEngageBuff)
                    Player.instance.Hit(enemy.attackDamage[enemy.AnimAttackNum()] * enemy.engageBuffValue[1]/100);
                else
                    Player.instance.Hit(enemy.attackDamage[enemy.AnimAttackNum()]);
            }
        }
    }

    // 원거리 공격 홀드를 시작할 때, fixedFireDirectionVec3 위치에 쏘도록 함.
    private void Fire() // 화살, 화염구 일직선으로 날리는 투사체
    {
        // 런타임 안전성 검사: AudioManager 확인
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    
        // 런타임 안전성 검사: PoolManager 확인
        if (PoolManager.instance != null)
        {
            GameObject bulletObj = PoolManager.instance.GetWithPosition(enemy.attackEffectID[enemy.AnimAttackNum()], transform.position);
            if (bulletObj != null)
            {
                Transform bullet = bulletObj.transform;
                bullet.rotation  = Quaternion.FromToRotation(Vector3.up, fixedFireDirectionVec3);
                
                EnemyBullet enemyBulletComponent = bullet.GetComponent<EnemyBullet>();
                if (enemyBulletComponent != null)
                {
                    if(enemy.isEngageBuff)
                        enemyBulletComponent.Init(enemy.attackDamage[enemy.AnimAttackNum()] * enemy.engageBuffValue[1]/100, fixedFireDirectionVec3);
                    else
                        enemyBulletComponent.Init(enemy.attackDamage[enemy.AnimAttackNum()], fixedFireDirectionVec3);
                }
            }
        }
    }
    
    private void FloorWarning() // 공통 마법 장판 or 낙하 등등
    {
        // 런타임 안전성 검사: PoolManager 확인
        if (PoolManager.instance != null)
        {
            GameObject floorWarningObj = PoolManager.instance.GetWithPosition(enemy.waringEffectID[enemy.AnimAttackNum()], fixedPlayerVec3);
            if (floorWarningObj != null)
            {
                Transform floorWarningPrefabs = floorWarningObj.transform;
            }
        }
    }
    
    private void Nova() // 공통 마법사 폭발 or 엘리트전사 낙하 등등 순간 터지는 공격
    {
        // 런타임 안전성 검사: AudioManager 확인
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Landing); // 엘리트전사 낙하는 해당 소리만 남.

        // 런타임 안전성 검사: PoolManager 확인
        if (PoolManager.instance != null)
        {
            GameObject novaObj = PoolManager.instance.GetWithPosition(enemy.attackEffectID[enemy.AnimAttackNum()], fixedPlayerVec3);
            if (novaObj != null)
            {
                Transform novaPrefabs = novaObj.transform;
                
                ExplosionCheck explosionComponent = novaPrefabs.GetComponent<ExplosionCheck>();
                if (explosionComponent != null)
                {
                    // 범위 안에 플레이어가 있는지 체크(버스 상태 따라서, 데미지 다르게 적용)
                    if(enemy.isEngageBuff)
                        explosionComponent.Check(enemy.attackDamage[enemy.AnimAttackNum()] * enemy.engageBuffValue[1]/100);
                    else
                        explosionComponent.Check(enemy.attackDamage[enemy.AnimAttackNum()]);
                }
            }
        }
    }

    private void UpSky() // 엘리트 하늘 올라가기 or
    {
       enemy.rigid.bodyType = RigidbodyType2D.Kinematic;
       enemy.cap2D.enabled  = false;
    }
    
    private void DownSky() // 엘리트 내려오기 or
    {
        enemy.rigid.bodyType = RigidbodyType2D.Dynamic;
        enemy.cap2D.enabled  = true;
    }

    private void MovePlayerPosition()
    {
        transform.parent.position = fixedPlayerVec3;    // 고정된 위치로 이동
    }
    
    private void RangeConditionCheck()
    {
        // 런타임 안전성 검사: Player.instance 확인
        if (Player.instance == null || Player.instance.transform == null)
        {
            Debug.Log("EnemyAimeFunction: RangeConditionCheck - Player.instance가 null입니다.");
            return;
        }
        
        // 준비 동작에서, 조건을 체크하고, 조건에 불만족 하면, 애니메이션 종료
        if(enemy.attackRange < Vector2.Distance(transform.position,Player.instance.transform.position))
            enemy.anim.SetTrigger("Quit");
    }

    private void AttackMoveStart()
    {
        isAttackMove = true;
    }

    private void AttackMoveStop()
    {
        isAttackMove = false;
    }
    
    private void RushMoveStart()
    {
        isRushMove = true;
        originCollisionDamage = enemy.collisionDamage; 
        enemy.collisionDamage = (int)enemy.attackDamage[enemy.AnimAttackNum()]; // 충돌 데미지 변경
    }

    private void RushMoveStop()
    {
        isRushMove = false;
        enemy.collisionDamage = originCollisionDamage;
    }

    private void ChargingStart()
    {
        isCharging = true;
    }
    
    private void ChargingStop()
    {
        isCharging = false;
    }

    private void EnrageBuff()   // 광폭화
    {
        // 런타임 안전성 검사: AudioManager 확인
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Engage);
            
        // OverlapCircle로 Enemy 찾기(전체 다)
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, 100f, LayerMask.GetMask("Enemy"));

        if (hit != null)
        {
            foreach (var hits in hit)
            {
                // 런타임 안전성 검사: hits null 체크
                if (hits == null || !hits.gameObject.activeInHierarchy)
                    continue;
                    
                EnemyAimeFunction hitEnemyAimeFunction = hits.GetComponentInChildren<EnemyAimeFunction>();
                if(hitEnemyAimeFunction != null && hitEnemyAimeFunction.enemy != null && hitEnemyAimeFunction.enemy.isLive)
                    hitEnemyAimeFunction.EnrageSetting(enemy.engageBuffValue);
            }
        }
    }
    
    private void EnrageSetting(float[] enrageBuffValue)
    {
        enemy.engageBuffValue = enrageBuffValue;
        enemy.engageBuffTimer = enemy.engageBuffValue[0]/100;
    }

    private void EarthSplitting() // 대지 가르기
    {
        // 런타임 안전성 검사: AudioManager 확인
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Slash);
    
        // 런타임 안전성 검사: PoolManager 확인
        if (PoolManager.instance != null)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject bulletObj = PoolManager.instance.GetWithPosition(enemy.attackEffectID[enemy.AnimAttackNum()], transform.position);
                if (bulletObj != null)
                {
                    Transform bullet = bulletObj.transform;

                    // 기준 방향에서 Z축 회전 오프셋 설정 (-20, 0, 20도)
                    float angleOffset = -20 + (20 * i); // -20, 0, 20
                    Quaternion offsetRotation = Quaternion.Euler(0f, 0f, angleOffset);

                    // 기존 방향에 오프셋 회전을 더하여 총알 회전 설정(FromToRotation에 곱해주면 됨!)
                    bullet.rotation = Quaternion.FromToRotation(Vector3.up, fixedFireDirectionVec3) * offsetRotation;
                    
                    EnemyBullet enemyBulletComponent = bullet.GetComponent<EnemyBullet>();
                    if (enemyBulletComponent != null)
                    {
                        if(enemy.isEngageBuff)
                            enemyBulletComponent.Init(enemy.attackDamage[enemy.AnimAttackNum()] * enemy.engageBuffValue[1]/100, Vector3.zero, false);
                        else
                            enemyBulletComponent.Init(enemy.attackDamage[enemy.AnimAttackNum()], Vector3.zero, false);
                    }
                }
            }
        }
    }
    
    private void ShieldBuff()   // 쉴드 버프
    {
        // 런타임 안전성 검사: AudioManager 확인
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Engage);
            
        // OverlapCircle로 Enemy 찾기(전체 다)
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, 100f, LayerMask.GetMask("Enemy"));

        if (hit != null)
        {
            foreach (var hits in hit)
            {
                // 런타임 안전성 검사: hits null 체크
                if (hits == null || !hits.gameObject.activeInHierarchy)
                    continue;
                    
                EnemyAimeFunction hitEnemyAimeFunction = hits.GetComponentInChildren<EnemyAimeFunction>();
                if(hitEnemyAimeFunction != null && hitEnemyAimeFunction.enemy != null && hitEnemyAimeFunction.enemy.isLive)
                    hitEnemyAimeFunction.ShieldSetting(enemy.shieldBuffValue);
            }
        }
    }   
    
    private void ShieldSetting(float[] shieldValue)
    {
        enemy.shieldBuffTimer     = shieldValue[0]/100; // 타이머 초기화
        enemy.currentShieldAmount = shieldValue[1];     // 쉴드량 초기화
    }
    
    private void Slash()
    {
        // 런타임 안전성 검사: AudioManager 확인
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Slash);
    
        // 런타임 안전성 검사: PoolManager 확인
        if (PoolManager.instance != null)
        {
            GameObject bulletObj = PoolManager.instance.GetWithPosition(enemy.attackEffectID[enemy.AnimAttackNum()], transform.position);
            if (bulletObj != null)
            {
                Transform bullet = bulletObj.transform;
                bullet.rotation  = Quaternion.FromToRotation(Vector3.up, fixedFireDirectionVec3);
                
                EnemyBullet enemyBulletComponent = bullet.GetComponent<EnemyBullet>();
                if (enemyBulletComponent != null)
                {
                    if(enemy.isEngageBuff)
                        bullet.GetComponent<EnemyBullet>().Init(enemy.attackDamage[enemy.AnimAttackNum()] * enemy.engageBuffValue[1]/100, fixedFireDirectionVec3,false);
                    else
                        bullet.GetComponent<EnemyBullet>().Init(enemy.attackDamage[enemy.AnimAttackNum()], fixedFireDirectionVec3,false);
                }
                
            }
        }
    }
    
    private void FlowerOfDeath()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.FireAttack); // 10갈래 화염어택 점프하고 내려오는 사운드
    
        for (int index = 0; index < 10; index++) // 10발 발사
        {
            Transform bullet = PoolManager.instance.GetWithPosition(enemy.attackEffectID[enemy.AnimAttackNum()], transform.position).transform;
            
            // 360도를 6개로 나누어 각도를 계산
            float angle  = 360f / 10 * index;
            float radian = angle * Mathf.Deg2Rad; // 각도를 라디안으로 변환

            // 방향 계산 (자신 기준으로 360도)
            Vector3 dir = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0).normalized;
            bullet.rotation  = Quaternion.FromToRotation(Vector3.up, dir);
            
            if(enemy.isEngageBuff)
                bullet.GetComponent<EnemyBullet>().Init(enemy.attackDamage[enemy.AnimAttackNum()] * enemy.engageBuffValue[1]/100, Vector3.zero, false);
            else
                bullet.GetComponent<EnemyBullet>().Init(enemy.attackDamage[enemy.AnimAttackNum()], Vector3.zero, false);
        }
    }
    
    private void FireWall()
    {
        Transform fireWallClone = PoolManager.instance.GetWithPosition(enemy.attackEffectID[enemy.AnimAttackNum()], fixedPlayerVec3).transform;
        
        Vector3 targetPos = Player.instance.transform.position;
        Vector3 dir       = targetPos - transform.position;
        dir               = dir.normalized;
        
        fireWallClone.rotation  = Quaternion.FromToRotation(Vector3.up, dir);

        fireWallClone.GetComponent<Pitfall>().Init(enemy.attackDamage[enemy.AnimAttackNum()]);
    }
    
    public void EarthSplittingAlterLine()  // 17 - 0.5 - fixedFireDirectionVec3
    {
        for (int i = 0; i < 3; i++)
        {
            // 기준 방향에서 Z축 회전 오프셋 설정 (-20, 0, 20도)
            float angleOffset = -20 + (20 * i); // -20, 0, 20
            Quaternion offsetRotation = Quaternion.Euler(0f, 0f, angleOffset);
            
            Transform alterLineClone = PoolManager.instance.GetWithPosition(alterLineID[0], transform.position).transform;
            alterLineClone.rotation = Quaternion.FromToRotation(Vector3.up, fixedFireDirectionVec3) * offsetRotation;
            createAlterLines.Add(alterLineClone.gameObject);
        }
    }

    public void SlashAlterLine()   // Infi - 2 - fixedFireDirectionVec3
    {
        Transform alterLineClone = PoolManager.instance.GetWithPosition(alterLineID[3], transform.position).transform;
        alterLineClone.rotation  = Quaternion.FromToRotation(Vector3.up, fixedFireDirectionVec3);
        createAlterLines.Add(alterLineClone.gameObject);
    }

    public void FlowerOfDeathAlterLine() // 17 - 1 - dir
    {
        for (int index = 0; index < 10; index++) // 10발 발사
        {
            // 360도를 6개로 나누어 각도를 계산
            float angle = 360f / 10 * index;
            float radian = angle * Mathf.Deg2Rad; // 각도를 라디안으로 변환

            // 방향 계산 (자신 기준으로 360도)
            Vector3 dir = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0).normalized;
            Transform alterLineClone = PoolManager.instance.GetWithPosition(alterLineID[1], transform.position).transform;
            alterLineClone.rotation  = Quaternion.FromToRotation(Vector3.up, dir);
            createAlterLines.Add(alterLineClone.gameObject);
        }
    }

    public void ProjectileAlterLine() // Infi - 0.5 - fixedFireDirectionVec3
    {
        Transform alterLineClone = PoolManager.instance.GetWithPosition(alterLineID[2], transform.position).transform;
        alterLineClone.rotation  = Quaternion.FromToRotation(Vector3.up, fixedFireDirectionVec3);
        createAlterLines.Add(alterLineClone.gameObject);
    }

    public void RushAlterLine() // 5 - 1 - fixedFireDirectionVec3
    {
        Transform alterLineClone = PoolManager.instance.GetWithPosition(alterLineID[4], transform.position).transform;
        alterLineClone.rotation  = Quaternion.FromToRotation(Vector3.up, fixedFireDirectionVec3);
        createAlterLines.Add(alterLineClone.gameObject);
    }

    public void AllAlterLineSetActiveFalse()
    {
        foreach (var VARIABLE in createAlterLines)
            VARIABLE.SetActive(false);
    }
    
    public void Dead()
    {
        AllAlterLineSetActiveFalse();
        enemy.gameObject.SetActive(false); // 오브젝트 풀링으로 재사용 하기 때문에, 삭제 X
    }
    
    public void IsBossDie()
    {
        SpawnManager.instance.isBossDie = true;
    }
}