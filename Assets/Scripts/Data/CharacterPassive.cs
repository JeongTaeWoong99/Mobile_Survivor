using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

// ID에 따라, 패시브를 가지고 있음.
// ID를 확인하고, 패시브값을 더해주어야 함.
public class CharacterPassive : MonoBehaviour
{
    // PlayerPrefs 캐싱 - 게임 시작 시 한 번만 로드하여 성능 최적화
    private static bool _isInitialized = false;
    private static int _powerUp0Level;
    private static int _powerUp1Level; 
    private static int _powerUp2Level;
    private static int _powerUp3Level;
    private static int _powerUp4Level;
    private static int _powerUp5Level;
    private static int _powerUp6Level;
    private static int _powerUp7Level;
    private static int _powerUp8Level;
    private static int _powerUp9Level;
    
    /// <summary>
    /// PlayerPrefs 값들을 캐싱합니다. 게임 시작 시 한 번만 호출하세요.
    /// </summary>
    public static void InitializePowerUps()
    {
        _powerUp0Level = PlayerPrefs.GetInt("PowerUp0Level", 0);
        _powerUp1Level = PlayerPrefs.GetInt("PowerUp1Level", 0);
        _powerUp2Level = PlayerPrefs.GetInt("PowerUp2Level", 0);
        _powerUp3Level = PlayerPrefs.GetInt("PowerUp3Level", 0);
        _powerUp4Level = PlayerPrefs.GetInt("PowerUp4Level", 0);
        _powerUp5Level = PlayerPrefs.GetInt("PowerUp5Level", 0);
        _powerUp6Level = PlayerPrefs.GetInt("PowerUp6Level", 0);
        _powerUp7Level = PlayerPrefs.GetInt("PowerUp7Level", 0);
        _powerUp8Level = PlayerPrefs.GetInt("PowerUp8Level", 0);
        _powerUp9Level = PlayerPrefs.GetInt("PowerUp9Level", 0);
        
        _isInitialized = true;
        Debug.Log("CharacterPassive: PlayerPrefs 캐싱 완료");
    }
    
    /// <summary>
    /// PowerUp 값이 변경되었을 때 캐시를 새로고침합니다.
    /// </summary>
    public static void RefreshPowerUps()
    {
        InitializePowerUps();
        Debug.Log("CharacterPassive: 캐시 새로고침 완료");
    }
    // public static float MoveSpeed
    // {
    //     get { return GameManager.instance.playerId == 0 ? 1.1f : 1f; }
    // }
    
    // public static float WeaponRate
    // {
    //     get { return GameManager.instance.playerId == 1 ? 0.9f : 1f; }
    // }
    
    // public static int WeaponCount
    // {
    //     get { return GameManager.instance.playerId == 3 ? 1 : 0; }
    // }
    
    
    // 거지
    public static float Gold => GameManager.instance.playerId == 0 ? 1.1f : 1f;
    
    // 궁병
    public static float RangeDamage(ItemData.ItemType itemType)
    {
        if (itemType != ItemData.ItemType.Range)
            return 1f;
        
        return GameManager.instance.playerId == 1 ? 1.1f : 1f;
    }
    
    // 창병
    public static float AllDamage => GameManager.instance.playerId == 2 ? 1.05f : 1f;
    
    // 검사
    public static float MeleeDamage(ItemData.ItemType itemType)
    {
        if (itemType != ItemData.ItemType.Melee)
            return 1f;

        return GameManager.instance.playerId == 3 ? 1.1f : 1;
    }
    
    // 파워업 0 (체력 +) - 캐싱된 값 사용
    public static int PowerUp0()
    {
        if (!_isInitialized) InitializePowerUps();
        return _powerUp0Level * 10;
    }
    
    // 파워업 1 (체력 회복 +) - 캐싱된 값 사용 (매 프레임 호출되므로 가장 중요!)
    public static float PowerUp1()
    {
        if (!_isInitialized) InitializePowerUps();
        return _powerUp1Level * 0.1f;
    }
    
    // 파워업 2 (부활) - 캐싱된 값 사용
    public static bool PowerUp2()
    {
        if (!_isInitialized) InitializePowerUps();
        return _powerUp2Level == 1;
    }
    
    // 파워업 3 (관통 +) - 캐싱된 값 사용
    public static int PowerUp3()
    {
        if (!_isInitialized) InitializePowerUps();
        return _powerUp3Level * 1;
    }
    
    // 파워업 4 (총 공격력 %) - 캐싱된 값 사용
    public static float PowerUp4()
    {
        if (!_isInitialized) InitializePowerUps();
        return _powerUp4Level * 2f;
    }
    
    // 파워업 5 (총 공속 %) - 캐싱된 값 사용
    public static float PowerUp5()
    {
        if (!_isInitialized) InitializePowerUps();
        return _powerUp5Level * 5f;
    }
    
    // 파워업 6 (총 겸치 %) - 캐싱된 값 사용
    public static float PowerUp6()
    {
        if (!_isInitialized) InitializePowerUps();
        return _powerUp6Level * 5f;
    }
    
    // 파워업 7 (획득 범위 %) - 캐싱된 값 사용
    public static float PowerUp7()
    {
        if (!_isInitialized) InitializePowerUps();
        return _powerUp7Level * 10f;
    }
    
    // 파워업 8 (이속 %) - 캐싱된 값 사용
    public static float PowerUp8()
    {
        if (!_isInitialized) InitializePowerUps();
        return _powerUp8Level * 5f;
    }
    
    // 파워업 9 (투사체 +) - 캐싱된 값 사용
    public static int PowerUp9()
    {
        if (!_isInitialized) InitializePowerUps();
        return _powerUp9Level * 1;
    }
}