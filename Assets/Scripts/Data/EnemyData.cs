using UnityEngine;

[System.Serializable]
public class Skill
{
    public int skillID;
    public int skillDelay;
}

[System.Serializable]
public class Gold
{
    public int   dropRate;
    public int[] minMaxDrop;
}

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptble Object/EnemyData")]
public class EnemyData : ScriptableObject
{
    public enum EnemyType {NonAttack, Melee, Range, Elite,Boss}
    public enum AttackType {Melee,Range,Skill}
    public enum BodyType {small,big}
    
    [Header("----- Info -----")]
    public int        enemyID;  
    public string     enemyName;
    public EnemyType  enemyType;
    public BodyType   bodyType;
    public int        rank;     
    public int[]      minMaxEXP;
    public Gold       gold;
    
    [Header("----- Effect -----")]      
    //public GameObject[] chargingEffect; // 차징할 때, 자기 자신이 사용하는 이펙트(반짝임 or 기 모으기 등등)
    public GameObject[] waringEffect;   
    public GameObject[] attackEffect;   
    
    [Header("----- Value -----")] 
    public int     maxHealth;               
    public int     collisionDamage;         
    public float   speed;         
    public float[] attackDamage;  
    public float[] attackRate ;   
    public float[] attackMoveSpeed; // 공격할 때, 이동하는 속도
    public float   attackRange;     // 공격 범위(근거리는 애니메이션에 맞춤 / 원거리는 CircleCollider2D에 적용)
    
    [Header("----- Elite/Boss -----")] 
    public AttackType[] attackType;
    public float[]      enrageBuffValue; // 지속 시간 0 // 공격 퍼센트 1 // 이속 퍼센트 2
    public float[]      shieldBuffValue; // 지속 시간 0 // 멕스 쉴드양 1
}