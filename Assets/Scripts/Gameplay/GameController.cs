using System.Collections;
using System.Collections.Generic;

using TowerDefenseDemo.Persistence;

using UnityEngine;
using UnityEngine.Events;

namespace TowerDefenseDemo.Gameplay
{
    public enum GameState
    {
        CountDown,
        AFK,
        Deploying,
        GameOver,
        Paused
    }

    public class GameController : SingletonBehaviour<GameController>
    {
        public GameState State => state;
        private GameState state;
        public UnityEvent<GameState> StateChangeEvent = new();
        public LevelData CurrentLevelData;

        public const float BlockLength = 10f;



        private void ChangeGameState(GameState newState)
        {
            if (newState == state) { return; }
            state = newState;
            StateChangeEvent.Invoke(newState);
        }

    }
}