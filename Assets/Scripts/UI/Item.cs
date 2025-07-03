using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    private bool isNewCreate;   // 존재하지 않아, 새로 만들어짐

    [Header("----- Common -----")]
    public ItemData data;
    
    public List<Image> imageList;
    public List<Text>  textList;
    
    [Header("----- Weapon/accessories-----")]
    [HideInInspector] public int         level;
    [HideInInspector] public Weapon      weapon;      // 무기 스크립트
    [HideInInspector] public Accessories accessories; // 악세 스크립트
    
    private Text  textState; // 장착 상태
    private Text  textName;  // 이름
    private Text  textType;  // 무기 타입
    private Text  textLevel; // 레벨
    private Text  textDesc;  // 레벨업 설명
    
    [Header("----- Skill -----")]
    public GameObject skillPanel;
    
    private SkillData currentSkillData; // 활성화 되었을 때, 보여지는 스킬 데이터가 들어감.
    
    private Text textSkillName;
    private Text textSkillDesc;

    void Awake()
    {
        // 무기 및 악세서리 세팅
        imageList[0].sprite = data.itemIcon;
        
        textState     = textList[0];
        textName      = textList[1];
        textName.text = data.itemName;
        textType      = textList[2];
        textType.text = data.itemType.ToString();
        textLevel     = textList[3];
        textDesc      = textList[4];
        
        // 스킬 세팅
        skillPanel.SetActive(false);
        
        textSkillName = textList[5];
        textSkillDesc = textList[6];
    }

    // 활성화 시, 텍스트 변경
    private void OnEnable()
    {
        WeaponTextSet();
        AccessoriesTextSet();
        ConsumptionTextSet();
    }

    private void WeaponTextSet()
    {
        // 무기
        // 장착상태 // 레벨 // 업그레이드
        if (data.itemType is ItemData.ItemType.Melee or ItemData.ItemType.Range)
        {
            // 생성 전
            if (!isNewCreate)
            {
                textState.text = "신규";

                textLevel.text = level.ToString();
                
                textDesc.text = "";
                textDesc.text = data.desc;
            }
            // 생성 후
            else if (isNewCreate)
            {
                textState.text = "장착 중";
                textLevel.text = level + " -> " + (level+1);
                textDesc.text  = "";
                foreach (var VARIABLE in data.levelUpList[level].levelUpData)
                {
                    switch (VARIABLE.levelUpType)
                    {
                        case ItemData.LevelUpType.Damage:
                            textDesc.text += "공격력 " + VARIABLE.increaseValue + "% 증가\n";
                            break;
                        case ItemData.LevelUpType.Rate:
                            textDesc.text += "공격 속도 " + VARIABLE.increaseValue + "% 증가\n";
                            break;
                        case ItemData.LevelUpType.Range:
                            textDesc.text += "범위 " + VARIABLE.increaseValue + "% 증가\n";
                            break;
                        case ItemData.LevelUpType.Count:
                            textDesc.text += "무기 " + VARIABLE.increaseValue + "개 증가\n";
                            break;
                        case ItemData.LevelUpType.Penetration:
                            textDesc.text += "관통 " + VARIABLE.increaseValue + "명 증가\n";
                            break;
                    }
                }
            }
            
            // 빈 버튼 공간 있는지 확인...
            bool emptySkillButton = false;
            foreach (var VARIABLE in IngameUI.instance.playerSkillList)
            {
                if (!VARIABLE.isMounting)
                    emptySkillButton = true;
            }
            
            // 스킬 패널 활성화
            if ((level + 1) == 5 && emptySkillButton) // 4레벨(5레벨 전)에 활성화
            { 
                skillPanel.SetActive(true);
                SkillPanelSet(data.selectableSkillList[Random.Range(0,data.selectableSkillList.Count)]);    // 랜덤 무기 스킬 보여주기
            }
            else
                skillPanel.SetActive(false);
        }
    }

    private void AccessoriesTextSet()
    { 
        // 악세사리
        if(data.itemType == ItemData.ItemType.Accessories)
        {
            // 생성 전
            if (!isNewCreate)
            {
                textState.text = "신규";
                
                textLevel.text = (level + 1).ToString();
                
                textDesc.text = "";
                if(data.itemId == 0)
                    textDesc.text += data.desc + "\n이동 속도 " + data.levelUpList[level].levelUpData[0].increaseValue + "%";
                if(data.itemId == 1)
                    textDesc.text += data.desc + "\n경험치 획득량 " + data.levelUpList[level].levelUpData[0].increaseValue + "%";
                if(data.itemId == 2)
                    textDesc.text += data.desc + "\n최대 체력 " + data.levelUpList[level].levelUpData[0].increaseValue + "%";
                if(data.itemId == 3)
                    textDesc.text += data.desc + "\n총 공격 속도 " + data.levelUpList[level].levelUpData[0].increaseValue + "%";
                if (data.itemId == 4)
                    textDesc.text += data.desc + "\n초당 체력 회복 " + data.levelUpList[level].levelUpData[0].increaseValue;
                if(data.itemId == 5)
                    textDesc.text += data.desc + "\n경험치 획득 반경 " + data.levelUpList[level].levelUpData[0].increaseValue + "%";
                if(data.itemId == 6)
                    textDesc.text += data.desc + "\n총 공격력 " + data.levelUpList[level].levelUpData[0].increaseValue + "% ";
            }
            // 생성 후
            else if (isNewCreate)
            {
                textState.text = "장착 중";
                textLevel.text = level + " -> " + (level + 1);
                textDesc.text  = "";
                if(data.itemId == 0)
                    textDesc.text += "이동 속도 " + data.levelUpList[level].levelUpData[0].increaseValue + "%";
                if(data.itemId == 1)
                    textDesc.text += "경험치 획득량 " + data.levelUpList[level].levelUpData[0].increaseValue + "%";
                if(data.itemId == 2)
                    textDesc.text += "최대 체력 " + data.levelUpList[level].levelUpData[0].increaseValue + "%";
                if(data.itemId == 3)
                    textDesc.text += "총 공격 속도 " + data.levelUpList[level].levelUpData[0].increaseValue + "%";
                if(data.itemId == 4)
                    textDesc.text += "초당 체력 회복 " + data.levelUpList[level].levelUpData[0].increaseValue;
                if(data.itemId == 5)
                    textDesc.text += "경험치 획득 반경 " + data.levelUpList[level].levelUpData[0].increaseValue + "%";
                if(data.itemId == 6)
                    textDesc.text += "총 공격력 " + data.levelUpList[level].levelUpData[0].increaseValue + "%";
            }
        }
    }

    private void ConsumptionTextSet()
    {
        // 소모품
        if(data.itemType == ItemData.ItemType.Consumption)
        {
            textState.gameObject.SetActive(false);
            
            textLevel.gameObject.SetActive(false);
            
            textDesc.text = "";
            textDesc.text = data.desc;
        }
    }

    private void SkillPanelSet(SkillData data)
    {
        currentSkillData    = data;
        
        imageList[1].sprite = data.skillIcon;
        textSkillName.text  = data.skillName;
        textSkillDesc.text  = data.skillDesc;
    }
    
    // 클릭 시, 상호작용
    public void OnClick()
    {
        switch (data.itemType) 
        {
            case ItemData.ItemType.Melee: case ItemData.ItemType.Range:
                // 처음 획득(캐릭터 선택 or 아이템 선택) - > 새로 생성
                if (!isNewCreate && level == 0)
                {
                    isNewCreate = true;
                    GameObject newWeapon = new GameObject();
                    weapon = newWeapon.AddComponent<Weapon>();
                    weapon.Init(data);
                }
                // 이미 보유 -> 레벨 업
                else
                {
                    float nextDamage      = 0;
                    float nextRate        = 0;
                    float nextRange       = 0;
                    int   nextCount       = 0;
                    int   nextPenetration = 0;
                    foreach (var VARIABLE in data.levelUpList[level].levelUpData)
                    {
                        if (VARIABLE.levelUpType == ItemData.LevelUpType.Damage)    // 곱
                        {
                            nextDamage = VARIABLE.increaseValue;
                        }
                        else if (VARIABLE.levelUpType == ItemData.LevelUpType.Rate) // 곱
                        {
                            nextRate = VARIABLE.increaseValue;
                        }
                        else if (VARIABLE.levelUpType == ItemData.LevelUpType.Range) // 곱
                        {
                            nextRange =VARIABLE.increaseValue;
                        }
                        else if (VARIABLE.levelUpType == ItemData.LevelUpType.Count)       // 합
                        {
                            nextCount = (int)VARIABLE.increaseValue;
                        }
                        else if (VARIABLE.levelUpType == ItemData.LevelUpType.Penetration) // 합
                        {
                            nextPenetration = (int)VARIABLE.increaseValue;
                        }
                    }
                    weapon.WeaponLevelUp(nextDamage,nextRate, nextRange,nextCount,nextPenetration);
                    level++;
                }
                break;
                
            case ItemData.ItemType.Accessories:
                // 처음 획득(캐릭터 선택 or 아이템 선택) - > 새로 생성
                if (!isNewCreate && level == 0)
                {
                    isNewCreate = true;
                    GameObject newAccessories = new GameObject();
                    accessories = newAccessories.AddComponent<Accessories>();
                    accessories.Init(data);
                    level++; // 악세사리는 베이스 값이 없기 때문에, 초기화 때, levelUpList[0].levelUpData[0]번 값을 넣어 주면서 시작. 그래서, 레벨을 올려줘야 함.
                }
                // 이미 보유 -> 레벨 업
                else
                {
                    float nextRate  = 0;
                    int   nextCount = 0;
                    foreach (var VARIABLE in data.levelUpList[level].levelUpData)
                    {
                        
                        if (VARIABLE.levelUpType == ItemData.LevelUpType.Rate)       // 곱
                        {
                            nextRate = VARIABLE.increaseValue;
                        }
                        else if (VARIABLE.levelUpType == ItemData.LevelUpType.Count) // 합
                        {
                            nextCount = (int)VARIABLE.increaseValue;
                        }
                    }
                    accessories.LevelUp(nextRate,nextCount);
                    level++;
                }
                break;
                
            case ItemData.ItemType.Consumption:
                GameManager.instance.playerCurrentHealth = GameManager.instance.playerCurrentMaxHealth;
                break;
        }
        
        // 만렙이 된 경우, 비활성화 시키기(소비 제외)
        if (level == data.levelUpList.Count && data.itemType != ItemData.ItemType.Consumption)
            GetComponent<Button>().interactable = false;
            
        // 스킬 선택(스킬 패널이 켜져 있고, 레벨업해서 5레벨이 된 경우)
        if (skillPanel.activeSelf && level == 5)
        {
            // 0번 버튼부터 확인하여, 비어있는 칸에 데이터 넣어주기
            foreach (var VARIABLE in IngameUI.instance.playerSkillList)
            {
                if (!VARIABLE.isMounting)
                {
                    VARIABLE.SkillMounting(currentSkillData,weapon);
                    break;
                }
            }
        }
    }
}

