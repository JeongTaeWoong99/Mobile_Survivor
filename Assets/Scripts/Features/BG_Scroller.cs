using System;
using UnityEngine;

public class BG_Scroller : MonoBehaviour
{
	private MeshRenderer render;
	private float offset;

	public float speed;

	private void Start()
	{
		render = GetComponent<MeshRenderer>();
	}

	private void Update()
	{
		offset += Time.deltaTime * speed;
		render.material.mainTextureOffset = new Vector2(offset, 0);
	}
}
