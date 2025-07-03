using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerFunction : MonoBehaviour
{
	private void FixedUpdate()
	{
		// 스킬 빽플립 이동
		if (Player.instance.anim.GetCurrentAnimatorStateInfo(0).IsName("BackFlip"))
		{
			// 물리 이동
			Vector2 nextVec;
			if (Player.instance.spriteRen.flipX)
			{
				nextVec = Vector2.right * (10 * Time.fixedDeltaTime);
				Player.instance.rb2D.MovePosition(Player.instance.rb2D.position + nextVec);
			}
			else
			{
				nextVec = Vector2.left * (10 * Time.fixedDeltaTime);
				Player.instance.rb2D.MovePosition(Player.instance.rb2D.position + nextVec);
			}
		}
	}
	
	public void IgnoreCollisionsTrue()
	{
		Physics2D.IgnoreLayerCollision(7, 6, true);   // 적
		Physics2D.IgnoreLayerCollision(7, 9, true);	 // 오브젝트(나무/풀)
		Physics2D.IgnoreLayerCollision(7, 10, true); // 적 투사체
		Physics2D.IgnoreLayerCollision(7, 11, true); // 함정
	}
	
	public void IgnoreCollisionsFalse()
	{
		Physics2D.IgnoreLayerCollision(7, 6, false);  // 적
		Physics2D.IgnoreLayerCollision(7, 9, false);  // 오브젝트
		Physics2D.IgnoreLayerCollision(7, 10, false); // 작 투사체
		Physics2D.IgnoreLayerCollision(7, 11, false); // 함정
	}
	
	// public void ColliderOn()
	// {
	// 	Player.instance.cap2D.enabled = true;
	// }
	//
	// public void ColliderOff()
	// {
	// 	Player.instance.cap2D.enabled = false;
	// }
	
	public void SkillProgressFalse()
	{
		Player.instance.isSkillProgress = false;
	}
}
