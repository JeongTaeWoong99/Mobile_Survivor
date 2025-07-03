using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptble Object/SkillData")]
public class SkillData : ScriptableObject
{
	public enum SkillType { Passive, Active, Both } 

	[Header("----- Weapon Info -----")]
	public int       skillId;  
	public Sprite    skillIcon;
	public string    skillName;
	public SkillType skillType;
	[TextArea] 
	public string    skillDesc;

	[Header("----- Object -----")] 
	public GameObject[] skillObject;
	
	[Header("----- Value -----")]
	public List<float> skillRatePer;
	public float       skillCooldown;
	public float       skillDamageDuration;
	public float       stunTime;
	public bool        isBossDamageBoost;
	public int         stack;
}
