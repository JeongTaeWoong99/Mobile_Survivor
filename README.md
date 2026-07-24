## 📑 목차
- [📋 개요](#-개요)
- [🎬 인게임 사진](#-인게임-사진)
- [🔗 관련 링크](#-관련-링크)
- [✨ 주요 기능](#-주요-기능)
- [📂 프로젝트 구조](#-프로젝트-구조)
- [🛠 기술 스택](#-기술-스택)
- [🏗 아키텍처](#-아키텍처)
- [🔍 성능 최적화 과정](#-성능-최적화-과정)

## 📋 개요

| 항목 | 내용 |
|------|------|
| 기간 | 2025.01 ~ 2025.02 (2개월) |
| 인원 | 2명(클라이언트 1, 기획 1) |
| 역할 | 팀장, 클라이언트 |
| 도구 | Unity, C# |
| 타겟 기기 | Mobile |

모바일 플랫폼 개발 경험과 성능 최적화를 목표로 진행한 프로젝트입니다.

방대한 적 웨이브와 성장 시스템을 갖춘 캐주얼 2D 모바일 뱀서라이크류 게임입니다.

레벨업 시 무기나 악세서리를 선택해 캐릭터를 강화하며, 특정 무기 레벨에 도달하면 스킬이 해금되어 전략적인 전투와 재미를 더했습니다.

## 🎬 인게임 사진

<table>
  <tr>
    <td align="center">
      <img width="640" height="360" alt="메인 메뉴" src="https://github.com/user-attachments/assets/2845e559-3841-40ee-aaeb-4d57e67cbc6f" />
      <br/>
      <b>메인 메뉴</b>
    </td>
    <td align="center">
      <img width="640" height="360" alt="파워 업" src="https://github.com/user-attachments/assets/e2664fa9-f621-41c4-8030-764e4e3e8269" />
      <br/>
      <b>파워 업</b>
    </td>
  </tr>
  <tr>
    <td align="center">
      <img width="640" height="360" alt="캐릭터 선택" src="https://github.com/user-attachments/assets/72e3b1c9-682a-4cd3-8a53-5d0cd6347ab4" />
      <br/>
      <b>캐릭터 선택</b>
    </td>
    <td align="center">
      <img width="640" height="360" alt="게임 사진 1" src="https://github.com/user-attachments/assets/c40eb3dd-c267-47ea-8b3f-4894f2a9b019" />
      <br/>
      <b>게임 화면 1</b>
    </td>
  </tr>
  <tr>
    <td align="center">
      <img width="640" height="360" alt="게임 사진 2" src="https://github.com/user-attachments/assets/15f1355d-63cc-478d-82b2-c232231dc8e4" />
      <br/>
      <b>게임 화면 2</b>
    </td>
    <td align="center">
    </td>
  </tr>
</table>

## 🔗 관련 링크

| 항목 | 링크 |
|------|------|
| 시연 영상 | [바로가기](https://www.youtube.com/watch?v=ugx6h1X3BUs) |
| 프로파일링 영상 | [바로가기](https://youtu.be/XffEw6n8kGA) |


## ✨ 주요 기능

### 오브젝트 풀링 시스템
- **Pre-warming 기법 적용** : 게임 시작 전 필요한 오브젝트들을 미리 생성하여 런타임 성능 최적화
- **프레임 분산 생성** : 코루틴을 활용한 프레임별 오브젝트 생성으로 로딩 시 프레임 드랍 방지
- **안전성 검증** : Null 체크 및 인덱스 범위 검사를 통한 모바일 환경 안정성 확보
- **동적 확장** : 풀 부족 시 런타임 중 자동 생성으로 유연한 리소스 관리

### 성장 시스템
- **레벨업 기반 무기/악세서리 강화** : 경험치를 획득하여 레벨업 시 무기 또는 악세서리 선택
- **무기별 고유 스킬 해금** : 특정 레벨 도달 시 스킬 활성화
- **영구 업그레이드 시스템** : PlayerPrefs를 활용한 파워업 데이터 캐싱으로 게임 재시작 시에도 유지
- **캐릭터별 패시브 스탯** : 캐릭터마다 고유한 기본 능력치 제공

### 전투 시스템
- **4가지 무기 타입** : 주먹(근접), 활(원거리), 창(관통), 칼(회전) 각각 고유한 전투 방식
- **타겟팅 시스템** : 가시거리 내 가장 가까운 적 자동 타겟팅 및 화면 밖 적 필터링
- **무기 스탯 시스템** : 데미지, 공격속도, 사거리, 개수, 관통력 등 다층 스탯 계산
- **조건부 발사체** : 스킬 레벨에 따라 특수 발사체(폭발 화살 등) 확률 적용

### 적 AI 및 스폰 시스템
- **웨이브 기반 스폰** : 시간에 따라 난이도가 증가하는 적 스폰
- **보스 시스템** : 특정 시간에 보스 등장 및 클리어 조건
- **적 종류별 패턴** : 돌진형, 원거리형 등 다양한 적 행동 패턴

### 성능 최적화
- **프레임레이트 관리** : 기기 사양에 맞춘 타겟 프레임 설정 (저사양 30fps / 고사양 60fps)
- **메모리 관리** : 오브젝트 풀링으로 Instantiate/Destroy 호출 최소화
- **안전성 검사** : 모든 주요 메서드에 Null 체크 및 예외 처리
- **렌더링 최적화** : 2D Pixel Perfect Camera로 픽셀 아트 최적화

### 반응형 UI 시스템
- **해상도 대응** : Canvas Scaler의 Scale With Screen Size 모드로 다양한 화면 비율 지원
- **앵커 기반 배치** : Anchor/Pivot 시스템으로 UI 요소 자동 정렬
- **일관된 UX** : 모든 모바일 기기에서 동일한 조작 경험 제공
- **동적 크기 조정** : 화면 크기에 따라 UI 요소 비율 유지

## 📂 프로젝트 구조

```
Assets/Scripts/
├── Manager/                        # 게임 전체 관리 시스템
│   ├── GameManager.cs              # 게임 상태, 플레이어 정보 관리
│   ├── PoolManager.cs              # 오브젝트 풀링 시스템
│   ├── SpawnManager.cs             # 적 스폰 관리
│   ├── AudioManager.cs             # 사운드 관리
│   └── AchieveManager.cs           # 업적 시스템
│
├── Player/                         # 플레이어 관련
│   ├── Player.cs                   # 플레이어 제어 및 충돌 처리
│   ├── Weapon.cs                   # 무기 시스템
│   ├── PlayerBullet.cs             # 플레이어 투사체
│   ├── PlayerSkill.cs              # 스킬 시스템
│   ├── PlayerFunction.cs           # 플레이어 유틸리티 함수
│   ├── SkillFunction.cs            # 스킬 기능 구현
│   ├── Accessories.cs              # 악세서리 시스템
│   └── Hand.cs                     # 무기 표시 핸들러
│
├── Enemy/                          # 적 관련
│   ├── Enemy.cs                    # 적 AI 및 행동
│   ├── EnemyBullet.cs              # 적 투사체
│   ├── EnemyAttackRange.cs         # 적 공격 범위
│   ├── EnemyAimeFunction.cs        # 적 조준 기능
│   ├── ExplosionCheck.cs           # 폭발 판정
│   └── KeepScale.cs                # 스케일 유지
│
├── UI/                             # UI 관련
│   ├── MainUI.cs                   # 메인 메뉴 UI
│   ├── IngameUI.cs                 # 인게임 UI
│   ├── HUD.cs                      # 게임 정보 표시
│   ├── Item.cs                     # 아이템 UI
│   ├── PowerUp.cs                  # 파워업 UI
│   ├── VirtualJoystick.cs          # 가상 조이스틱
│   └── UI_Follow.cs                # UI 추적 시스템
│
├── Data/                           # 데이터 정의
│   ├── ItemData.cs                 # 아이템 ScriptableObject
│   ├── EnemyData.cs                # 적 ScriptableObject
│   ├── SkillData.cs                # 스킬 ScriptableObject
│   ├── PowerUpData.cs              # 파워업 ScriptableObject
│   └── CharacterPassive.cs         # 캐릭터 패시브 데이터
│
├── Object/                         # 게임 오브젝트
│   ├── DropItem.cs                 # 드롭 아이템 (경험치 등)
│   └── Pitfall.cs                  # 함정
│
└── Features/                       # 기타 기능
    ├── BG_Scroller.cs              # 배경 스크롤
    └── SortingGroupHandler.cs      # 렌더링 순서 관리
```

## 🛠 기술 스택

### 개발 환경
- **Unity 2022.3 LTS** - 2D 게임 엔진
- **C#** - 프로그래밍 언어
- **JetBrains Rider 2024.3.4f** - IDE

### 핵심 기술
- **Unity 2D** : Sprite Renderer, 2D Physics, Pixel Perfect Camera
- **Unity Input System** : 모바일 터치 및 가상 조이스틱
- **ScriptableObject** : 데이터 기반 설계
- **Coroutine** : 비동기 작업 및 시간 기반 로직
- **Object Pooling** : 메모리 최적화
- **PlayerPrefs** : 로컬 데이터 저장

### Unity Packages
- **2D Pixel Perfect** : 픽셀 아트 최적화
- **Profiling Core** : 성능 프로파일링

### 디자인 패턴
- **Singleton Pattern** : 매니저 클래스
- **Factory Pattern** : 무기 및 아이템 생성
- **Object Pooling** : 성능 최적화
- **Data-Driven Design** : ScriptableObject 기반 데이터 관리

## 🏗 아키텍처

### 1️⃣ 오브젝트 풀링 기반 최적화 파이프라인

게임 시작부터 적 스폰까지 전체 흐름을 자동화하여 런타임 성능을 극대화했습니다.

```
[게임 시작 - 로딩 화면]
  ↓
GameManager.GameStartCo()
  ↓
PoolManager.PreWarmAllPoolsCoroutine()  // 페이드인 중 Pre-warming (10개/프레임)
  ├─ Enemy 프리팹 × 20개 사전 생성
  ├─ PlayerBullet × 20개 사전 생성
  └─ EnemyBullet × 20개 사전 생성
  ↓
[게임 플레이 - 런타임]
  ↓
SpawnManager.SpawnEnemy(EnemyData)  // 웨이브 기반 스폰
  ↓
PoolManager.Get(0)  // 풀에서 재사용 (Instantiate 없음)
  ├─ 비활성화된 오브젝트 재사용 (O(1) 성능)
  └─ 풀 부족 시 런타임 확장
  ↓
Enemy.Init(EnemyData)  // ScriptableObject 데이터 자동 세팅
```

**핵심 코드** :

```csharp
// PoolManager.cs - 프레임 분산 Pre-warming
public IEnumerator PreWarmAllPoolsCoroutine(int objectsPerFrame = 10)
{
    for (int prefabIndex = 0; prefabIndex < prefabs.Length; prefabIndex++)
    {
        for (int count = 0; count < preWarmCounts[prefabIndex]; count++)
        {
            GameObject obj = Instantiate(prefabs[prefabIndex], transform);
            obj.SetActive(false);
            poolList[prefabIndex].Add(obj);

            if (++createdThisFrame >= objectsPerFrame)
            {
                createdThisFrame = 0;
                yield return null;  // 프레임 분산으로 렉 방지
            }
        }
    }
}
```

```csharp
// SpawnManager.cs - 풀링과 연계된 스폰 시스템
private void SpawnEnemy(EnemyData data)
{
    GameObject enemy = PoolManager.instance.Get(0);  // 풀에서 가져오기
    enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
    enemy.GetComponent<Enemy>().Init(data);          // 데이터 자동 초기화
}
```

**최적화 효과** :
- 게임 중 Instantiate/Destroy 호출 최소화 (GC 압력 감소)
- 로딩 시 프레임 드랍 방지 (10개/프레임 분산 생성)
- 적 스폰 시 즉각적인 생성 (Pool.Get()의 빠른 탐색)

### 2️⃣ ScriptableObject 기반 데이터 자동화 시스템

모든 게임 데이터를 ScriptableObject로 관리하여 코드와 데이터를 완전히 분리하고, 런타임 초기화를 자동화했습니다.

**데이터 구조** :
```
[Inspector - 디자인 단계]
ItemData.asset (ScriptableObject)
  ├─ itemId     : 0
  ├─ baseDamage : 10
  ├─ baseRate   : 2.0
  ├─ bullet     : [Prefab 참조]
  └─ levelUpList:
       ├─ Lv1: Damage +20%, Count +1
       ├─ Lv2: Rate +15%
       └─ Lv3: Penetration +1

[Runtime - 게임 실행]
Weapon.Init(ItemData data)
  ↓
자동 초기화 :
  ├─ weaponId   = data.itemId
  ├─ baseDamage = data.baseDamage × 캐릭터 패시브
  ├─ baseRate   = data.baseRate
  ├─ bulletId   = PoolManager에서 자동 검색
  └─ CalculateAndDeployment() 호출
```

**핵심 코드** :

```csharp
// Weapon.cs - 데이터 기반 자동 초기화
public void Init(ItemData data)
{
    weaponId   = data.itemId;
    baseDamage = data.baseDamage
        * CharacterPassive.AllDamage
        * CharacterPassive.MeleeDamage(itemType)
        * CharacterPassive.RangeDamage(itemType);

    // 프리팹 ID 자동 검색
    bulletId = new int[data.bullet.Length];
    for (int i = 0; i < PoolManager.instance.prefabs.Length; i++)
    {
        for (int j = 0; j < data.bullet.Length; j++)
        {
            if (data.bullet[j] == PoolManager.instance.prefabs[i])
                bulletId[j] = i;
        }
    }

    CalculateAndDeployment();  // 최종 스탯 계산 및 배치
}
```

**자동화 장점** :
- 코드와 데이터의 강한 결합 분리 (수정 시마다 개발자 개입 불필요)
- **협업 생산성 향상** : 기획자와 데이터 시트(컬럼/타입/단위/규칙)를 공동 설계하고, 코드 수정 없이 Inspector에서 데이터 생성·밸런스 수정 가능
- 데이터 재사용 (EnemyData, SkillData 동일 구조)
- 런타임 에러 방지 (Null 체크 및 자동 검증)

### 3️⃣ 레벨업 기반 성장 시스템

경험치 획득부터 무기 강화, 스킬 해금까지 이어지는 성장 파이프라인을 구현했습니다.

```
[적 처치]
  ↓
DropItem 생성 (경험치 구슬)
  ↓
Player 충돌 감지
  ↓
GameManager.GetExp(minEXP, maxEXP)
  ├─ currnetEXP += Random.Range(min, max) × expInPer
  └─ if (currnetEXP >= nextEXP[playerLevel])
       ↓
     playerLevel++
       ↓
     IngameUI.Show()  // 레벨업 UI 표시
       ↓
     CreateSelection()  // 아이템 3개 랜덤 선택
       ├─ 무기/악세사리 우선 (만렙 제외)
       └─ 부족 시 소모품 추가
       ↓
     [플레이어 선택]
       ↓
     Item.OnClick()
       ├─ 무기 강화: Weapon.WeaponLevelUp()
       │   ├─ damageInPer += 20%
       │   ├─ countIn += 1
       │   └─ CalculateAndDeployment() (재계산)
       │
       └─ 스킬 해금 조건 체크 (특정 레벨 도달 시)
           ↓
         SkillData 활성화
```

**핵심 코드** :

```csharp
// IngameUI.cs - 레벨업 아이템 선택 시스템
private void CreateSelection()
{
    List<int> weaponOrAccessoryList = new List<int>();
    List<int> consumableList = new List<int>();

    // 만렙이 아닌 무기/악세사리 필터링
    for (int i = 0; i < items.Length; i++)
    {
        if ((items[i].data.itemType == ItemData.ItemType.Melee ||
             items[i].data.itemType == ItemData.ItemType.Range ||
             items[i].data.itemType == ItemData.ItemType.Accessories)
            && items[i].level != items[i].data.levelUpList.Count)
        {
            weaponOrAccessoryList.Add(i);
        }
        else if (items[i].data.itemType == ItemData.ItemType.Consumption)
        {
            consumableList.Add(i);
        }
    }

    // 3개 선택 (무기/악세사리 우선, 부족 시 소모품 추가)
    List<int> randomNums = new List<int>();
    if (weaponOrAccessoryList.Count >= 3)
    {
        while (randomNums.Count < 3)
        {
            int selected = weaponOrAccessoryList[Random.Range(0, weaponOrAccessoryList.Count)];
            if (!randomNums.Contains(selected))
                randomNums.Add(selected);
        }
    }
    else
    {
        randomNums = weaponOrAccessoryList;
        foreach (var consumable in consumableList)
        {
            if (!randomNums.Contains(consumable))
                randomNums.Add(consumable);
        }
    }

    // 선택된 아이템만 활성화
    foreach (int index in randomNums)
        items[index].gameObject.SetActive(true);
}
```

**성장 시스템 특징** :
- 성장 완료 무기 자동 제외 (중복 선택 방지)
- 무기/악세사리 우선 제공 (성장성 보장)
- 스킬 해금 조건 (특정 무기 레벨 도달 시)

### 4️⃣ PlayerPrefs 영구 재화 시스템

게임 세션 종료 후에도 재화를 유지하여 메타 성장을 구현했습니다.

```
[게임 플레이]
  ↓
적 처치 → currentGold++
  ↓
[게임 종료]
  ↓
GameManager.GameVictory() / GameOver()
  ↓
IncrementGoldCoroutine(currentGold × 배율)
  ├─ PlayerPrefs.SetInt("GoldData", totalGold)  // 즉시 저장 (강제 종료 대비)
  └─ 부드러운 카운팅 애니메이션 (Lerp)
  ↓
[메인 메뉴]
  ↓
MainUI - 파워업 상점
  ├─ PlayerPrefs.GetInt("GoldData")로 골드 로드
  └─ 파워업 구매
      ├─ CharacterPassive.PowerUp0() (최대 체력 +10)
      ├─ CharacterPassive.PowerUp4() (총 공격력 +5%)
      └─ PlayerPrefs.SetInt("PowerUp0", level++)
  ↓
CharacterPassive.InitializePowerUps()  // 게임 시작 시 캐싱
  ├─ powerUpCache["PowerUp0"] = PlayerPrefs.GetInt("PowerUp0")
  └─ 반복 호출 최적화 (Dictionary 캐싱)
  ↓
[다음 게임]
  ↓
적용된 파워업으로 시작
  ├─ playerMaxHealth = baseHealth + PowerUp0()
  ├─ totalAttackDamage = baseDamage × (100 + PowerUp4()) / 100
  └─ moveSpeed = baseSpeed × (100 + PowerUp8()) / 100
```

**핵심 코드** :

```csharp
// CharacterPassive.cs - PlayerPrefs 캐싱 최적화
private static Dictionary<string, int> powerUpCache = new Dictionary<string, int>();

public static void InitializePowerUps()
{
    // 게임 시작 시 한 번만 로드 (반복 호출 방지)
    powerUpCache["PowerUp0"] = PlayerPrefs.GetInt("PowerUp0", 0);  // 최대 체력
    powerUpCache["PowerUp1"] = PlayerPrefs.GetInt("PowerUp1", 0);  // 초당 체력 회복
    powerUpCache["PowerUp4"] = PlayerPrefs.GetInt("PowerUp4", 0);  // 총 공격력
    // ...
}

public static int PowerUp0() => powerUpCache["PowerUp0"] * 10;  // 레벨당 +10 체력
public static int PowerUp4() => powerUpCache["PowerUp4"] * 5;   // 레벨당 +5% 공격력
```

```csharp
// GameManager.cs - 골드 저장 및 애니메이션
private IEnumerator IncrementGoldCoroutine(int targetGold)
{
    // 즉시 저장 (코루틴 중단 대비)
    PlayerPrefs.SetInt("GoldData", PlayerPrefs.GetInt("GoldData") + targetGold);

    // 부드러운 카운팅 애니메이션
    int startGold = currentGold;
    float elapsed = 0f;

    while (currentGold < targetGold)
    {
        elapsed += Time.fixedUnscaledDeltaTime * 2f;
        currentGold = Mathf.Clamp((int)Mathf.Lerp(startGold, targetGold, elapsed), startGold, targetGold);
        yield return null;
    }
}
```

**재화 시스템 특징** :
- 강제 종료 대비 (IncrementGoldCoroutine 시작 시 즉시 저장)
- 성능 최적화 (Dictionary 캐싱으로 반복 PlayerPrefs 호출 방지)
- 부드러운 UI (Lerp 기반 카운팅 애니메이션)

## 🔍 성능 최적화 과정

Unity Profiler를 활용하여 실제 모바일 기기에서 병목 지점을 분석하고 최적화를 진행했습니다.

### 프로파일링 프로세스

```
[1단계: 실기기 프로파일링]
  ↓
모바일 기기 USB 연결
  ↓
Unity Profiler - Development Build
  ├─ CPU Usage 분석
  ├─ Rendering 분석
  ├─ Memory 분석
  └─ GC Alloc 분석
  ↓
[2단계: 병목 지점 탐색]
  ↓
발견된 문제점:
  ├─ Instantiate/Destroy 반복 호출 (높은 GC Alloc 발견)
  ├─ 비효율적인 스크립트 로직 (과도한 호출 및 초기화 발견)
  ├─ 고비용 파티클 (3D 전용으로 만들어져, 2D 모바일 전용이 아닌 파티클 발견)
  └─ 불필요한 물리 연산 (과도한 물리 체크로 인한 CPU 부하 발견)
  ↓
[3단계: 최적화 적용]
  ↓
해결 방법:
  ├─ 오브젝트 풀링 도입 → GC Alloc 대폭 감소
  ├─ 컴포넌트 캐싱 → CPU 사용률 개선
  ├─ 파티클 경량화 → 렌더링 부하 감소
  └─ 불필요한 물리 체크 제거 → CPU 연산 부하 감소
  ↓
[4단계: 재측정 및 검증]
  ↓
최적화 결과 확인
  ├─ FPS 안정화 (기기별 안정적인 타겟 프레임 유지)
  └─ 프레임 드랍 현상 개선
```

### 주요 최적화 항목

**1. 오브젝트 풀링 시스템 도입**

**문제점** :
- Profiler에서 Instantiate/Destroy 호출이 프레임당 수십 회 발생
- 높은 GC Alloc으로 인한 프레임 드랍 발생

**해결** :
- Pre-warming 기법으로 게임 시작 시 오브젝트 사전 생성
- 풀에서 재사용하여 런타임 중 Instantiate 호출 제거

**효과** :
- GC Alloc 대폭 감소
- 프레임레이트 안정화

**2. 스크립트 최적화**

**문제점** :
- GetComponent 반복 호출로 CPU 부하 증가
- PlayerPrefs.GetInt 매 프레임 호출로 불필요한 디스크 I/O 발생

**해결** :
```csharp
// 개선 전
void Update()
{
    int powerUp = PlayerPrefs.GetInt("PowerUp0");  // 매 프레임 호출
    GetComponent<Enemy>().DoSomething();           // 매 프레임 탐색
}

// 개선 후
private Enemy enemyComponent;
private static Dictionary<string, int> powerUpCache;

void Awake()
{
    enemyComponent = GetComponent<Enemy>();  // 한 번만 캐싱
}

void Update()
{
    int powerUp = powerUpCache["PowerUp0"];  // Dictionary에서 즉시 조회
    enemyComponent.DoSomething();            // 캐싱된 참조 사용
}
```

**효과** :
- CPU 사용률 개선
- Profiler Scripts 항목 부하 감소

**3. 파티클 시스템 최적화**

**문제점** :
- 3D 전용 파티클을 2D 모바일에 사용하여 불필요한 렌더링 기능 작동
- 프레임 드랍 현상 발생 (간헐적 렉)

**해결** :
- 2D 전용 경량 파티클로 교체
- Max Particles 수 조정
- 불필요한 라이팅/쉐이더 제거

**효과** :
- 렌더링 부하 감소
- 프레임 드랍 현상 개선

**4. 불필요한 물리 연산 제거**

**문제점** :
- 과도한 물리 체크(충돌/트리거 연산)로 CPU 연산 부하 증가
- Profiler Physics 항목에서 불필요한 연산 비용 발견

**해결** :
- 불필요한 물리 체크 로직 제거 및 연산 조건 최소화

**효과** :
- CPU 연산 부하 감소
- 프레임 안정화에 기여

### 기기 테스트 결과

저사양/고사양 실기기를 각각 프로파일링하여, 기기 사양에 맞는 타겟 프레임을 안정적으로 방어했습니다.

| 테스트 기기 | 스펙 | 타겟 | 최적화 전 | 최적화 후 |
|------|------|------|------|------|
| **Galaxy S10+** (5년 사용) | Exynos 9820 / Mali-G76 MP12 | 30 FPS | 프레임 드랍 O, 30 FPS 유지 실패 | 프레임 드랍 X, **30 FPS 유지 성공** |
| **Galaxy S24** (1년 사용) | Exynos 2400 / Xclipse 940 | 60 FPS | 간헐적 프레임 드랍, 60 FPS 불안정 | 프레임 드랍 X, **60 FPS 유지 성공** |

### 프로파일링 영상

실제 프로파일링 과정과 최적화 결과는 [프로파일링 영상](https://youtu.be/XffEw6n8kGA)에서 확인할 수 있습니다.
