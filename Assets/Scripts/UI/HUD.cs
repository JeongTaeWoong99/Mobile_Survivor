using UnityEngine;
using UnityEngine.UI;

// HUD enum에 따라서, 각각 넣어줌.
public class HUD : MonoBehaviour
{
    public enum     InfoType { FPS, Exp, Level, PlayerKill, Time, Health, Wave, Gold, BossHealth,BossShied,AI_Kill}
    public InfoType type;

    private Text   myText;
    private Slider mySlider;
    
    // FPS 계산용 변수들
    private float fpsUpdateInterval = 1f;   // 1초마다 FPS 업데이트
    private float fpsAccumulator    = 0f;   // 누적 시간
    private int   fpsFrames         = 0;    // 누적 프레임 수
    private float currentFPS        = 0f;   // 현재 계산된 FPS

    void Awake()
    {
        myText   = GetComponent<Text>();
        mySlider = GetComponent<Slider>();
    }
    
    void Update()
    {
        // FPS 계산 (평균 FPS를 위한 누적)
        if (type == InfoType.FPS)
        {
            fpsAccumulator += Time.deltaTime;
            fpsFrames++;
            
            // 일정 간격마다 FPS 계산 및 업데이트
            if (fpsAccumulator >= fpsUpdateInterval)
            {
                currentFPS     = fpsFrames / fpsAccumulator;
                fpsAccumulator = 0f;
                fpsFrames      = 0;
            }
        }
    }

    void LateUpdate()
    {
        // GameManager 인스턴스 null 체크
        if (GameManager.instance == null)
            return;
            
        switch (type) 
        {
            case InfoType.FPS:
                if (myText != null)
                    myText.text = $"FPS: {currentFPS:F1}";
                break;
            
            case InfoType.Exp:
                if (mySlider != null && GameManager.instance.nextEXP != null)
                {
                    float curExp   = GameManager.instance.currnetEXP;
                    float maxExp   = GameManager.instance.nextEXP[Mathf.Min(GameManager.instance.playerLevel, GameManager.instance.nextEXP.Length - 1)];    // Min 제한
                    mySlider.value = curExp / maxExp;
                }
                break;
                
            case InfoType.Level:
                if (myText != null)
                    myText.text = $"Lv.{GameManager.instance.playerLevel:F0}";
                break;
                
            case InfoType.Wave:
                if (myText != null && SpawnManager.instance != null)
                    myText.text = !SpawnManager.instance.waveEnd ? $"Wave {SpawnManager.instance.currentWaveLevel:F0}" : "BOSS";
                break;

            case InfoType.PlayerKill:
                if (myText != null)
                    myText.text = $"{GameManager.instance.currentKill:F0}";
                break;
            
            case InfoType.AI_Kill:
                if (myText != null)
                    myText.text = $"{GameManager.instance.current_AI_Kill:F0}";
                break;
                
            case InfoType.Time:
                if (myText != null)
                {
                    float remainTime = GameManager.instance.currentGameTime;
                    int min          = Mathf.FloorToInt(remainTime / 60);
                    int sec          = Mathf.FloorToInt(remainTime % 60);
                    myText.text      = $"{min:D2}:{sec:D2}";
                }
                break;
                
            case InfoType.Health:
                if (mySlider != null)
                {
                    float curHealth = GameManager.instance.playerCurrentHealth;
                    float maxHealth = GameManager.instance.playerCurrentMaxHealth;
                    mySlider.value  = curHealth / maxHealth;
                }
                break;
            
            case InfoType.BossHealth:
                if (mySlider != null && SpawnManager.instance != null && SpawnManager.instance.waveEnd && SpawnManager.instance.bossEnemy != null)
                {   
                    float curBossHealth = SpawnManager.instance.bossEnemy.currentHealth;
                    float maxBossHealth = SpawnManager.instance.bossEnemy.maxHealth;
                    mySlider.value  = curBossHealth / maxBossHealth;
                }
                break;
            
            case InfoType.BossShied:
                if (mySlider != null && SpawnManager.instance != null)
                {
                    if (SpawnManager.instance.waveEnd && SpawnManager.instance.bossEnemy != null && SpawnManager.instance.bossEnemy.isShieldBuff)
                    {
                        float curBossShield = SpawnManager.instance.bossEnemy.currentShieldAmount;
                        float maxBossShield = SpawnManager.instance.bossEnemy.shieldBuffValue[1];
                        mySlider.value  = curBossShield / maxBossShield;
                    }
                    else
                        mySlider.value = 0;
                }
                break;

            case InfoType.Gold:
                if (myText != null)
                    myText.text = $"{GameManager.instance.currentGold:F0}";
                break;
        }
    }
}

