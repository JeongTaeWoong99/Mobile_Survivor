using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillFunction : MonoBehaviour
{
	private CapsuleCollider2D cap2D;

	public GameObject effectObj;
	
	[HideInInspector] public float damage;
	
	[HideInInspector] public bool  isBossDamageBoost; // 보스에게 데미지가 증폭 되는지
	[HideInInspector] public float stunTime;		  // 스턴 시간
	
	// 이기어검
	private bool  isOrderByDescending; // 한명의 적을 동시에 노리지 않도록 함.
	private Enemy previousFindEnemy;   // 이전 적, 저장(또 다시, 공격하지 않도록)

	private void Awake()
	{
		cap2D = GetComponent<CapsuleCollider2D>();
	}
	
	public void Init(float damage, float stunTime,bool isBossDamageBoost, bool isOrderByDescending = false)
	{
		this.damage            = damage;
		this.isBossDamageBoost = isBossDamageBoost;
		this.stunTime          = stunTime;

		previousFindEnemy = null;
		this.isOrderByDescending = isOrderByDescending;
	}

	public void ColliderOn()
	{
		cap2D.enabled = true;
		//Debug.Log("켜짐");
	}
	
	public void ColliderOff()
	{
		cap2D.enabled = false;
		//Debug.Log("꺼짐");
	}

	public void EffectActiveTure()
	{
		effectObj.SetActive(true);
	}
	
	public void EffectActiveFalse()
	{
		effectObj.SetActive(false);
	}

	public void SetActiveFalse()
	{
		gameObject.SetActive(false);
	}

	public void FollowHighestTypeEnemy(bool isStart)
	{
		if (isStart)
			StartCoroutine(FollowHighestTypeEnemyCo());
		else
		{
			StopCoroutine(FollowHighestTypeEnemyCo());
			gameObject.SetActive(false);
		}
	}
	
	private IEnumerator FollowHighestTypeEnemyCo()
	{
		while (true)
		{
			GameObject highestTypeEnemy = FindHighestTypeEnemy(isOrderByDescending);
			
			Vector3 targetPosition;
			
			// 적이 있음 -> 빠르게 향하기
			if (highestTypeEnemy != null)
			{
				targetPosition = highestTypeEnemy.transform.position;
				yield return StartCoroutine(MoveToPositionToTarget(targetPosition));
			}
			// 적 없음 -> 느리게 돌아가기
			else
			{
				targetPosition = Player.instance.transform.position;
				yield return StartCoroutine(MoveToPositionToTarget(targetPosition));
			}
			yield return StartCoroutine(MoveRight());	// 자연스러운 이동을 위해, 앞으로 잠깐 이동하도록 하기
		}
	}
	
	private IEnumerator MoveRight(float speed = 20)
	{
		float moveTimer = 0;
	
		while (true)
		{
			transform.position += transform.right * (speed * Time.deltaTime);
			moveTimer += Time.deltaTime;

			if (moveTimer > 0.2)
				break;
			yield return null;
		}
	}
	
	// private IEnumerator MoveToPositionToTarget(Vector3 destination,float distance = 2, float speed = 20)
	// {
	// 	while (Vector2.Distance(transform.position, destination) > distance)
	// 	{
	// 		Vector2 direction = (destination - transform.position).normalized;
	// 		
	// 		if (direction != Vector2.zero)
	// 		{
	// 			float angle               = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
	// 			Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
	// 			transform.rotation        = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
	// 		}
	//
	// 		// 칼이 바라보는 방향(transform.right)으로 이동
	// 		transform.position += transform.right * (speed * Time.deltaTime);
	// 	
	// 		yield return null;
	// 	}
	// }
	
	private IEnumerator MoveToPositionToTarget(Vector3 destination, float distance = 2, float maxSpeed = 50, float minSpeed = 20)
	{
		float initialDistance = Vector2.Distance(transform.position, destination); // 초기 거리

		while (Vector2.Distance(transform.position, destination) > distance)
		{
			Vector2 direction = (destination - transform.position).normalized;

			if (direction != Vector2.zero)
			{
				float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
				Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
			}

			// 거리 기반으로 속도 계산 (0~1 사이를 반전하여 속도가 줄어들도록 설정)
			float currentDistance     = Vector2.Distance(transform.position, destination);
			float normalizationFactor = Mathf.Clamp01(currentDistance / initialDistance);			 // 0 ~ 1 사이 값
			float currentSpeed        = Mathf.Lerp(minSpeed, maxSpeed, normalizationFactor);   // 속도 계산 (가까울수록 느려짐)

			// 칼이 바라보는 방향(transform.right)으로 이동
			transform.position += transform.right * (currentSpeed * Time.deltaTime);

			yield return null;
		}
	}
	
	private GameObject FindHighestTypeEnemy(bool isOrderByDescending)
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

		GameObject highestTypeEnemy;
		// 오른차순 정렬
		if (isOrderByDescending)
		{
			highestTypeEnemy = activeEnemies
				.Select(enemy => enemy.GetComponent<Enemy>())							                                 // Enemy 스크립트 가져오기
				.Where(enemy => enemy != null && IsFullyVisible(enemy.gameObject) && enemy.isLive && enemy != previousFindEnemy) // 적이 보이고, 적이 살아 있고, +++ 이전 적이 아니고
				.OrderByDescending(enemy => (int)enemy.enemyType)								                                 // 적의 타입 순으로 내림차순 정렬(Non < Melee < Range < Elite)
				.FirstOrDefault()?.gameObject;									                                                 // 가장 높은 등급의 적 가져오기(OrderByDescending에 의해 내림차순 정렬 된, 맨 처음 오브젝트 = 가장 타입이 높은 적)
		}
		// 내림차순 정렬
		else
		{
			highestTypeEnemy = activeEnemies
				.Select(enemy => enemy.GetComponent<Enemy>())							                                 // Enemy 스크립트 가져오기
				.Where(enemy => enemy != null && IsFullyVisible(enemy.gameObject) && enemy.isLive && enemy != previousFindEnemy) // 적이 보이고, 적이 살아 있고, +++ 이전 적이 아니고
				.OrderBy(enemy => (int)enemy.enemyType)																			 // 적의 타입 순으로 내림차순 정렬(Non < Melee < Range < Elite)
				.FirstOrDefault()?.gameObject;									                                                 // 가장 높은 등급의 적 가져오기(OrderByDescending에 의해 내림차순 정렬 된, 맨 처음 오브젝트 = 가장 타입이 높은 적)
		}
		
		if (highestTypeEnemy)
		{
			// 보스의 경우, 연속으로 추적 가능
			if(highestTypeEnemy.GetComponent<Enemy>().enemyType != EnemyData.EnemyType.Boss)
				previousFindEnemy = highestTypeEnemy.GetComponent<Enemy>();
		}
		
		return highestTypeEnemy;
	}
	
	private bool IsFullyVisible(GameObject obj)
	{
		// 중심이 범위 안에 있는지 확인
		Vector3 viewportPoint = Camera.main.WorldToViewportPoint(obj.transform.position); // 월드 좌표를 뷰포트 좌표로 변환
		if (viewportPoint.x < 0.05f || viewportPoint.x > 0.95f ||
		    viewportPoint.y < 0.1f  || viewportPoint.y > 0.9f  ||
		    viewportPoint.z < 0)
				return false; // 화면에 보이지 않으면 false 반환

		return true; // 모든 코너가 보이면 true 반환
	}
}
