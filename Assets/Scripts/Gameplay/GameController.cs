using Cysharp.Threading.Tasks;

using DG.Tweening;

using System.Collections;
using System.Collections.Generic;

using TowerDefenseDemo.Persistence;
using TowerDefenseDemo.UI;

using UnityEngine;
using UnityEngine.Events;

namespace TowerDefenseDemo.Gameplay
{
    public enum GameState
    {
        WaitingStart,
        Deploying,
        AFK,
        LevelFailed,
        LevelCompleted,
        Paused
    }

    /// <summary>
    /// GameController handles everything related to game state transitions
    /// </summary>
    public class GameController : SingletonBehaviour<GameController>
    {
        public GameState State => state;
        private GameState state = GameState.WaitingStart;
        [HideInInspector] public UnityEvent<GameState> StateChangeEvent = new();

        public LevelData currentLevelData;
        public int currentWaveIndex { get; private set; } = 0;

        private void ChangeGameState(GameState newState)
        {
            if (newState == state) { return; }
            state = newState;
            StateChangeEvent.Invoke(newState);
        }

        public void EnterNextWave()
        {
            if (state != GameState.Deploying || GameplayUITracker.GetCurrentStatus() != UIOperationStatus.DeployIdle) { return; }
            ChangeGameState(GameState.AFK);
            GameplayUITracker.BackToPreviousStatus();
            EnemySpawner.Instance.InitializeSpawner(GlobalData.CurrentLevelData.waveInfos[currentWaveIndex]);
            EnemySpawner.Instance.StartSpawn();
        }

        public void OnLastEnemyDieCall(Enemy enemy) //change game state
        {
            if (state == GameState.AFK)
            {
                if (currentWaveIndex == GlobalData.CurrentLevelData.waveInfos.Count - 1)
                {
                    if (Time.timeScale != 1) { TimeScaleFadeIn(); }
                    CameraController.Instance.SetFollowPos(enemy.transform.position);
                    ChangeGameState(GameState.LevelCompleted);
                    GameplayUITracker.PushStatus(UIOperationStatus.Complete);
                }
                else
                {
                    currentWaveIndex++;
                    ChangeGameState(GameState.Deploying);
                    GameplayUITracker.PushStatus(UIOperationStatus.DeployIdle);
                }
            }
        }

        public void GameLost(Enemy enemy)
        {
            if (state == GameState.AFK)
            {
                if (Time.timeScale != 1) { TimeScaleFadeIn(); }
                ChangeGameState(GameState.LevelFailed);
                GameplayUITracker.PushStatus(UIOperationStatus.Complete);
            }
        }

        public async UniTaskVoid Pause()
        {
            if (state != GameState.AFK) { return; }
            await TimeScaleFadeOut().AsyncWaitForCompletion();
            if (state == GameState.AFK)
            {
                ChangeGameState(GameState.Paused);
                GameplayUITracker.PushStatus(UIOperationStatus.Pause);
            }
        }

        public void Resume()
        {
            if (state != GameState.Paused) { return; }
            ChangeGameState(GameState.AFK);
            GameplayUITracker.BackToPreviousStatus();
            TimeScaleFadeIn();
        }

        protected async override void Awake()
        {
            base.Awake();
            GlobalData.CurrentLevelData = currentLevelData;
            GlobalData.Money = currentLevelData.startMoney;
            GameplayUITracker.ClearHistory();
            
            currentWaveIndex = 0;
            GlobalData.AliveEnemyCount = 0;
            await MapBuilder.BuildMap(GlobalData.CurrentLevelData);
            ChangeGameState(GameState.Deploying);
            GameplayUITracker.PushStatus(UIOperationStatus.DeployIdle);
        }

        private Tweener TimeScaleFadeIn() => DOTween.To(() => Time.timeScale, v => Time.timeScale = v, 1f, 0.75f).SetUpdate(true);

        private Tweener TimeScaleFadeOut() => DOTween.To(() => Time.timeScale, v => Time.timeScale = v, 0f, 0.75f).SetUpdate(true);


        private void Update()
        {
            Debug.Log(GlobalData.AliveEnemyCount);
        }

        private void OnDestroy()
        {
            GlobalData.CurrentLevelData = null;
            GameplayUITracker.ClearHistory();
        }
    }
}