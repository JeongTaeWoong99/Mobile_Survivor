## 📑 목차
- [📋 개요](#-개요)
- [🎬 인게임 사진](#-인게임-사진)
- [🔗 관련 링크](#-관련-링크)
- [✨ 주요 기능](#-주요-기능)
- [📂 프로젝트 구조](#-프로젝트-구조)
- [🛠 기술 스택](#-기술-스택)
- [🏗 아키텍처](#-아키텍처)

## 📋 개요

| 항목 | 내용 |
|------|------|
| 기간 | 2025.01 ~ 2025.02 |
| 인원 | 2명(클라이언트 1, 기획 1) |
| 역할 | 클라이언트 |
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
- **프레임레이트 관리** : 모바일 환경에 맞춘 30fps 타겟 프레임
- **메모리 관리** : 오브젝트 풀링으로 Instantiate/Destroy 호출 최소화
- **안전성 검사** : 모든 주요 메서드에 Null 체크 및 예외 처리
- **렌더링 최적화** : 2D Pixel Perfect Camera로 픽셀 아트 최적화

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

### 1️⃣ 싱글톤 패턴

주요 매니저 클래스들은 싱글톤 패턴을 적용하여 전역 접근성과 단일 인스턴스를 보장합니다.

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }
}
```

### 2️⃣ 데이터 기반 설계

ScriptableObject를 활용하여 게임 데이터와 로직을 분리했습니다.

```
ItemData (ScriptableObject)
  ├─ 무기 스탯 (damage, rate, range, count, penetration)
  ├─ 무기 타입 (melee / ranged)
  └─ 발사체 프리팹 참조
```

### 3️⃣ 컴포넌트 기반 구조

각 게임 오브젝트는 역할에 따라 분리된 컴포넌트로 구성됩니다.

```
Player GameObject
  ├─ Player.cs (이동, 충돌, 상태)
  ├─ PlayerFunction.cs (유틸리티)
  └─ Weapon GameObject (자식)
       └─ Weapon.cs (공격 로직)
```

### 4️⃣ 게임 흐름 제어

게임 상태 변화에 따른 UI 업데이트 및 로직 처리를 구조화했습니다.

```
레벨업 발생
  ↓
GameManager.GetExp()
  ↓
IngameUI.Show() (아이템 선택 UI)
  ↓
Weapon.WeaponLevelUp() (무기 강화)
```

### 5️⃣ 오브젝트 풀링 아키텍처

```
PoolManager
  ├─ prefabs[] (프리팹 배열)
  ├─ poolList[] (오브젝트 풀 리스트)
  ├─ PreWarmAllPoolsCoroutine() (사전 생성)
  └─ Get() / GetWithPosition() (재사용)
       ↓
  비활성화된 오브젝트 재사용
            또는
  새 오브젝트 생성 및 풀 추가
```

**핵심 구현** :

```csharp
public IEnumerator PreWarmAllPoolsCoroutine(int objectsPerFrame = 10)
{
    for (int prefabIndex = 0; prefabIndex < prefabs.Length; prefabIndex++)
    {
        int createdThisFrame = 0;

        for (int count = 0; count < preWarmCounts[prefabIndex]; count++)
        {
            GameObject obj = Instantiate(prefabs[prefabIndex], transform);
            obj.SetActive(false);
            poolList[prefabIndex].Add(obj);

            createdThisFrame++;

            // 프레임 분산
            if (createdThisFrame >= objectsPerFrame)
            {
                createdThisFrame = 0;
                yield return null;
            }
        }
    }
}
```

### 6️⃣ 무기 시스템 아키텍처

각 무기 타입은 고유한 발사 메커니즘을 가집니다.

**주먹 (근접)** - 회전 배치 + 재활성화
```csharp
private void ArmDeployment()
{
    int forCount = baseCount + countIn + skillCount;

    for (int index = 0; index < forCount; index++)
    {
        Transform bullet = transform.GetChild(index);

        // 원형 배치
        Vector3 rotVec = Vector3.forward * (360 * index) / forCount;
        bullet.Rotate(rotVec);
        bullet.Translate(bullet.up, Space.World);

        // 수치 초기화
        bulletComponent.Init(finalCalculationDamage, basePenetration + penetrationIn, adjustedRange, Vector3.forward, false, 0);
    }
}

private void ReActivateBullet()
{
    for (int i = 0; i < conditionalBulletList.Count; i++)
    {
        if (!conditionalBulletList[i].bullet.gameObject.activeSelf)
        {
            conditionalBulletTimerList[i] += Time.deltaTime;
            if (conditionalBulletTimerList[i] > baseRate * 100f / rateInPer)
            {
                conditionalBulletList[i].bullet.gameObject.SetActive(true);
            }
        }
    }
}
```

**활 (원거리)** - 타겟팅 + 발사
```csharp
private IEnumerator BowFire(Transform shootTrans)
{
    int forCount = baseCount + countIn + skillCount;

    for (int index = 0; index < forCount; index++)
    {
        Vector3 dir = (shootTrans.position - transform.position).normalized;

        // 랜덤 분산
        dir.x += Random.Range(-0.1f, 0.1f);
        dir.y += Random.Range(-0.1f, 0.1f);

        // 폭발 화살 확률
        int randomRange = Random.Range(0, 100);
        GameObject bulletObj = randomRange >= explosiveArrowPer
            ? PoolManager.instance.GetWithPosition(bulletId[0], transform.position)
            : PoolManager.instance.GetWithPosition(skillObjectId, transform.position);

        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        yield return new WaitForSeconds(0.05f);
    }
}
```

**창 (관통)** - 360도 발사
```csharp
private void SpearShoot()
{
    int forCount = baseCount + countIn + skillCount;

    for (int index = 0; index < forCount; index++)
    {
        // 360도 분할
        float angle = (360f * index / forCount) + 90f;
        float radian = angle * Mathf.Deg2Rad;

        Vector3 dir = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0f).normalized;

        GameObject bulletObj = PoolManager.instance.GetWithPosition(bulletId[0], transform.position);
        bulletComponent.Init(finalCalculationDamage, basePenetration + penetrationIn, 0, dir, true, 0f);
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
    }
}
```

**칼 (회전)** - 연속 회전
```csharp
void Update()
{
    if (weaponId == 3)
    {
        // 증가율 적용
        transform.Rotate(Vector3.back * (finalCalculationRatePlus * Time.deltaTime));
    }
}

private void SwordDeployment()
{
    int forCount = baseCount + countIn + skillCount;

    for (int index = 0; index < forCount; index++)
    {
        Transform bullet = transform.GetChild(index);

        Vector3 rotVec = Vector3.forward * (360 * index) / forCount;
        bullet.Rotate(rotVec);
        bullet.Translate(bullet.up * (baseRange * rangeInPer / 100f), Space.World);

        bulletComponent.Init(finalCalculationDamage, -100, 0, Vector3.zero, false, 0);
    }
}
```

### 7️⃣ 스탯 계산 시스템

무기 스탯은 3단계 곱연산으로 최종값을 계산합니다.

```csharp
private void CalculateAndDeployment()
{
    // 데미지 = 기본 데미지 × 무기 강화% × 총 데미지%
    finalCalculationDamage = baseDamage
        * (damageInPer / 100f)
        * (GameManager.instance.totalAttackDamageInPer / 100f);

    // 공격속도 증가율 (칼 회전속도)
    finalCalculationRatePlus = baseRate
        * (rateInPer / 100f)
        * (GameManager.instance.totalAttackRateInPer / 100f);

    // 공격속도 감소율 (발사 간격)
    finalCalculationRateMinus = baseRate
        * (100f / rateInPer)
        * (100f / GameManager.instance.totalAttackRateInPer);
}
```

**스탯 계산 구조** :
```
Base Stat (기본값)
  ↓
× 무기 레벨업 % (damageInPer, rateInPer)
  ↓
× 악세서리 버프 % (totalAttackDamageInPer, totalAttackRateInPer)
  ↓
× 캐릭터 패시브 (CharacterPassive.AllDamage, MeleeDamage, RangeDamage)
  ↓
= Final Calculation (최종 적용값)
```
