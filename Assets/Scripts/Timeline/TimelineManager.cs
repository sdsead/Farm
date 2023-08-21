using System;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : Singleton<TimelineManager>
{
    public PlayableDirector startDirector;
    private PlayableDirector currentDirector;

    private bool isDone;
    public bool IsDone { set => isDone = value; }
    private bool isPause;
    protected override void Awake()
    {
        base.Awake();
        currentDirector = startDirector;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneloadEvent += OnAfterSceneLoadedEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        startDirector.played += OnPlayed;
        startDirector.stopped += OnStopped;
    }


    private void OnDisable()
    {
        EventHandler.AfterSceneloadEvent -= OnAfterSceneLoadedEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }


    private void Update()
    {
        //恢复时间
        if (isPause && Input.GetKeyDown(KeyCode.Space) && isDone)
        {
            isPause = false;
            currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
        }
    }

    private void OnStartNewGameEvent(int obj)
    {
        if (startDirector != null)
            startDirector.Play();
    }

    private void OnAfterSceneLoadedEvent()
    {
        if (!startDirector.isActiveAndEnabled)
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
    }

    private void OnStopped(PlayableDirector obj)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
    }

    private void OnPlayed(PlayableDirector obj)
    {
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }
    
    /// <summary>
    /// 暂停
    /// </summary>
    /// <param name="director"></param>
    public void PauseTimeline(PlayableDirector director)
    {
        currentDirector = director;

        currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);
        isPause = true;
    }
}
