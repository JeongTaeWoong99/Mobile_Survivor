using System.Collections;
using UnityEngine;

public class Pitfall : MonoBehaviour
{
	private PolygonCollider2D poly2D;
	private ParticleSystem[]  particleSystemList;
	
	[HideInInspector] public float collisionDamage;

	public  float loopTime;
	private float loopTimer;

	private bool isStopCo;

	private void Awake()
	{
		poly2D = GetComponent<PolygonCollider2D>();
	
		particleSystemList = GetComponentsInChildren<ParticleSystem>(true);
	}

	private void OnEnable()
	{
		isStopCo       = false;
		poly2D.enabled = true;
		loopTimer      = 0;
		foreach (var VARIABLE in particleSystemList)
			VARIABLE.Play();
	}

	private void Update()
	{
		loopTimer += Time.deltaTime;
			
		if (!isStopCo && loopTimer > loopTime)
		{
			isStopCo = true;
			StartCoroutine(StopCo());
		}
	}

	public void Init(float collisionDamage)
	{
		this.collisionDamage = collisionDamage;
	}

	private IEnumerator StopCo()
	{
		poly2D.enabled = false;
		foreach (var VARIABLE in particleSystemList)
			VARIABLE.Stop();

		yield return new WaitForSeconds(1f);
		
		gameObject.SetActive(false);
	}
}
