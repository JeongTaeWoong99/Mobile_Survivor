using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptble Object/ItemData")]
public class ItemData : ScriptableObject
{
    public enum ItemType { Melee, Range, Accessories, Consumption }   // 아이템 타입
    public enum LevelUpType {Damage, Rate, Count, Penetration, Range} // 레벨업 타입(어떤 종류의 값이 업그레이드 되는지)
    
    [Header("----- Info -----")]
    public int      itemId;  
    public Sprite   itemIcon;
    public string   itemName;
    public ItemType itemType;
    public string   desc;         // 0레벨 아이템 설명

    [Header("----- Base Value -----")]
    public float baseDamage;      // 기본 데미지
    public float baseRate;        // 기본 퍼센트 증가률(발사률 / 악세서리 증가률 / 회전 범위 / )
    public float baseRange;       // 공격 범위(칼 회전 )
    public int   baseCount;       // 초기 보유 갯수(칼 같은 경우, 0레벨에 3개의 삽이 회전함 / 활의 경우, 0레벨 1발 발사)
    public int   basePenetration; // 관통력(근접류 무한 = -100 / 원거리류는 숫자)

    [Header("----- Weapon Object-----")]
    public GameObject[] bullet;   // Weapon에서 bulletId 찾기(오브젝트 풀링)
    //public Sprite     hand;     // Weapon에서 알맞은 손 찾기 및 활성화
    
    [Header("----- LevelUp -----")]
    public List<LevelUp> levelUpList;

    [Header("----- Skill -----")]
    public List<SkillData> selectableSkillList;
}

[System.Serializable]
public class LevelUp
{
    public List<LevelUpData> levelUpData;
}

[System.Serializable]
public class LevelUpData
{
    public ItemData.LevelUpType levelUpType;
    public float                increaseValue;
}

