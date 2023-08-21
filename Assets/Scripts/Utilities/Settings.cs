using System;
using UnityEngine;
public class Settings
{
    
    public const float itemFadeDuration = 0.35f;
    public const float targetAlpha = 0.45f;
    
    //时间
    public const float secondThreshold = 0.1f; //数值越小时间越快
    public const int secondHold = 59;
    public const int minuteHold = 59;
    public const int hourHold = 23;
    public const int dayHold = 30;
    public const int seasonHold = 3;

    //Transition
    public const float fadeDuration = 1.5f;
   
    //reap
    public const int reapAmount = 2;
    
    //NPC 网格移动
    public const float gridCellSize = 1;
    public const float gridCellDiagonalSize = 1.41f;
    public const float pixelSize = 0.05f;   //20*20 占 1 unit
    public const float animationBreakTime = 5f; //动画间隔时间
    public const int maxGridSize = 9999;    //最大网格尺寸

    //灯光
    public const float lightChangeDuration = 25f;
    public static TimeSpan morningTime = new TimeSpan(5, 0, 0);
    public static TimeSpan nightTime = new TimeSpan(19, 0, 0);

    public static Vector3 playerStartPos = new Vector3(11.7f,-1.15f,0);
    
    public const int playerStartMoney = 100;
}