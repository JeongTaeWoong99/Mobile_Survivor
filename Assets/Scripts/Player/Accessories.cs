using System;
using UnityEngine;

public class Accessories : MonoBehaviour
{
    [HideInInspector] public int accessoriesId;
    
    [HideInInspector] public float rate; // 현재값
    private float previousRate = 0;      // 이전값 저장
    
    [HideInInspector] public int count;  // 현재값

    public void Init(ItemData data)
    {
        // 세팅
        gameObject.name         = "Accessories " + data.name;
        accessoriesId           = data.itemId;
        transform.parent        = Player.instance.transform;
        transform.localPosition = Vector3.zero;
        rate                    = data.levelUpList[0].levelUpData[0].increaseValue;      // 악세사리는 베이스 값이 없기 때문에, 초기화 때, levelUpList[0].levelUpData[0]번 값을 넣어 주면서 시작. 그래서, 레벨을 올려줘야 함.
        count                   = (int)data.levelUpList[0].levelUpData[0].increaseValue; // 동일
        
        ApplyAccessories();
    }

    public void LevelUp(float rate,int count)
    {
        previousRate = this.rate;   // 이전 값 저장
        this.rate  = rate;
        this.count = count;
        
        ApplyAccessories();
    }
    
    // 플레이어에게 적용
    private void ApplyAccessories()
    {
        switch (accessoriesId) 
        {
            case 0:
                SpeedUp();
                break;
            case 1:
                ExpUp();
                break;
            case 2:
                MaxHpUp();
                break;
            case 3:
                TotalAttackRateInPerUp();
                break;
            case 4:
                PerHpUp();
                break;
            case 5:
                AcquisitionRangeInPerUp();
                break;
            case 6:
                TotalAttackDamageInPerUp();
                break;
        }
    }
    
    // 이속 퍼 교체
    private void SpeedUp()
    {
        //Debug.Log("이속 전 : " + GameManager.instance.moveSpeedInPer);
        GameManager.instance.moveSpeedInPer = rate + 100f;  // 교체
        //Debug.Log("이속 후 : " + GameManager.instance.moveSpeedInPer);
    }
    
    // 겸치 퍼 교체
    private void ExpUp()
    {
        //Debug.Log("겸치 전 : " + GameManager.instance.expInPer);
        GameManager.instance.expInPer = rate + 100f;    // 교체
        //Debug.Log("겸치 후 : " + GameManager.instance.expInPer);
    }
    
    // MaxHp 퍼 교체
    private void MaxHpUp()
    {
        //Debug.Log("멕스체력 전 : " + GameManager.instance.maxHpInPer + " / " + GameManager.instance.playerCurrentMaxHealth);
        GameManager.instance.maxHpInPer = rate + 100f;
        GameManager.instance.playerCurrentMaxHealth = (GameManager.instance.playerBaseMaxHealth * (GameManager.instance.maxHpInPer / 100f)) + CharacterPassive.PowerUp0();
        //Debug.Log("멕스체력 후 : " + GameManager.instance.maxHpInPer + " / " + GameManager.instance.playerCurrentMaxHealth);
    }
    
    // PerHp 퍼 교체
    private void PerHpUp()
    {
        GameManager.instance.perHpIn = count;   // 교체
        //Debug.Log(GameManager.instance.perHpIn);
    }
    
    // 획득반경 퍼 교체
    private void AcquisitionRangeInPerUp()
    {
        GameManager.instance.acquisitionRangeInPer = rate + 100f;
    }
    
    // 토탈 공속 퍼 교체
    private void TotalAttackRateInPerUp()
    {
        GameManager.instance.totalAttackRateInPer += rate - previousRate;
        Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();
        foreach (var VARIABLE in weapons)
            VARIABLE.AccessoriesAndSkillToWeaponApply();
    }
    
    // 토탈 공격력 퍼 교체
    private void TotalAttackDamageInPerUp()
    {
        GameManager.instance.totalAttackDamageInPer += rate - previousRate;
        Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();
        foreach (var VARIABLE in weapons)
            VARIABLE.AccessoriesAndSkillToWeaponApply();
    }
}
