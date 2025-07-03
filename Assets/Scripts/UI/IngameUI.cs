using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class IngameUI : MonoBehaviour
{
	public static IngameUI instance;

	public GameObject       hud;
	public GameObject       levelUpItemSelect;
	public Transform        uiJoy;
	public GameObject       skillButtonPanel;
	public List<GameObject> statusWindows;	// 승리, 패배, 퍼즈
	public GameObject       bossHealthSlider;
	public GameObject       killUI;

	private Item[] items;
	[HideInInspector] public PlayerSkill[] playerSkillList;

	private void Awake()
	{
		instance = this;
		
		// 비활성화 되어 있는 컴포넌트도 받을 수 있음.
		items           = GetComponentsInChildren<Item>(true);		 
		playerSkillList = GetComponentsInChildren<PlayerSkill>(true);
	}

	private void Start()
	{
		hud.SetActive(false);
		skillButtonPanel.gameObject.transform.localScale = Vector3.zero; // 코루틴이 꺼지지 않도록, 크기를 줄임....!
		uiJoy.localScale                                 = Vector3.zero; // 코루틴이 꺼지지 않도록, 크기를 줄임....!
		levelUpItemSelect.SetActive(false);
		foreach (var resultTitles in statusWindows)
			resultTitles.SetActive(false);
		bossHealthSlider.SetActive(false);
		killUI.gameObject.SetActive(false);
	}

	private void Update()
	{
#if UNITY_EDITOR
			// 레벨업
			if (Input.GetKeyDown(KeyCode.F1))
				Show();
			
			// // 재시작
			// if (Input.GetKeyDown(KeyCode.F5))
			// 	SceneManager.LoadScene(0);
	        
			// // 게임 승리 종료
			// if (Input.GetKeyDown(KeyCode.F3))
			// 	SpawnManager.instance.isBossDie = true;
				
			// 골드 증가
			if (Input.GetKeyDown(KeyCode.F7))
			{
				GameManager.instance.currentGold += 1000;
				PlayerPrefs.SetInt("GoldData", GameManager.instance.currentGold);
			}
#endif
	
	}

	public void Win()
	{
		statusWindows[0].SetActive(true);
	}
	
	public void Lose()
	{
		statusWindows[1].SetActive(true);
	}
	
	public void Pause(bool isSee)
	{
		AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
	
		statusWindows[2].SetActive(isSee);
		if (isSee)
			GameManager.instance.Stop();
		else
			GameManager.instance.Resume();
	}

	public void ReturnMain()
	{
		StartCoroutine(ReturnMainCo());
	}
	
	private IEnumerator ReturnMainCo()
	{
		AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
	
		MainUI.instance.FadeIn();
        
		// AudioManager.instance.PlayBgm(true);
		// AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        
		yield return new WaitForSecondsRealtime(2);
	
		SceneManager.LoadScene(0);
	}
	
    // 기본 무기 지급(캐릭터 선택 후)
    public void ProvidesBasicWeapons(int index)
    {
        items[index].OnClick();
    }
    
    // 아이템 보이기
    private void CreateSelection()
	{
	    // 모든 아이템 선택 비활성화
	    foreach (Item item in items)
		    item.gameObject.SetActive(false); // 모든 아이템 비활성화

	    // 랜덤으로 선택된 아이템 리스트
	    List<int> randomNums = new List<int>();
	
	    // 무기/악세사리 리스트와 소모품 리스트 분리
	    List<int> weaponOrAccessoryList = new List<int>(); // 무기/악세사리 리스트
	    List<int> consumableList        = new List<int>(); // 소모품 리스트
	
	    for (int i = 0; i < items.Length; i++)
	    {
	        //Debug.Log(items[i].name + "의 타입은 " + items[i].data.itemType);
	        // 무기 또는 악세사리이며, 만렙이 아닌 경우
	        if ((items[i].data.itemType == ItemData.ItemType.Melee || items[i].data.itemType == ItemData.ItemType.Range || items[i].data.itemType == ItemData.ItemType.Accessories) 
	            && items[i].level != items[i].data.levelUpList.Count)
	        {
	            weaponOrAccessoryList.Add(i);
	            //Debug.Log(i +"가 리스트에 들어감.");
	            //Debug.Log($"무기/악세사리 = {items[i].gameObject.name}"+ " / 현재 레벨 = " + items[i].level);
	        }

	        if (items[i].data.itemType != ItemData.ItemType.Consumption) 
				continue;
	        consumableList.Add(i);
	    }
	
	    // 만렙이 아닌 무기/악세사리가 3개 이상인 경우
	    if (weaponOrAccessoryList.Count >= 3)
	    {
	        //Debug.Log("만렙이 아닌 무기/악세사리가 3개 이상입니다. 무기/악세사리에서 선택합니다.");
	        while (randomNums.Count < 3)
	        {
	            int randomIndex = Random.Range(0, weaponOrAccessoryList.Count);
	            int selected = weaponOrAccessoryList[randomIndex];
	            if (!randomNums.Contains(selected)) // 중복 방지
	                randomNums.Add(selected);
	        }
	    }
	    else
	    {
	        //Debug.Log("만렙이 아닌 무기/악세사리가 3개 미만입니다. 소모품을 추가합니다.");
	        // 무기/악세사리 먼저 추가
	        //Debug.Log("카운트 = " + weaponOrAccessoryList.Count);
	        if(weaponOrAccessoryList.Count != 0)
				randomNums = weaponOrAccessoryList;
		
	        // 소모품에서 랜덤 추가(중복됨...!)
	        foreach (var consumableLists in consumableList)
	        {
	            int selected = consumableLists;
	            if (!randomNums.Contains(selected)) // 중복 방지
	                randomNums.Add(selected);
	        }
	    }
			
	    // 선택된 아이템 활성화
	    foreach (int ran in randomNums)
	    {
	        //Debug.Log($"아이템 활성화: {items[ran].gameObject.name}");
	        //Debug.Log("활성화 = " + ran);
	        items[ran].gameObject.SetActive(true);
	    }
	}

	public void Show()
    {
        CreateSelection();
        levelUpItemSelect.gameObject.SetActive(true);
        GameManager.instance.Stop();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

	// 아이템 선택 시, 숨기기 버튼
    public void Hide()
    {
	    levelUpItemSelect.gameObject.SetActive(false);
        GameManager.instance.Resume();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        AudioManager.instance.EffectBgm(false);
    }
}
