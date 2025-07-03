using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class SortingGroupController : MonoBehaviour
{
	private SortingGroup sortingGroup;

	private void Awake()
	{
		sortingGroup = GetComponent<SortingGroup>();
	}

	private void FixedUpdate()
	{
		sortingGroup.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
	}
}