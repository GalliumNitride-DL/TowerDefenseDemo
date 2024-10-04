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

        private void Update()
        {
            if (state == GameState.AFK) //Try to change game state
            {
                if (EnemySpawner.Instance.State == SpawnState.SpawnComplete && GlobalData.AliveEnemyCount == 0)
                {
                    if (currentWaveIndex == GlobalData.CurrentLevelData.waveInfos.Count - 1)
                    {
                        ChangeGameState(GameState.LevelCompleted);
                    }
                    else
                    {
                        currentWaveIndex++;
                        ChangeGameState(GameState.Deploying);
                        GameplayUITracker.PushStatus(UIOperationStatus.DeployIdle);
                    }
                }
            }
            Debug.Log(GlobalData.Money);
        }

        private void OnDestroy()
        {
            GlobalData.CurrentLevelData = null;
            GameplayUITracker.ClearHistory();
        }
    }
}