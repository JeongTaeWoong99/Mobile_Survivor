using System;
using UnityEngine;

public class UI_Follow : MonoBehaviour
{
    RectTransform rect;
    
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }
    private void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
    
        if (!rect)
            transform.position = new Vector3(Player.instance.transform.position.x, Player.instance.transform.position.y, -10);
    }
}