using System.Collections;
using System.Collections.Generic;

using TowerDefenseDemo.Persistence;

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

    public class GameController : SingletonBehaviour<GameController>
    {
        public GameState State => state;
        private GameState state = GameState.WaitingStart;
        [HideInInspector] public UnityEvent<GameState> StateChangeEvent = new();

        public const float BlockLength = 10f;

        public LevelData CurrentLevelData;
        public int CurrentWaveIndex { get; private set; } = 0;
        public int AliveEnemyCount = 0;

        private void ChangeGameState(GameState newState)
        {
            if (newState == state) { return; }
            state = newState;
            StateChangeEvent.Invoke(newState);
        }

        private async void Start()
        {
            EnemySpawner.Instance.InitializeSpawner(CurrentLevelData.waveInfos[0]);
            CurrentWaveIndex = 0;
            AliveEnemyCount = 0;
            await MapBuilder.BuildMap(CurrentLevelData);
            ChangeGameState(GameState.AFK);
            EnemySpawner.Instance.StartSpawn();
        }

        private void Update()
        {
            if (state == GameState.AFK) //Try to change game state
            {
                if (EnemySpawner.Instance.State == SpawnState.SpawnComplete && AliveEnemyCount == 0)
                {
                    if (CurrentWaveIndex == CurrentLevelData.waveInfos.Count)
                    {
                        ChangeGameState(GameState.LevelCompleted);
                    }
                    else
                    {
                        CurrentWaveIndex++;
                        ChangeGameState(GameState.Deploying);
                    }
                }
            }
        }
    }
}