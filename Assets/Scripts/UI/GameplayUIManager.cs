using System.Collections;
using System.Collections.Generic;

using TMPro;

using TowerDefenseDemo.Gameplay;

using UnityEngine;

namespace TowerDefenseDemo.UI
{
    public class GameplayUIManager : SingletonBehaviour<GameplayUIManager>
    {
        [SerializeField] private TextMeshProUGUI moneyText, waveText, jieSuanText;
        private int currentShowMoney = 0;

        private void Start()
        {
            GameController.Instance.StateChangeEvent.AddListener(OnGameStateChange);
            currentShowMoney = GlobalData.CurrentLevelData.startMoney;
            moneyText.text = $"Money ${currentShowMoney}";
        }

        private void Update()
        {
            if (currentShowMoney == GlobalData.Money) { return; }
            if (currentShowMoney > GlobalData.Money)
            {
                currentShowMoney--;
            }
            else if (currentShowMoney < GlobalData.Money)
            {
                currentShowMoney++;
            }
            moneyText.text = $"Money ${currentShowMoney}";
        }

        private void OnDestroy()
        {
            GameController.Instance?.StateChangeEvent.RemoveListener(OnGameStateChange);
        }

        private void OnGameStateChange(GameState newState)
        {
            if (newState == GameState.Deploying)
            {
                waveText.text = $"Wave { GameController.Instance.currentWaveIndex + 1 } of { GlobalData.CurrentLevelData.waveInfos.Count }";
            }
            else if (newState == GameState.LevelCompleted)
            {
                jieSuanText.text = "Complete";
            }
            else if (newState == GameState.LevelFailed)
            {
                jieSuanText.text = "Wasted";
            }
        }

#region Unity UI Call

        public void Pause() => GameController.Instance.Pause().Forget();

        public void Resume() => GameController.Instance.Resume();

#endregion
    }
}