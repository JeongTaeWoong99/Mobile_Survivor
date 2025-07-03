using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
	public static PoolManager instance;

	[Header("Pool Settings")]
	public GameObject[] prefabs;		 // 풀 하여 관리할 프리팹
	public int[] preWarmCounts;          // 각 프리팹당 미리 생성할 개수 (기본값: 5개)

	public List<GameObject>[] poolList; // 각각의 풀 프리팹의 리스트 배열
	
	[Header("Pre-warming")]
	public bool usePreWarming = true;    // Pre-warming 사용 여부
	public bool showPreWarmProgress = false; // Pre-warming 진행상황 로그 출력 여부
	
	void Awake()
	{
		// 싱글톤 패턴: 이미 인스턴스가 존재하는 경우 중복 생성 방지
		if (instance != null && instance != this)
		{
			Debug.Log("PoolManager: 이미 존재하는 인스턴스가 있어 새로운 PoolManager를 제거합니다.");
			Destroy(gameObject);
			return;
		}
		
		instance = this;
		DontDestroyOnLoad(gameObject);
		
		// 안전성 검사: prefabs 배열이 null이거나 비어있는 경우 처리
		if (prefabs == null || prefabs.Length == 0)
		{
			Debug.LogError("PoolManager: prefabs 배열이 비어있습니다! Inspector에서 프리팹을 할당해주세요.");
			return;
		}
    
		poolList = new List<GameObject>[prefabs.Length];       // 풀 배열 초기화(프리팹 갯수 만큼, 리스트 배열 만들기)

		for (int index = 0; index < poolList.Length; index++)  // 풀 리스트 초기화 
			poolList[index] = new List<GameObject>();
			
		// preWarmCounts 배열이 설정되지 않았거나 길이가 다르면 기본값으로 초기화
		if (preWarmCounts == null || preWarmCounts.Length != prefabs.Length)
		{
			preWarmCounts = new int[prefabs.Length];
			for (int i = 0; i < preWarmCounts.Length; i++)
				preWarmCounts[i] = 20; // 기본값: 각 프리팹당 20개씩
		}
		
		// 모바일 환경에서 null 프리팹 검사 및 경고
		int nullCount = 0;
		for (int i = 0; i < prefabs.Length; i++)
		{
			if (prefabs[i] == null)
			{
				nullCount++;
				Debug.LogError($"PoolManager: prefabs[{i}]가 null입니다! Inspector에서 확인해주세요.");
			}
		}
		
		if (nullCount > 0)
		{
			Debug.LogError($"PoolManager: 총 {nullCount}개의 null 프리팹이 발견되었습니다. 모바일에서 오류가 발생할 수 있습니다!");
		}
		else
		{
			Debug.Log($"PoolManager: 모든 프리팹({prefabs.Length}개) 검증 완료.");
		}
	}

	/// <summary>
	/// 게임 시작 시 모든 풀을 미리 생성합니다. (동기 방식)
	/// 페이드 화면이나 로딩 화면에서 호출하세요.
	/// </summary>
	public void PreWarmAllPools()
	{
		if (!usePreWarming) return;
		
		// 안전성 검사
		if (prefabs == null || prefabs.Length == 0)
		{
			Debug.LogError("PoolManager: prefabs 배열이 비어있어서 Pre-warming을 할 수 없습니다.");
			return;
		}
		
		if (showPreWarmProgress)
			Debug.Log("Pool Pre-warming 시작...");
			
		for (int prefabIndex = 0; prefabIndex < prefabs.Length; prefabIndex++)
		{
			if (prefabs[prefabIndex] == null)
			{
				Debug.LogError($"PoolManager: prefabs[{prefabIndex}]가 null입니다. Inspector에서 확인해주세요.");
				continue;
			}
			
			PreWarmPool(prefabIndex, preWarmCounts[prefabIndex]);
			
			if (showPreWarmProgress)
				Debug.Log($"Pool [{prefabIndex}] {prefabs[prefabIndex].name}: {preWarmCounts[prefabIndex]}개 생성 완료");
		}
		
		if (showPreWarmProgress)
			Debug.Log("Pool Pre-warming 완료!");
	}
	
	/// <summary>
	/// 코루틴을 사용한 프레임 분산 Pre-warming (선택사항)
	/// 더 부드러운 로딩을 원할 때 사용하세요.
	/// </summary>
	public IEnumerator PreWarmAllPoolsCoroutine(int objectsPerFrame = 10)
	{
		if (!usePreWarming) yield break;
		
		// 안전성 검사
		if (prefabs == null || prefabs.Length == 0)
		{
			Debug.LogError("PoolManager: prefabs 배열이 비어있어서 Pre-warming을 할 수 없습니다.");
			yield break;
		}
		
		if (showPreWarmProgress)
			Debug.Log("Pool Pre-warming 시작... (코루틴)");
			
		int totalCreated = 0;
		
		for (int prefabIndex = 0; prefabIndex < prefabs.Length; prefabIndex++)
		{
			if (prefabs[prefabIndex] == null)
			{
				Debug.LogError($"PoolManager: prefabs[{prefabIndex}]가 null입니다. Inspector에서 확인해주세요.");
				continue;
			}
			
			int createdThisFrame = 0;
			
			for (int count = 0; count < preWarmCounts[prefabIndex]; count++)
			{
				GameObject obj = Instantiate(prefabs[prefabIndex], transform);
				obj.SetActive(false);
				poolList[prefabIndex].Add(obj);
				
				createdThisFrame++;
				totalCreated++;
				
				// 프레임당 생성 개수 제한 + 렌더링 부하 분산
				if (createdThisFrame >= objectsPerFrame)
				{
					createdThisFrame = 0;
					yield return null; // 다음 프레임까지 대기
					yield return null; // 추가 프레임 대기로 렌더링 부하 분산
				}
			}
			
			// 각 프리팹 타입 완료 후 추가 대기 (렌더링 시스템 안정화)
			yield return null;
			
			if (showPreWarmProgress)
				Debug.Log($"Pool [{prefabIndex}] {prefabs[prefabIndex].name}: {preWarmCounts[prefabIndex]}개 생성 완료");
		}
		
		if (showPreWarmProgress)
			Debug.Log($"Pool Pre-warming 완료! 총 {totalCreated}개 오브젝트 생성");
	}
	
	/// <summary>
	/// 특정 풀만 미리 생성합니다.
	/// </summary>
	void PreWarmPool(int prefabIndex, int count)
	{
		// 안전성 검사
		if (prefabIndex < 0 || prefabIndex >= prefabs.Length)
		{
			Debug.LogError($"PoolManager: PreWarmPool의 prefabIndex({prefabIndex})가 범위를 벗어났습니다. (0 ~ {prefabs.Length-1})");
			return;
		}
		
		if (prefabs[prefabIndex] == null)
		{
			Debug.LogError($"PoolManager: prefabs[{prefabIndex}]가 null입니다.");
			return;
		}
		
		for (int i = 0; i < count; i++)
		{
			GameObject obj = Instantiate(prefabs[prefabIndex], transform);
			obj.SetActive(false);
			poolList[prefabIndex].Add(obj);
		}
	}

	// 각각, 오브젝트 풀링을 하는 prefabs들은 필요할 때, 바로 Instantiate하는 것이 아닌, Get(index)를 통해서, 재사용 or 생성함.
	// 오브젝트 풀링을 하는 prefabs들은 사용이 종료되면, Destroy되는 것이 아니라, 비활성화 해야 함.
	// 그래야, 재사용이 가능함.
	public GameObject Get(int index)
	{
		// 안전성 검사 1: prefabs나 poolList가 null인 경우
		if (prefabs == null || poolList == null)
		{
			Debug.LogError("PoolManager: prefabs 또는 poolList가 초기화되지 않았습니다.");
			return null;
		}
		
		// 안전성 검사 2: 인덱스 범위 검사
		if (index < 0 || index >= prefabs.Length)
		{
			//Debug.LogError($"PoolManager: Get({index}) - 인덱스가 범위를 벗어났습니다. (유효 범위: 0 ~ {prefabs.Length-1})");
			return null;
		}
		
		// 안전성 검사 3: 해당 인덱스의 프리팹이 null인 경우
		if (prefabs[index] == null)
		{
			Debug.LogError($"PoolManager: prefabs[{index}]가 null입니다. Inspector에서 확인해주세요.");
			return null;
		}
		
		// 안전성 검사 4: 해당 인덱스의 풀 리스트가 null인 경우
		if (poolList[index] == null)
		{
			Debug.LogError($"PoolManager: poolList[{index}]가 null입니다. 초기화 과정에서 문제가 발생했습니다.");
			return null;
		}

		GameObject select = null;

		// 해당 pools[index] 리스트에서 만들어져 있고, 비활성화 되어 있는 오브젝트가 있는지, 확인.
		// pools[index].Add(select)에서 찾기
		// 비활성화 + 만들어져 있음 = 재활용
		foreach (GameObject item in poolList[index])
		{
			// null 체크 추가 (오브젝트가 삭제되었을 수 있음)
			if (item != null && !item.activeSelf) 
			{
				select = item;
				select.SetActive(true);
				break;
			}
		}
		
		// select된 오브젝트가 없음 = 생성 (Pre-warming으로 인해 이 경우는 거의 발생하지 않음)
		if (select == null) 
		{
			select = Instantiate(prefabs[index], transform); // 깔끔한 정리를 위해, 인스펙터 창 생성 위치는 PoolManager 자식으로...
			poolList[index].Add(select);                     // 생성 및 풀 리스트 등록.
			
			// Pre-warming을 사용하는데도 새로 생성되는 경우 경고
			if (usePreWarming && showPreWarmProgress)
				Debug.LogWarning($"Pool [{index}] {prefabs[index].name}: Pre-warming 부족! 런타임 중 새로 생성됨");
		}
		
		// Enemy의 경우, 활성화 하기 전, 미리 플레이어 방향을 바라보도록 함(좌우반전 연속 2번으로 인한, 어색함 문제 해결)
		// Player.instance null 체크 추가
		if (select.name.Contains("Enemy") && Player.instance != null && Player.instance.rb2D != null)
		{
			select.transform.localScale = Player.instance.rb2D.position.x < select.transform.position.x ? new Vector3(-1, 1) : new Vector3(1, 1); // 좌우반전
		}
		
		return select;
	}
	
	/// <summary>
	/// 안전한 Get 메서드 - 오류 시 null 대신 기본 오브젝트 반환 옵션
	/// </summary>
	public GameObject GetSafe(int index, bool createFallback = false)
	{
		GameObject result = Get(index);
		
		// Get()에서 null이 반환되고, createFallback이 true인 경우
		if (result == null && createFallback && index >= 0 && index < prefabs.Length && prefabs[index] != null)
		{
			Debug.LogWarning($"PoolManager: GetSafe({index}) - 풀에서 가져오기 실패, 새 오브젝트를 생성합니다.");
			result = Instantiate(prefabs[index], transform);
		}
		
		return result;
	}
	
	/// <summary>
	/// 위치를 미리 설정한 후 활성화하는 Get 메서드 - 오브젝트 풀링 위치 문제 해결
	/// </summary>
	public GameObject GetWithPosition(int index, Vector3 position)
	{
		// 안전성 검사 1: prefabs나 poolList가 null인 경우
		if (prefabs == null || poolList == null)
		{
			Debug.LogError("PoolManager: prefabs 또는 poolList가 초기화되지 않았습니다.");
			return null;
		}
		
		// 안전성 검사 2: 인덱스 범위 검사
		if (index < 0 || index >= prefabs.Length)
		{
			return null;
		}
		
		// 안전성 검사 3: 해당 인덱스의 프리팹이 null인 경우
		if (prefabs[index] == null)
		{
			Debug.LogError($"PoolManager: prefabs[{index}]가 null입니다. Inspector에서 확인해주세요.");
			return null;
		}
		
		// 안전성 검사 4: 해당 인덱스의 풀 리스트가 null인 경우
		if (poolList[index] == null)
		{
			Debug.LogError($"PoolManager: poolList[{index}]가 null입니다. 초기화 과정에서 문제가 발생했습니다.");
			return null;
		}

		GameObject select = null;

		// 해당 pools[index] 리스트에서 만들어져 있고, 비활성화 되어 있는 오브젝트가 있는지, 확인.
		foreach (GameObject item in poolList[index])
		{
			// null 체크 추가 (오브젝트가 삭제되었을 수 있음)
			if (item != null && !item.activeSelf) 
			{
				select = item;
				// 위치를 먼저 설정한 후 활성화
				select.transform.position = position;
				select.SetActive(true);
				break;
			}
		}
		
		// select된 오브젝트가 없음 = 생성
		if (select == null) 
		{
			select = Instantiate(prefabs[index], transform);
			select.transform.position = position; // 위치 설정
			poolList[index].Add(select);
			
			// Pre-warming을 사용하는데도 새로 생성되는 경우 경고
			if (usePreWarming && showPreWarmProgress)
				Debug.LogWarning($"Pool [{index}] {prefabs[index].name}: Pre-warming 부족! 런타임 중 새로 생성됨");
		}
		
		// Enemy의 경우, 활성화 하기 전, 미리 플레이어 방향을 바라보도록 함
		if (select.name.Contains("Enemy") && Player.instance != null && Player.instance.rb2D != null)
		{
			select.transform.localScale = Player.instance.rb2D.position.x < select.transform.position.x ? new Vector3(-1, 1) : new Vector3(1, 1);
		}
		
		return select;
	}
}