using UnityEngine;


public class Hand : MonoBehaviour
{
    SpriteRenderer player;
    
    public bool           isLeft;
    public SpriteRenderer spriteRen;
    
    Quaternion leftRot         = Quaternion.Euler(0, 0, -35);
    Quaternion leftRotReverse  = Quaternion.Euler(0, 0, -135);
    Vector3    rightPos        = new Vector3(0.35f, -0.15f, 0);
    Vector3    rightPosReverse = new Vector3(-0.15f, -0.15f, 0);

    void Awake()
    {
        player = GetComponentsInParent<SpriteRenderer>()[1];
    }

    void LateUpdate()
    {
        bool isReverse = player.flipX;
        
        // 왼손 근접 무기
        if (isLeft) 
        {
            transform.localRotation = isReverse ? leftRotReverse : leftRot;
            spriteRen.flipY         = isReverse;
            spriteRen.sortingOrder  = isReverse ? 4 : 6;
        }
        // 오른손 원거리 무기
        else 
        {
            transform.localPosition = isReverse ? rightPosReverse : rightPos;
            spriteRen.flipX         = isReverse;
            spriteRen.sortingOrder  = isReverse ? 6 : 4;
        }
    }
}

