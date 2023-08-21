using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Save;

namespace Transition
{
    public class TransitionManager : Singleton<TransitionManager>,ISaveable
    {
        [SceneName]public string startSceneName;

        private CanvasGroup fadeCanvasGroup;
        private bool isFade;
        
        public string GUID => GetComponent<DataGUID>().guid;

        protected override void Awake()
        {
            base.Awake();
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed, 0);
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        }

        private void OnEnable()
        {
            EventHandler.TransitionEvent += OnTransitionEvent;
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {
            EventHandler.TransitionEvent -= OnTransitionEvent;
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }
        
        private void OnEndGameEvent()
        {
            StartCoroutine(UnloadScene());
        }

        private void OnStartNewGameEvent(int obj)
        {
            StartCoroutine(LoadSaveDataScene(startSceneName));
        }
        
        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();

            fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
        }

        private void OnTransitionEvent(string sceneToGo, Vector3 posToGo)
        {
            if(!isFade)
                StartCoroutine(Transition(sceneToGo, posToGo));
        }
        
        /// <summary>
        /// 加载场景并激活
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(newScene);
        }

        
        
        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="targetPos"></param>
        /// <returns></returns>
        private IEnumerator Transition(string sceneName,Vector3 targetPos)
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return Fade(1);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
           
            EventHandler.CallMoveToPosEvent(targetPos);
            yield return LoadSceneSetActive(sceneName);
            EventHandler.CallAfterSceneloadEvent();
            yield return Fade(0);
        }

        /// <summary>
        /// 场景淡入
        /// </summary>
        /// <param name="targetAlpha"></param>
        /// <returns></returns>
        private IEnumerator Fade(float targetAlpha)
        {
            isFade = true;

            fadeCanvasGroup.blocksRaycasts = true;
            float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha)/Settings.fadeDuration;

            while (!Mathf.Approximately(fadeCanvasGroup.alpha,targetAlpha))
            {
                fadeCanvasGroup.alpha = 
                    Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }

            isFade = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }
      
        private IEnumerator LoadSaveDataScene(string sceneName)
        {
            yield return Fade(1f);

            //在游戏过程中 加载另外游戏进度
            if (SceneManager.GetActiveScene().name != "PersistentScene")    
            {
                EventHandler.CallBeforeSceneUnloadEvent();
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            }

            yield return LoadSceneSetActive(sceneName);
            EventHandler.CallAfterSceneloadEvent();
            yield return Fade(0);
        }


        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <returns></returns>
        private IEnumerator UnloadScene()
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return Fade(1f);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            yield return Fade(0);
        }
        
        
        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.dataSceneName = SceneManager.GetActiveScene().name;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            //加载游戏进度场景
            StartCoroutine(LoadSaveDataScene(saveData.dataSceneName));
        }
    }
}

