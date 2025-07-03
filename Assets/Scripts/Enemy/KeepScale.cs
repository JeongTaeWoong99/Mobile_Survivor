using UnityEngine;

public class KeepScale : MonoBehaviour
{
	private Transform parent;

	void Start()
	{
		parent = transform.parent;
		if (parent == null)
		{
			Debug.Log("부모 오브젝트가 없습니다.");
			return;
		}
	}

	void FixedUpdate()
	{
		if (parent != null)
		{
			// 부모의 X 스케일이 음수면 자식의 X 스케일 반전
			float parentScaleX = parent.localScale.x;
			if(parentScaleX == 1)
				transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
			else
				transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
		}
	}
}