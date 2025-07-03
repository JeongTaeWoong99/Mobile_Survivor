using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField]
	private RectTransform lever;

	[SerializeField]
	private float leverRange = 100f;

	private RectTransform rectTransform;
	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		UpdateLeverPosition(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		UpdateLeverPosition(eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		lever.anchoredPosition   = Vector2.zero;
		Player.instance.inputVec = Vector2.zero;
	}

	private void UpdateLeverPosition(PointerEventData eventData)
	{
		Vector2 localPoint;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			    rectTransform,
			    eventData.position,
			    eventData.pressEventCamera,
			    out localPoint))
		{
			Vector2 clamped = Vector2.ClampMagnitude(localPoint, leverRange);
			lever.anchoredPosition = clamped;
			Player.instance.inputVec = clamped.normalized; // 정규화된 방향 벡터
		}
	}
}