using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class AchieveManager : MonoBehaviour
{
    public static AchieveManager instance;

    [Header("----- Character -----")]
    public GameObject[] lockCharacter;
    public GameObject[] unlockCharacter;
    public GameObject   uiNotice;
    
    private enum AchieveCharacterType { UnlockSpear, UnlockSword }
    private AchieveCharacterType[] achieveCharacters;
    
    [Header("----- Gold -----")]
    public TextMeshProUGUI dataGoldText;
    
    private void Awake()
    {
        instance = this;
    
        achieveCharacters = (AchieveCharacterType[])Enum.GetValues(typeof(AchieveCharacterType));
        // MyData가 없으면, 초기화
        if (!PlayerPrefs.HasKey("CharacterData"))
            InitCharacter();
        
        // GoldData가 없으면, 초기화
        if (!PlayerPrefs.HasKey("GoldData"))
            InitGold();
        // 있으면, 설정
        else
        {
            dataGoldText.text = PlayerPrefs.GetInt("GoldData").ToString();
        }
        
        if (PlayerPrefs.GetInt("GoldData") == 0)
        {
            PlayerPrefs.SetInt("GoldData", 50000);
            dataGoldText.text = PlayerPrefs.GetInt("GoldData").ToString();
        }
    }

    private void InitCharacter()
    {
        PlayerPrefs.SetInt("CharacterData", 1);
        // 캐릭터 획득 상태 초기화
        foreach (AchieveCharacterType achieve in achieveCharacters)
            PlayerPrefs.SetInt(achieve.ToString(), 0);
    }

    private void Start()
    {
        // 획득 상태에 따라, UI 활성화
        UnlockCharacter();
    }

    private void LateUpdate()
    {
        // 획득 상태 체크
        foreach (AchieveCharacterType achieve in achieveCharacters)
            CheckAchieve(achieve);
    }
    
    private void UnlockCharacter()
    {
        for (int index = 0; index < lockCharacter.Length; index++) 
        {
            string achieveName = achieveCharacters[index].ToString();
            bool   isUnlock   = PlayerPrefs.GetInt(achieveName) == 1;
            lockCharacter[index].SetActive(!isUnlock);
            unlockCharacter[index].SetActive(isUnlock);
        }
    }
    
    private void CheckAchieve(AchieveCharacterType achieveCharacterType)
    {
        bool iAchieve = false;
        
        // 조건 달성 체크
        switch (achieveCharacterType) 
        {
            case AchieveCharacterType.UnlockSpear:
                if (GameManager.instance.isLive)
                    iAchieve = GameManager.instance.currentKill >= 100;
                break;
                
            case AchieveCharacterType.UnlockSword:
                iAchieve = GameManager.instance.gameClear;  // 보스를 처치하고, 게임을 클리어 한 경우
                break;
        }

        // 조건 달성 + 아직 미획득
        if (iAchieve && PlayerPrefs.GetInt(achieveCharacterType.ToString()) == 0) 
        {
            PlayerPrefs.SetInt(achieveCharacterType.ToString(), 1);
        
            for (int index = 0; index < uiNotice.transform.childCount; index++) 
            {
                bool isActive = index == (int)achieveCharacterType;
                uiNotice.transform.GetChild(index).gameObject.SetActive(isActive);  // 해당 알림 텍스트 활성화(알맞는)
            }

            StartCoroutine(NoticeRoutine());
        }
    }

    private IEnumerator NoticeRoutine()
    {
        uiNotice.SetActive(true);                                   // 알림 틀 켜기
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);

        yield return new WaitForSecondsRealtime(5);

        uiNotice.SetActive(false);
    }

    private void InitGold()
    {
        PlayerPrefs.SetInt("GoldData", 0);
    }
}

