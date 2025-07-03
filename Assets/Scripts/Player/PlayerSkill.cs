using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerSkill : MonoBehaviour
{
	private Button button;

	[HideInInspector] public SkillData data;	// 스킬 정보
	[HideInInspector] public Weapon    weapon;	// 스킬의 무기의 실시간 정보를 받아와 사용
	
	public Image skillIconImage;
	public Image hideImage;
	public Text  remainTimeText;
	public Text  remainStackText;
	
	public  Material[] bulletMaterials;
	private int        innerOutLineID;
	
	[HideInInspector] public bool isMounting; // 버튼에 스킬이 선택되어, 장착된 경우 -> TRUE

	private bool  isSkillOperation;		// 스킬 작동 중 체크
	private int[] prefabId;
	private float skillCooldownTimer;
	//private float skillDurationTimer;
	private bool  isStackCheckProgress;
	private int   remainStack;

	private void Awake()
	{
		button = GetComponent<Button>();
		
		innerOutLineID = Shader.PropertyToID("_InnerOutlineFade");
		foreach (var VARIABLE in bulletMaterials)
			VARIABLE.SetFloat(innerOutLineID,0f);
	}

	private void Start()
	{
		button.interactable = false;
	
		skillIconImage.gameObject.SetActive(false);
		hideImage.gameObject.SetActive(false);
		remainTimeText.gameObject.SetActive(false);
		remainStackText.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (!isMounting) 
			return;

		// 일반형
		if (data.stack == 0)
		{
			if (!isSkillOperation)
			{
				skillCooldownTimer -= Time.deltaTime;
			}
		
			// 사용 가능
			if (skillCooldownTimer <= 0 && !isSkillOperation)
			{
				if (!button.interactable) button.interactable = true;

				if (hideImage.gameObject.activeSelf)      hideImage.gameObject.SetActive(false);
				if (remainTimeText.gameObject.activeSelf) remainTimeText.gameObject.SetActive(false);
			}
			// 사용 불가
			else
			{
				if (button.interactable) button.interactable = false;

				if (!remainTimeText.gameObject.activeSelf) remainTimeText.gameObject.SetActive(true);
				if (!hideImage.gameObject.activeSelf)      hideImage.gameObject.SetActive(true);

				hideImage.fillAmount = skillCooldownTimer / (data.skillCooldown / 1000);
				remainTimeText.text  = isSkillOperation ? "작동 중" : ((int)skillCooldownTimer).ToString();
			}
		}
		// 스택형
		else
		{
			// 풀 스택
			if (remainStack == data.stack)
			{
				if (!button.interactable) button.interactable = true;

				if (hideImage.gameObject.activeSelf)      hideImage.gameObject.SetActive(false);
				if (remainTimeText.gameObject.activeSelf) remainTimeText.gameObject.SetActive(false);
				//Debug.Log("풀스택");
			}
			// 스택 충전 가능
			else if(remainStack > 0)
			{
				if (!button.interactable) button.interactable = true;
				
				if (!remainTimeText.gameObject.activeSelf) remainTimeText.gameObject.SetActive(true);
				if (!hideImage.gameObject.activeSelf)      hideImage.gameObject.SetActive(true);
				//Debug.Log("스택 존재");
			}
			// 스택 없음
			else if (remainStack == 0)
			{
				if (!button.interactable) button.interactable = false;
				
				if (!remainTimeText.gameObject.activeSelf) remainTimeText.gameObject.SetActive(true);
				if (!hideImage.gameObject.activeSelf)      hideImage.gameObject.SetActive(true);
				//Debug.Log("스택 없음");
			}
			
			// 스택 관리			
			// 풀 스택
			if (remainStack == data.stack)
			{
				skillCooldownTimer = data.skillCooldown / 1000;
				//Debug.Log("풀스택. 쿨타임 초기화");
			}
			// 스택 충전 가능 + 스택 없음
			else if (remainStack >= 0)
			{
				skillCooldownTimer -= Time.deltaTime;
				//Debug.Log("skillCooldownTimer 감소");
				if (skillCooldownTimer <= 0)
				{
					remainStack++;
					//Debug.Log("스택 증가");
					if (remainStack != data.stack)
					{
						//Debug.Log("스택 더 보유 가능. 쿨타임 초기화");
						skillCooldownTimer = data.skillCooldown / 1000;
					}
				}
			}
			
			hideImage.fillAmount = skillCooldownTimer / (data.skillCooldown / 1000);
			remainTimeText.text  = ((int)skillCooldownTimer).ToString();
			remainStackText.text = remainStack.ToString();
		}
	}

	// 스킬 장착
	public void SkillMounting(SkillData data, Weapon weapon)
	{
		// 안전성 검사: 기본 검증
		if (data == null)
		{
			Debug.LogError("PlayerSkill: SkillMounting - SkillData가 null입니다!");
			return;
		}
		
		if (weapon == null)
		{
			Debug.LogError("PlayerSkill: SkillMounting - Weapon이 null입니다!");
			return;
		}
		
		// 안전성 검사: PoolManager 초기화 확인
		if (PoolManager.instance == null || PoolManager.instance.prefabs == null)
		{
			Debug.LogError("PlayerSkill: PoolManager가 초기화되지 않았습니다!");
			return;
		}
		
		// 안전성 검사: skillObject 배열 확인
		if (data.skillObject == null || data.skillObject.Length == 0)
		{
			Debug.LogWarning($"PlayerSkill: '{data.name}'의 skillObject 배열이 비어있습니다.");
			// 배열이 비어있어도 스킬은 마운트할 수 있도록 함
		}
		
		this.data   = data;
		this.weapon = weapon;
		
		isMounting = true;
		
		skillIconImage.gameObject.SetActive(true);
		
		button.interactable = true;
		
		skillIconImage.sprite = data.skillIcon;

		if (data.stack != 0)
		{
			remainStackText.gameObject.SetActive(true);
			remainStack = data.stack;
		}

		// 프리팹 아이디 받아오기(찾기 자동화)
		prefabId = new int[data.skillObject.Length];
		
		// 모든 요소를 -1로 초기화 (찾지 못했음을 표시)
		for (int i = 0; i < prefabId.Length; i++)
		{
			prefabId[i] = -1;
		}
		
		// PoolManager의 prefabs 배열에서 skillObject 찾기
		for (int index = 0; index < PoolManager.instance.prefabs.Length; index++)
		{
			if (PoolManager.instance.prefabs[index] == null) continue;
			
			for (int num = 0; num < data.skillObject.Length; num++)
			{
				if (data.skillObject[num] != null && data.skillObject[num] == PoolManager.instance.prefabs[index])
				{
					prefabId[num] = index;
					break;
				}
			}
		}
		
		// prefabId 검증 및 경고
		for (int i = 0; i < prefabId.Length; i++)
		{
			if (prefabId[i] == -1)
			{
				Debug.LogWarning($"PlayerSkill: '{data.name}'의 skillObject[{i}]를 PoolManager에서 찾을 수 없습니다.");
			}
		}

		if (data.skillType == SkillData.SkillType.Passive || data.skillType == SkillData.SkillType.Both)
			Passive();
	}
	
	// 스킬 클릭
	public void OnClick()
	{
		AudioManager.instance.PlaySfx(AudioManager.Sfx.SkiilUse);
			 if (data.skillId == 0)  FistPress();
		else if (data.skillId == 1)  StartCoroutine(PiercingFist());
		else if (data.skillId == 2)  StraightPunch();
		else if (data.skillId == 3)  StartCoroutine(RapidFire());
		else if (data.skillId == 4)  ExplosiveArrow();
		else if (data.skillId == 5)  EvasiveManeuver();
		else if (data.skillId == 6)  DeathSpear();
		else if (data.skillId == 7)  OneHitKilling();
		else if (data.skillId == 8)  SixMonthStrike();
		else if (data.skillId == 9)  StartCoroutine(SwordsRotation());
		else if (data.skillId == 10) StartCoroutine(HailOfBlades());
		else if (data.skillId == 11) StartCoroutine(SoulSword());
	}

	private void Passive()
	{
		// 피스트 프레스
		if (data.skillId == 1)
			weapon.WeaponLevelUp(0,0,0,0,1); // 기본 관통력 1 증가
		// 속사
		else if (data.skillId == 3)
		{
			weapon.WeaponLevelUp(0,data.skillRatePer[0],0,0,0); // 무기퍼에 더하기
		}
		// 폭발 화살
		else if (data.skillId == 4)
		{
			weapon.skillData          = data;      // 데이터 넣어주기
			weapon.explosiveArrowPer += 5;	       // 일반 공격 중, 5%로 폭발 화살 발사
			weapon.skillObjectId      = prefabId[1];
		}
	}
	
	// 스킬 0 피스트 프레스
	private void FistPress()
	{
		// 주먹 생성(화면에 1명 이상의 적이 있음)
		GameObject findEnemy = FindHighestTypeEnemy();
		if (findEnemy && IsValidPrefabIndex(0))
		{
			GameObject bullet = PoolManager.instance.GetWithPosition(prefabId[0], findEnemy.transform.position);
			if (bullet != null)
			{
				SkillFunction skillComponent = bullet.GetComponent<SkillFunction>();
				if (skillComponent != null && data != null && data.skillRatePer != null && data.skillRatePer.Count > 0)
				{
					skillComponent.Init(weapon.finalCalculationDamage * (data.skillRatePer[0] / 100), data.stunTime, data.isBossDamageBoost);
				}
			}
			
			skillCooldownTimer = data.skillCooldown / 1000;
		}
		else
			Debug.Log("공격 가능한 적이 없음.");
	}
	
	// 스킬 1 피어싱 피스트
	private IEnumerator PiercingFist()
	{
		skillCooldownTimer = data.skillCooldown / 1000;
		
		isSkillOperation = true;
		Player.instance.activeBuffCount += 1;
		
			if (data != null && data.skillRatePer != null && data.skillRatePer.Count > 0)
	{
		weapon.WeaponLevelUp(data.skillRatePer[0],0,0,0,0); // 무기퍼에 더하기
		}
		
		if (bulletMaterials != null && bulletMaterials.Length > 0 && bulletMaterials[0] != null)
		{
			bulletMaterials[0].SetFloat(innerOutLineID,0.5f);
		}
	
		yield return new WaitForSeconds(data.skillDamageDuration / 1000f);
		
		isSkillOperation = false;
		Player.instance.activeBuffCount -= 1;
		
			if (data != null && data.skillRatePer != null && data.skillRatePer.Count > 0)
	{
		weapon.WeaponLevelUp(-data.skillRatePer[0],0,0,0,0); // 복구
		}
		
		if (bulletMaterials != null && bulletMaterials.Length > 0 && bulletMaterials[0] != null)
		{
			bulletMaterials[0].SetFloat(innerOutLineID,0f);
		}
	}
	
	// 스킬 2 스트레이트 펀치
	private void StraightPunch()
	{
		if (!IsValidPrefabIndex(0)) return;
		
		Vector3 bulletPosition;
		Vector3 bulletScale;
		
		if (Player.instance.spriteRen.flipX)
		{
			bulletPosition = Player.instance.transform.position + new Vector3(3,0,0);
			bulletScale = new Vector3(-1,1,1);
		}
		else
		{
			bulletPosition = Player.instance.transform.position + new Vector3(-3,0,0);
			bulletScale = new Vector3(1,1,1);
		}
		
		GameObject bullet = PoolManager.instance.GetWithPosition(prefabId[0], bulletPosition);
		if (bullet != null)
		{
			bullet.transform.localScale = bulletScale;
			
			SkillFunction skillComponent = bullet.GetComponent<SkillFunction>();
			if (skillComponent != null && data != null && data.skillRatePer != null && data.skillRatePer.Count > 0)
			{
				skillComponent.Init(weapon.finalCalculationDamage * (data.skillRatePer[0] / 100), data.stunTime, data.isBossDamageBoost);
			}
		}
			
		skillCooldownTimer = data.skillCooldown / 1000;
	}
	
	// 스킬 3 속사
	private IEnumerator RapidFire()
	{
		skillCooldownTimer = data.skillCooldown / 1000;
			
		isSkillOperation = true;
		Player.instance.activeBuffCount += 1;
		
			if (data != null && data.skillRatePer != null && data.skillRatePer.Count >= 2)
	{
		weapon.WeaponLevelUp(0,data.skillRatePer[1] - data.skillRatePer[0],0,0,0); // 무기퍼에 더하기
		}
		weapon.shortenedLaunchInterval = true;	// 발사 간격 단축
		
		yield return new WaitForSeconds(data.skillDamageDuration / 1000f);
		
		isSkillOperation = false;
		Player.instance.activeBuffCount -= 1;
		
			if (data != null && data.skillRatePer != null && data.skillRatePer.Count >= 2)
	{
		weapon.WeaponLevelUp(0,-(data.skillRatePer[1] - data.skillRatePer[0]),0,0,0); // 무기퍼에 더하기
		}
		weapon.shortenedLaunchInterval = false;	// 발사 간격 복구
	}
	
	// 스킬 4 폭발 화살
	private void ExplosiveArrow()
	{
		if (!IsValidPrefabIndex(0)) return;
		
		for (int index = 0; index < 10; index++) // 10발 발사
		{
			Vector3 randomPosition = GetRandomPositionInCameraView();
			GameObject bullet = PoolManager.instance.GetWithPosition(prefabId[0], randomPosition);
			if (bullet != null)
			{
				SkillFunction skillComponent = bullet.GetComponent<SkillFunction>();
				if (skillComponent != null && data != null && data.skillRatePer != null && data.skillRatePer.Count >= 2)
				{
					skillComponent.Init(weapon.finalCalculationDamage * (data.skillRatePer[1] / 100), data.stunTime, data.isBossDamageBoost);
				}
			}
		}
		skillCooldownTimer = data.skillCooldown / 1000;
	}
	
	// 스킬 5 회피 기동
	private void EvasiveManeuver()
	{
		if (!IsValidPrefabIndex(0)) return;
		
		Player.instance.isSkillProgress = true;
		Player.instance.anim.SetTrigger("BackFlip");
		
		Vector3 bulletPosition;
		if (Player.instance.spriteRen.flipX)
			bulletPosition = Player.instance.transform.position + Vector3.left * 2;
		else
			bulletPosition = Player.instance.transform.position + Vector3.right * 2;
		
		GameObject bulletObj = PoolManager.instance.GetWithPosition(prefabId[0], bulletPosition);
		if (bulletObj != null)
		{
			Transform bullet = bulletObj.transform;

					PlayerBullet bulletComponent = bullet.GetComponent<PlayerBullet>();
		if (bulletComponent != null && data != null && data.skillRatePer != null && data.skillRatePer.Count > 0)
		{
			bulletComponent.Init(weapon.finalCalculationDamage * (data.skillRatePer[0] / 100), -100, 0, Vector3.zero, false,0f);
			}
		}

		skillCooldownTimer = data.skillCooldown / 1000;
	}
	
	// 스킬 6 데스 스피어
	private void DeathSpear()
	{
		// 주먹 생성(화면에 1명 이상의 적이 있음)
		GameObject findEnemy = FindHighestTypeEnemy();
		if (findEnemy && IsValidPrefabIndex(0))
		{
			GameObject bullet = PoolManager.instance.GetWithPosition(prefabId[0], findEnemy.transform.position);
			if (bullet != null)
			{
				SkillFunction skillComponent = bullet.GetComponent<SkillFunction>();
				if (skillComponent != null && data != null && data.skillRatePer != null && data.skillRatePer.Count > 0)
				{
					skillComponent.Init(weapon.finalCalculationDamage * (data.skillRatePer[0] / 100), data.stunTime, data.isBossDamageBoost);
				}
			}
			
			skillCooldownTimer = data.skillCooldown / 1000;
		}
		else
			Debug.Log("공격 가능한 적이 없음.");
	}
	
	// 스킬 7 일격필살
	private void OneHitKilling()
	{
		if (remainStack < 1 || !IsValidPrefabIndex(0))
			return;
	
		GameObject findEnemy = FindHighestTypeEnemy();
		// + new Vector3(0, 3, 0)
		if (findEnemy)
		{
			Vector3 targetPos = findEnemy.transform.position;
			Vector3 dir       = targetPos - Player.instance.transform.position;
			dir               = dir.normalized;
			
			GameObject bulletObj = PoolManager.instance.Get(prefabId[0]);
			if (bulletObj != null)
			{
				Transform bullet = bulletObj.transform;
				bullet.position = Player.instance.transform.position;
						bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
		
		PlayerBullet bulletComponent = bullet.GetComponent<PlayerBullet>();
		if (bulletComponent != null && data != null && data.skillRatePer != null && data.skillRatePer.Count > 0)
		{
			bulletComponent.Init(weapon.finalCalculationDamage * (data.skillRatePer[0] / 100), -100, 0, dir, true, 0,30);
		}
			}
			
			remainStack--;
		}
		else
		{
			Debug.Log("공격 가능한 적이 없음.");
		}
	}
	
	// 스킬 8 육월참
	private void SixMonthStrike()
	{
		if (!IsValidPrefabIndex(0)) return;
		
		for (int index = 0; index < 6; index++) // 6발 발사
		{
			// 360도를 6개로 나누어 각도를 계산
			float angle  = 360f / 6 * index;
			float radian = angle * Mathf.Deg2Rad; // 각도를 라디안으로 변환

			// 방향 계산 (자신 기준으로 360도)
			Vector3 dir = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0).normalized;
			
			GameObject bulletObj = PoolManager.instance.GetWithPosition(prefabId[0], Player.instance.transform.position);
			if (bulletObj != null)
			{
				Transform bullet = bulletObj.transform;
				bullet.rotation  = Quaternion.FromToRotation(Vector3.up, dir);
		
		PlayerBullet bulletComponent = bullet.GetComponent<PlayerBullet>();
		if (bulletComponent != null && data != null && data.skillRatePer != null && data.skillRatePer.Count > 0)
		{
			bulletComponent.Init(weapon.finalCalculationDamage * (data.skillRatePer[0] / 100), -100, 0, dir, true, 0,30);
		}
			}
		}
		
		skillCooldownTimer = data.skillCooldown / 1000;
	}
	
	// 스킬 9 검의 공전
	private IEnumerator SwordsRotation()
	{
		skillCooldownTimer = data.skillCooldown / 1000;
		
		isSkillOperation  = true;
		Player.instance.activeBuffCount += 1;
		
		weapon.skillCount = weapon.baseCount + weapon.countIn;
		weapon.WeaponLevelUp(0,0,0,0,0); // 재배치 한번 해주기
		
		if (bulletMaterials != null && bulletMaterials.Length > 3 && bulletMaterials[3] != null)
		{
			bulletMaterials[3].SetFloat(innerOutLineID,0.5f);
		}

		yield return new WaitForSeconds(data.skillDamageDuration / 1000f);
		
		isSkillOperation = false;
		Player.instance.activeBuffCount -= 1;
		
		weapon.skillCount = 0;
		weapon.WeaponLevelUp(0,0,0,0,0); // 재배치 한번 해주기
		
		if (bulletMaterials != null && bulletMaterials.Length > 3 && bulletMaterials[3] != null)
		{
			bulletMaterials[3].SetFloat(innerOutLineID,0f);
		}
	}
	
	// 스킬 10 칼날비 
	private IEnumerator HailOfBlades()
	{
		skillCooldownTimer = data.skillCooldown / 1000;
		
		Vector3 createVec3;
		Vector3 localScaleVec3;
		Vector3 dir;
	
		if (Player.instance.spriteRen.flipX)
		{
			createVec3     = Player.instance.transform.position + new Vector3(3,0,0);
			localScaleVec3 = new Vector3(-1,1,1);
			dir            = Vector3.left;
		}
		else
		{
			createVec3     = Player.instance.transform.position + new Vector3(-3, 0, 0);
			localScaleVec3 = new Vector3(1,1,1);
			dir            = Vector3.right;
		}

		for (int i = 0; i < 10; i++)
		{
			Vector3 bulletPosition = createVec3 + new Vector3(Random.Range(-1f, 1f), Random.Range(-2f, 2f));
			GameObject bullet = PoolManager.instance.GetWithPosition(prefabId[0], bulletPosition);
			bullet.transform.localScale = localScaleVec3;
			bullet.GetComponent<PlayerBullet>().Init(weapon.finalCalculationDamage * (data.skillRatePer[0] / 100), -100, 0, dir, true, 0,30);
			yield return new WaitForSeconds(0.05f);
		}
		
	}
	
	// 스킬 11 이기어검
	private IEnumerator SoulSword()
	{
		skillCooldownTimer = data.skillCooldown / 1000;
		isSkillOperation   = true;

		List<Transform> bulletList = new List<Transform>();
		for (int i = 0; i < 2; i++)
		{
			// 360도를 2개로 나누어 각도를 계산
			float angle  = (360f / 2 * i)  - 90f;
			float radian = angle * Mathf.Deg2Rad; // 각도를 라디안으로 변환
			Vector3 dir = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0).normalized;
			
			Vector3 bulletPosition;
			if (i == 0)
			{
				bulletPosition = Player.instance.transform.position + new Vector3(1,0,0);
			}
			else
			{
				bulletPosition = Player.instance.transform.position + new Vector3(-1,0,0);
			}
			
			Transform bullet = PoolManager.instance.GetWithPosition(prefabId[0], bulletPosition).transform;
			bulletList.Add(bullet);
			
			if (i == 0)
			{
				bullet.rotation  = Quaternion.FromToRotation(Vector3.up, dir);
				bullet.GetComponent<SkillFunction>().Init(weapon.finalCalculationDamage * (data.skillRatePer[0] / 100), data.stunTime, data.isBossDamageBoost,false);
			}
			else if (i == 1)
			{
				bullet.rotation  = Quaternion.FromToRotation(Vector3.up, dir);
				bullet.GetComponent<SkillFunction>().Init(weapon.finalCalculationDamage * (data.skillRatePer[0] / 100), data.stunTime, data.isBossDamageBoost,true);
			}
			
			bullet.GetComponent<SkillFunction>().FollowHighestTypeEnemy(true);	// 작동
		}
		
		yield return new WaitForSeconds(data.skillDamageDuration / 1000f);
		
		isSkillOperation = false;
		foreach (var bulletLists in bulletList)	// 멈추기
			bulletLists.GetComponent<SkillFunction>().FollowHighestTypeEnemy(false);
	}
	
	private bool IsFullyVisible(GameObject obj)
	{
		// SpriteRenderer spriteRenderer = obj.GetComponentInChildren<SpriteRenderer>(); // SpriteRenderer 가져오기
		// if (spriteRenderer == null)
		// 	return false; // SpriteRenderer가 없으면 보이지 않는 것으로 간주
		//
		// Bounds bounds = spriteRenderer.bounds; // SpriteRenderer의 바운드 가져오기
		//
		// // 바운드의 모든 코너 좌표를 계산
		// Vector3[] corners = new Vector3[4];
		// corners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
		// corners[1] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
		// corners[2] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
		// corners[3] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);
		//
		// // 각 코너가 화면에 보이는지 확인
		// foreach (var corner in corners)
		// {
		// 	if (Camera.main != null)
		// 	{
		// 		Vector3 viewportPoint = Camera.main.WorldToViewportPoint(corner); // 월드 좌표를 뷰포트 좌표로 변환
		//
		// 		// 뷰포트 좌표가 설정된 범위 안에 있는지 확인
		// 		if (viewportPoint.x < 0.05f || viewportPoint.x > 0.95f ||
		// 		    viewportPoint.y < 0.1f  || viewportPoint.y > 0.9f  ||
		// 		    viewportPoint.z < 0) // 카메라 앞에 있어야 함
		// 		{
		// 			//Debug.Log("화면에 안보임");
		// 			return false; // 화면에 보이지 않으면 false 반환
		// 		}
		// 	}
		// }
		
		
		// 중심이 범위 안에 있는지 확인
		Vector3 viewportPoint = Camera.main.WorldToViewportPoint(obj.transform.position); // 월드 좌표를 뷰포트 좌표로 변환
		if (viewportPoint.x < 0.05f || viewportPoint.x > 0.95f ||
		    viewportPoint.y < 0.1f  || viewportPoint.y > 0.9f  ||
		    viewportPoint.z < 0) // 카메라 앞에 있어야 함
		{
			//Debug.Log("화면에 안보임");
			return false; // 화면에 보이지 않으면 false 반환
		}
		
		//Debug.Log("화면에 보임.");
		return true; // 모든 코너가 보이면 true 반환
	}

	// 화면에 보이는 적 중, 가장 강한 적을 찾기
	private GameObject FindHighestTypeEnemy()
	{
		// 적 풀 리스트 배열 받아오기
		List<GameObject> activeEnemies = new List<GameObject>();
		activeEnemies = PoolManager.instance.poolList[0];
		
		// 뒤에서부터, 비활성화된 적은 리스트에서 제외 해주기
		for (int i = activeEnemies.Count - 1; i >= 0 ; i--)
		{
			if(!activeEnemies[i].activeSelf)
				activeEnemies.Remove(activeEnemies[i]);
		}

		GameObject highestTypeEnemy = activeEnemies
			.Select(enemy => enemy.GetComponent<Enemy>())							   // Enemy 스크립트 가져오기
			.Where(enemy => enemy != null && IsFullyVisible(enemy.gameObject) && enemy.isLive) // 적이 보이고, 적이 살아 있고
			.OrderByDescending(enemy => (int)enemy.enemyType)								   // 적의 타입 순으로 내림차순 정렬(Non < Melee < Range < Elite)
			.FirstOrDefault()?.gameObject;									                   // 가장 높은 등급의 적 가져오기
		
		return highestTypeEnemy;
	}
	
	private Vector3 GetRandomPositionInCameraView()
	{
		float randomX = Random.Range(0.05f, 0.95f);
		float randomY = Random.Range(0.05f, 0.95f);

		float zDistance = Mathf.Abs(Camera.main.transform.position.z);
		Vector3 viewportPosition = new Vector3(randomX, randomY, zDistance);

		return Camera.main.ViewportToWorldPoint(viewportPosition);
	}

	// 안전성 검사 헬퍼 메서드
	private bool IsValidPrefabIndex(int index)
	{
		if (prefabId == null || index < 0 || index >= prefabId.Length)
		{
			Debug.LogError($"PlayerSkill: prefabId 배열 인덱스({index})가 유효하지 않습니다.");
			return false;
		}
		
		if (prefabId[index] < 0)
		{
			Debug.LogError($"PlayerSkill: prefabId[{index}]이 유효하지 않습니다.");
			return false;
		}
		
		if (PoolManager.instance == null)
		{
			Debug.LogError("PlayerSkill: PoolManager.instance가 null입니다.");
			return false;
		}
		
		return true;
	}
}
