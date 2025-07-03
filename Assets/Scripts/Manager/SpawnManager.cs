using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public float       waveDuration;     // 웨이브 지속 시간
    public float       creationInterval; // 생성 간격
    public EnemyRate[] enemyRate;        // 적 그룹 + 비율
}

[System.Serializable]
public class EnemyRate
{
    public EnemyData[] spawnEnemyData; // 적 스폰 데이터
    public int         creationRate;   // 생성 비율
}

[System.Serializable]
public class Elite
{
    [HideInInspector] public bool      isCreation = false; // 생성 상태(생성한 후, true로 전환)
    public int        spawnTime;      // 등장 시간
    public EnemyData  spawnEnemyData; // 등장 적
    public int        spawnNum;       // 등장 몬수터 수
}

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    private Transform[] spawnPoint; // 스폰 위치(자동화로 넣어줌)
    
    [Header("----- Normal Mode Setting -----")]
    public Wave[]    normalWave;
    public Elite[]   normalElite;
    public EnemyData normalBossData;
    
    [Header("----- AI Mode Setting -----")]
    public Wave[]  AI_Wave;
    public Elite[] AI_elite;
    
    // Wave
    private  Wave[]   wave;
    
    [HideInInspector] public int  currentWaveLevel; // 현재 웨이브 레벨(0에서 시작하여, 시간이 지남에 증가)
    [HideInInspector] public bool waveEnd;          // 웨이브가 끝나면, 보스 생성
    
    private float waveDurationTimer;     // 웨이브 시간 체크
    private float creationIntervalTimer; // 스폰 시간 체크
    
    // Elite
    private Elite[] elite;
    
    private float  eliteDurationTimer;     // 웨이브 시간 체크
    
    // Boss
    private EnemyData bossData;
    [HideInInspector] public Enemy bossEnemy;
    [HideInInspector] public bool  isBossDie;
    
    void Awake()
    {
        instance = this;
        
        // 모든 자식 Transform을 가져오기(= 자신도 포함됨...)
        Transform[] allTransforms = GetComponentsInChildren<Transform>();
        spawnPoint = allTransforms.Where(t => t != this.transform).ToArray(); // 자신의 Transform은 제외
    }

    private void Update()
    {
        if (!GameManager.instance.isLive)
            return;
    
        SpawnCheck();
        EliteCheck();
        
        WaveCheck();
    }

    public void SpawnSetting()
    {
        // 노멀 스폰 세팅
        if (GameManager.instance.isNormalGameMode)
        {
            wave     = normalWave;
            elite    = normalElite;
            bossData = normalBossData;
        }
        // AI 스폰 세팅
        else
        {
            wave     = AI_Wave;
            elite    = AI_elite;
        }
    }

    private void WaveCheck()
    {
        waveDurationTimer += Time.deltaTime;
        
        // 웨이브 업
        if (!waveEnd && waveDurationTimer > wave[currentWaveLevel].waveDuration)
        {
            waveDurationTimer = 0;
            currentWaveLevel++;
            
            // 보스 생성
            if (currentWaveLevel == wave.Length)
            {
                // 노멀 모드 웨이브 끝 -> 보스 생성
                if (GameManager.instance.isNormalGameMode)
                {
                    waveEnd = true;
                    StartCoroutine(SpawnBoss(bossData));
                }
                // 일반 모드 웨이브 끝 -> 종료
                else
                {
                    isBossDie = true;
                    GameManager.instance.enemyCleaner.SetActive(true); // 클리너 on
                }
            }
        }
    }
    
    private void SpawnCheck()
    {
        creationIntervalTimer += Time.deltaTime;
        
        // 적 스폰
        if (!waveEnd && creationIntervalTimer > wave[currentWaveLevel].creationInterval)
        {
            creationIntervalTimer = 0;

            // 총 비율
            int totalRate = 0;
            foreach (var enemyRates in wave[currentWaveLevel].enemyRate)
                totalRate += enemyRates.creationRate;

            // 적 배열 찾기
            // totalRate안에서 해당되는 배열 찾기(초급 배열 or 중급 배열 or 상급 배열)
            int randomNum = Random.Range(0, totalRate);
            int accumulatedRate = 0;
            foreach (var enemyRates in wave[currentWaveLevel].enemyRate)
            {
                accumulatedRate += enemyRates.creationRate; // 순차적으로 생성 % 확인

                if (randomNum < accumulatedRate)
                {
                    // 선택된, 적 난이도에서 적 1개 선택
                    EnemyData randomEnemy = enemyRates.spawnEnemyData[Random.Range(0, enemyRates.spawnEnemyData.Length)];
                    // 생성
                    SpawnEnemy(randomEnemy);
                    break;
                }
            }
        }
    }
    
    private void EliteCheck()
    {
        eliteDurationTimer += Time.deltaTime;
        
        // 엘리트 스폰(막족하는 )
        foreach (var elites in elite)
        {   
            // 아직 생성 안함 + 생성 시간이 됨
            if (!elites.isCreation && elites.spawnTime < eliteDurationTimer)
            {
                // 숫자 리스트 생성(spawnPoint.Length의 길이 만큼)
                List<int> numbers = new List<int>();
                for (int i = 0; i < spawnPoint.Length; i++)
                    numbers.Add(i);

                // numbers리스트 mix
                for (int i = 0; i < numbers.Count; i++)
                {
                    int randomIndex = Random.Range(0, numbers.Count);
                    (numbers[i], numbers[randomIndex]) = (numbers[randomIndex], numbers[i]);
                    // 같음
                    // int temp             = numbers[i];
                    // numbers[i]           = numbers[randomIndex];
                    // numbers[randomIndex] = temp;
                }
                
                // elites.spawnNum만큼 생성
                for (int i = 0; i < elites.spawnNum; i++)
                {
                    elites.isCreation = true;
                    SpawnElite(elites.spawnEnemyData,numbers[i]); // mix된 numbers[i]를 elites.spawnNum만큼 반복
                }
            }
        }
    }

    private void SpawnEnemy(EnemyData data)
    {
        // 안전성 검사: EnemyData 검증
        if (data == null)
        {
            Debug.LogError("SpawnManager: SpawnEnemy - EnemyData가 null입니다!");
            return;
        }
        
        // 안전성 검사: PoolManager 검증
        if (PoolManager.instance == null)
        {
            Debug.LogError("SpawnManager: PoolManager.instance가 null입니다!");
            return;
        }
        
        GameObject enemy = PoolManager.instance.Get(0);                        // 0번이 적 프리팹(ID에 따라, 자동으로 애니메이션 등등 교체함)
        if (enemy == null)
        {
            Debug.LogError("SpawnManager: Enemy 생성 실패!");
            return;
        }
        
        // 안전성 검사: spawnPoint 배열 검증
        if (spawnPoint == null || spawnPoint.Length <= 1)
        {
            Debug.LogError("SpawnManager: spawnPoint 배열이 유효하지 않습니다!");
            enemy.SetActive(false);
            return;
        }
        
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position; // 랜덤 생성 위치
        
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.Init(data);                                             // 웨이브 레벨에 따라, spawnData 변경 및 초기화
        }
        else
        {
            Debug.LogError("SpawnManager: Enemy 컴포넌트를 찾을 수 없습니다!");
            enemy.SetActive(false);
        }
    }
    
    private void SpawnElite(EnemyData data, int randomMixNum)
    {
        // 안전성 검사: EnemyData 검증
        if (data == null)
        {
            Debug.LogError("SpawnManager: SpawnElite - EnemyData가 null입니다!");
            return;
        }
        
        // 안전성 검사: PoolManager 검증
        if (PoolManager.instance == null)
        {
            Debug.LogError("SpawnManager: PoolManager.instance가 null입니다!");
            return;
        }
        
        // 안전성 검사: spawnPoint 인덱스 검증
        if (spawnPoint == null || randomMixNum < 0 || randomMixNum >= spawnPoint.Length)
        {
            Debug.LogError($"SpawnManager: spawnPoint 인덱스({randomMixNum})가 유효하지 않습니다!");
            return;
        }
        
        GameObject enemy = PoolManager.instance.Get(0);  // 0번이 적 프리팹(ID에 따라, 자동으로 애니메이션 등등 교체함)
        if (enemy == null)
        {
            Debug.LogError("SpawnManager: Elite Enemy 생성 실패!");
            return;
        }
        
        enemy.transform.position = spawnPoint[randomMixNum].position; // 겹치지 않는 랜덤 생성 위치
        
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.Init(data);                       // 웨이브 레벨에 따라, spawnData 변경 및 초기화
        }
        else
        {
            Debug.LogError("SpawnManager: Elite Enemy 컴포넌트를 찾을 수 없습니다!");
            enemy.SetActive(false);
        }
    }
    
    private IEnumerator SpawnBoss(EnemyData data)
    {
        GameManager.instance.enemyCleaner.SetActive(true); // 클리너 on

        yield return new WaitForFixedUpdate();
        
        GameManager.instance.enemyCleaner.SetActive(false); // 클리너 off
        
        yield return new WaitForFixedUpdate();
    
        GameObject enemy         = PoolManager.instance.Get(0); // 0번이 적 프리팹(ID에 따라, 자동으로 애니메이션 등등 교체함)
        enemy.transform.position = transform.position;               // 중앙에서 생성
        enemy.GetComponent<Enemy>().Init(data);                      // 웨이브 레벨에 따라, spawnData 변경 및 초기화
        bossEnemy = enemy.GetComponent<Enemy>();
        
        IngameUI.instance.bossHealthSlider.SetActive(true);
    }
}