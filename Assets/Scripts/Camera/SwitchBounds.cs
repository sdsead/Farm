using System;
using Cinemachine;
using UnityEngine;

public class SwitchBounds : MonoBehaviour
{
    private void OnEnable()
    {
        EventHandler.AfterSceneloadEvent += SwitchConfinerShape;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneloadEvent -= SwitchConfinerShape;
    }


    private void SwitchConfinerShape()
    {
        PolygonCollider2D confinerShape =
            GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();

        CinemachineConfiner confiner = GetComponent<CinemachineConfiner>();
        
        confiner.m_BoundingShape2D = confinerShape;
        
        //清除缓存
        confiner.InvalidatePathCache();
    }
}
