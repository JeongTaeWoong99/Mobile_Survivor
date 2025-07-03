using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("----- Game Control -----")]
    public  int[] nextEXP;
    private int   remainEXP;
    
    public GameObject enemyCleaner;
    
    [HideInInspector] public bool isNormalGameMode; // 기본 모드 true // AI 모드 false
    public List<GameObject> mapList;
    
    [HideInInspector] public float currentGameTime;
    [HideInInspector] public bool  isLive;         
    [HideInInspector] public bool  gameClear; // 보스를 처치하고, 게임이 끝난 경우

    [Header("----- Player Info -----")] 
    public RuntimeAnimatorController[] animCon;
    public float                       playerBaseMaxHealth;
    public float                       playerCurrentMaxHealth;
    public float                       playerCurrentHealth;
    public float                       baseSpeed;   
    public float                       baseDetectionRadius; 
    public LayerMask                   expBeadLayer;          
    
    [HideInInspector] public int playerId;       
    [HideInInspector] public int playerLevel;    
    [HideInInspector] public int currentKill;    
    [HideInInspector] public int currnetEXP;     
    [HideInInspector] public int currentGold;    
    
    [Header("----- Player Accessories -----")] 
    [HideInInspector] public float moveSpeedInPer;          // 베이스 x 퍼센트
    [HideInInspector] public float expInPer;                // 베이스 x 퍼센트
    [HideInInspector] public float maxHpInPer;              // 베이스 x 퍼센트
    [HideInInspector] public int   perHpIn;                 // 베이스 x 퍼센트
    [HideInInspector] public float acquisitionRangeInPer;   // 베이스 x 퍼센트
    [HideInInspector] public float totalAttackRateInPer;    // 베이스 x 무기 퍼센트 x 토탈 퍼센트
    [HideInInspector] public float totalAttackDamageInPer;  // 베이스 x 무기 퍼센트 x 토탈 퍼센트
    
    [Header("----- AI Info -----")] 
    [HideInInspector] public int current_AI_Kill;
    
    private void Awake()
    {
        instance = this;


#if UNITY_EDITOR
        // Application.targetFrameRate = 60;
        // Time.fixedDeltaTime         = 0.02f;
        Application.targetFrameRate = 30;
        Time.fixedDeltaTime         = 0.04f;
#else
        Application.targetFrameRate = 30;
        Time.fixedDeltaTime         = 0.04f;
        // // Application.targetFrameRate = 60;
        // // Time.fixedDeltaTime         = 0.02f;
#endif
        
        Time.timeScale = 1;
        
        // PlayerPrefs 캐싱 초기화 (성능 최적화)
        CharacterPassive.InitializePowerUps();

        moveSpeedInPer         = 100f + CharacterPassive.PowerUp8();
        expInPer               = 100f + CharacterPassive.PowerUp6();
        maxHpInPer             = 100f;
        perHpIn                = 0;
        acquisitionRangeInPer  = 100f + CharacterPassive.PowerUp7();
        totalAttackRateInPer   = 100f + CharacterPassive.PowerUp5();
        totalAttackDamageInPer = 100f + CharacterPassive.PowerUp4();
    }

    void Update()
    {
        if (!isLive)
            return;
        
        // 초당 체력 회복
        if(playerCurrentHealth < playerCurrentMaxHealth)
            playerCurrentHealth += Time.deltaTime * (perHpIn + CharacterPassive.PowerUp1());
        
        //게임 시작 체크
        currentGameTime += Time.deltaTime;
        
        // 보스 처치 시, GameVictory(); 실행
        if (SpawnManager.instance.isBossDie && !gameClear)
        {
            // 노말 모드 보스 처치 -> 게임 승리
            if (isNormalGameMode)
            {
                gameClear = true;
                GameVictory();
            }
            // AI 경쟁 모드
            else
            {
                gameClear = true;
                // AI 보다 킬이 많음 -> 승리
                if (currentKill > current_AI_Kill)
                    GameVictory();
                // AI 보다 킬이 적거나 같음 -> 패배
                else
                    GameOver();
            }
        }
    }

    public void GameStart(int ID)
    {
        StartCoroutine(GameStartCo(ID));
    }
    
    // 캐릭터 선택하면, 바로 게임 시작
    private IEnumerator GameStartCo(int ID)
    {
        AudioManager.instance.PlayBgm(0,false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        
        // 페이드인 시작 (검은 화면으로 전환)
        MainUI.instance.FadeIn(); 
        
        // 페이드인이 완전히 끝날 때까지 대기 (2초)
        yield return new WaitForSeconds(2);
        
        // 이제 완전히 검은 화면 상태에서 Pre-warming 실행 (플레이어가 렉을 느끼지 못함)
        if (PoolManager.instance != null)
        {
            Debug.Log("오브젝트 풀 Pre-warming 시작... (검은 화면에서 진행)");
            yield return StartCoroutine(PoolManager.instance.PreWarmAllPoolsCoroutine(10));
            Debug.Log("오브젝트 풀 Pre-warming 완료!");
        }
        
        MainUI.instance.AllMainUI_Invisible();
        
        playerId                = ID;
        playerCurrentMaxHealth  = playerBaseMaxHealth + CharacterPassive.PowerUp0(); // 최대 체력 설정
        playerCurrentHealth     = playerBaseMaxHealth + CharacterPassive.PowerUp0(); // 현재 체력 설정
        
        // 기본 무기 지급
        IngameUI.instance.ProvidesBasicWeapons(playerId);
        Player.instance.Init(); // 캐릭터 세팅

        // 맵 세팅(+위치 이동)
        ModeSetting();
        yield return new WaitForSeconds(.1f);
        
        MainUI.instance.FadeOut();
        
        yield return new WaitForSeconds(2);
        
        Resume();
        AudioManager.instance.PlayBgm(1,true);
    }
    
    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }
    
    private IEnumerator GameOverRoutine()
    {
        isLive = false; 
        
        yield return new WaitForSeconds(0.5f);  
        
        IngameUI.instance.Lose();
        Stop();
        
        AudioManager.instance.PlayBgm(1,false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
        
        // 골드 더해주기
        StartCoroutine(IncrementGoldCoroutine((int)(currentGold * CharacterPassive.Gold)));
    }
    
    private void GameVictory()
    {
        StartCoroutine(GameVictoryRoutine());
    }
    
    private IEnumerator GameVictoryRoutine()
    {
        isLive = false;
        enemyCleaner.SetActive(true);   
        yield return new WaitForSeconds(0.5f);  
        IngameUI.instance.Win();
        Stop();
        AudioManager.instance.PlayBgm(1,false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
        
        // 골드 더해주기
        StartCoroutine(IncrementGoldCoroutine((int)(currentGold * CharacterPassive.Gold)));
    }

    public void GetExp(int minEXP,int maxEXP)
    {
        if (!isLive)
            return;
        //Debug.Log(minEXP + " ~ " + maxEXP);
        int minExpValue = (int)(minEXP * expInPer / 100f);
        int maxExpValue = (int)(maxEXP * expInPer / 100f);
        //Debug.Log(minExpValue + " ~ " + maxExpValue);
        currnetEXP += Random.Range(minExpValue, maxExpValue); // 최소 ~ 최대 값에서 랜덤으로 EXP 증가
        
        // currnetEXP == nextEXP[playerLevel] 일 때, IF 작동
        // nextEXP[playerLevel]의 playerLevel에 nextEXP.Length - 1보다 큰 값이 들어갈 수 없음.
        remainEXP = currnetEXP - nextEXP[Mathf.Min(playerLevel, nextEXP.Length - 1)];
        if (currnetEXP >= nextEXP[Mathf.Min(playerLevel, nextEXP.Length - 1)]) 
        {
            playerLevel++;
            currnetEXP = 0;
            IngameUI.instance.Show();
            
            // 초과 경험치 유지 + GetExp한번 더 호출하여, 
            if (remainEXP > 0)
            {
                currnetEXP = remainEXP;
                //Debug.Log(currnetEXP + " / " + nextEXP[playerLevel]);
            }
        }
    }

    public void Stop()
    {
        isLive = false;     // 이동 방지
        Time.timeScale = 0;
        IngameUI.instance.hud.SetActive(false);
        IngameUI.instance.skillButtonPanel.gameObject.transform.localScale = Vector3.zero; // 코루틴이 꺼지지 않도록, 크기를 줄임....!
        IngameUI.instance.uiJoy.localScale                                 = Vector3.zero; // 코루틴이 꺼지지 않도록, 크기를 줄임....!
    }   
    
    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
        
        IngameUI.instance.hud.SetActive(true);
        IngameUI.instance.skillButtonPanel.gameObject.transform.localScale = Vector3.one; // 코루틴이 꺼지지 않도록, 크기를 줄임....!
        IngameUI.instance.uiJoy.localScale                                 = Vector3.one; // 코루틴이 꺼지지 않도록, 크기를 줄임....!
    }
    
    private IEnumerator IncrementGoldCoroutine(int targetGold)
    {
        PlayerPrefs.SetInt("GoldData",PlayerPrefs.GetInt("GoldData") + targetGold); // 미리 더해주기(코루틴이 끝나기 전, 나가버리는 경우 대비)
        
        int   startGold = currentGold;
        float elapsed   = 0f;

        while (currentGold < targetGold)
        {
            elapsed += Time.fixedUnscaledDeltaTime * 2f;
            currentGold = Mathf.Clamp((int)Mathf.Lerp(startGold, targetGold, elapsed), startGold, targetGold);
            yield return null;
        }
        
        currentGold = targetGold;
    }
    
    // 노말 모드 맵 + 캐릭터 세팅
    private void ModeSetting()
    {
        SpawnManager.instance.SpawnSetting();
        
        if (isNormalGameMode)
        {
            foreach (var mapLists in mapList)
                mapLists.SetActive(false);
            mapList[0].SetActive(true);
        }
        else
        {
            foreach (var mapLists in mapList)
                mapLists.SetActive(false);
            mapList[1].SetActive(true);
            Player.instance.transform.position += new Vector3(-4, 0, 0);
            IngameUI.instance.killUI.SetActive(true);
        }
    }

#if UNITY_EDITOR
    [MenuItem("Window/PlayerPrefs 초기화")]
    private static void ResetPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs has been reset.");
    }
#endif
}
