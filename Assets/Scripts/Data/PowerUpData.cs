using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PowerUp", menuName = "Scriptble Object/PowerUpData")]
public class PowerUpData : ScriptableObject
{
	// 개인 패널 데이터
	public int    id;
	public Sprite sprite;
	public string powerUpName;
	
	// 공용 패널 데이터
	public string desc;
	public int[]  needGold;
}
