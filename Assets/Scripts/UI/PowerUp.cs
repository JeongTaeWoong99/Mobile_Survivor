using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PowerUp : MonoBehaviour
{
	public PowerUpData data;

	// 플레이어프리팹 데이터 생성 및 받아오기
	[HideInInspector] public int level;
	
	// 세팅(개인 패널에서 사용)
	public Text  powerUpName; 
	public Image icon;			// 공통(개인+공용)
	
	public Sprite  rankOnImage;	// On 별 이미지
	public Image[] ranks;		// 그룹 별 이미지
	
	// 세팅(공용 패널에서 사용)
	[HideInInspector] public int    id;
	[HideInInspector] public string desc;
	[HideInInspector] public int[]  needGold;
	
	private void Awake()
	{
		// 세팅 개인
		icon.sprite      = data.sprite;
		powerUpName.text = data.powerUpName;
		
		// 세팅 공용
		desc     = data.desc;
		needGold = data.needGold;
		
		// 레벨 확인
		id = data.id;
		if (!PlayerPrefs.HasKey("PowerUp" + id + "Level"))
		{
			PlayerPrefs.SetInt("PowerUp" + id + "Level", 0);	// 레벨 0 설정
			level = 0;
		}
		else
		{
			level = PlayerPrefs.GetInt("PowerUp" + id + "Level");
			
			// 현재, 레벨에 따라, 별 활성화
			for (int i = 0; i < level; i++)
				ranks[i].sprite = rankOnImage;
		}
		
		// 업그레이드 가능에 맞춰서, 랭크 별 활성화
		for (int i = 0; i < ranks.Length; i++)
			ranks[i].gameObject.SetActive(i < needGold.Length);
	}

	public void OnClick()
	{
		AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
	
		// 디테일 패널이 활성화 되어 있지 않으면, 활성화
		if (!MainUI.instance.detailsPanel.activeSelf)
			MainUI.instance.detailsPanel.SetActive(true);
		
		MainUI.instance.currentPowerUp = this;
		
		MainUI.instance.detailsIcon.sprite  = icon.sprite;
		MainUI.instance.detailsText[0].text = powerUpName.text;
		MainUI.instance.detailsText[1].text = desc;
		
		// 만렙 O
		if(level == needGold.Length)
		{
			// 구매 불가
			MainUI.instance.buyButton.interactable = false;
			
			MainUI.instance.detailsText[2].text = "MAX";
		}
		// 만렙 X
		else
		{
			// 구매 가능하면, 구매 버튼 활성활
			MainUI.instance.buyButton.interactable = PlayerPrefs.GetInt("GoldData") >= needGold[level];
			
			MainUI.instance.detailsText[2].text = needGold[level].ToString();
		}
	}
}
