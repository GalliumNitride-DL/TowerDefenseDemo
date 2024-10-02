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
        AFK,
        Deploying,
        GameOver,
        Paused
    }

    public class GameController : SingletonBehaviour<GameController>
    {
        public GameState State => state;
        private GameState state = GameState.WaitingStart;
        [HideInInspector] public UnityEvent<GameState> StateChangeEvent = new();

        public const float BlockLength = 10f;

        public LevelData CurrentLevelData;
        public int currentWaveIndex { get; private set; } = 0;

        private void ChangeGameState(GameState newState)
        {
            if (newState == state) { return; }
            state = newState;
            StateChangeEvent.Invoke(newState);
        }

        private void Start()
        {
            EnemySpawner.Instance.InitializeSpawner(CurrentLevelData.waveInfos[0]);
            ChangeGameState(GameState.AFK);
            EnemySpawner.Instance.StartSpawn();
        }

    }
}