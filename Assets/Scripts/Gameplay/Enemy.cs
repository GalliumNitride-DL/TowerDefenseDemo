using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace TowerDefenseDemo.Gameplay
{
    public class Enemy : MonoBehaviour
    {
        [Tooltip("Blocks per second")]
        public float speed;
        public int hitPoints;
        public float height;
        public int reward;
        public DamageType immuneDamageType = DamageType.Decoy;
        public UnityEvent<Enemy> OnDie = new();

        [HideInInspector] public int currentSegmentIndex;
        [HideInInspector] public float currentSegmentTime;

        private void EvaluatePosition(float dt)
        {
            var segments = GlobalData.CurrentLevelData.enemyRoadSegments;
            currentSegmentTime += dt;

            var dx = currentSegmentIndex == segments.Count - 1 ? Vector2Int.zero : segments[currentSegmentIndex + 1] - segments[currentSegmentIndex];
            var t = currentSegmentIndex == segments.Count - 1 ? 1f : Mathf.Abs((dx.x + dx.y)) / speed;

            while (currentSegmentTime > t && currentSegmentIndex < segments.Count - 1)
            {
                currentSegmentTime -= t;
                currentSegmentIndex++;
                if (currentSegmentIndex < segments.Count - 1)
                {
                    dx = segments[currentSegmentIndex + 1] - segments[currentSegmentIndex];
                    t = Mathf.Abs((dx.x + dx.y)) / speed;
                }
            }

            var c = segments[currentSegmentIndex] + (Vector2)dx * (currentSegmentTime / t);
            
            if (currentSegmentIndex == segments.Count - 1)
            {
                GameController.Instance.GameLost(this);
            }

            transform.position = new(c.x * GlobalData.BlockLength, height, c.y * GlobalData.BlockLength);
        }

        public void TakeDamage(int damage, DamageType damageType)
        {
            if (hitPoints == 0 || damageType == immuneDamageType || damageType == DamageType.Decoy) { return; }
            hitPoints = Mathf.Max(0, hitPoints - damage);
            if (hitPoints == 0)
            {
                OnDie.Invoke(this);

                GlobalData.AliveEnemyCount--;
                GlobalData.Money += reward;
                if (EnemySpawner.Instance.State == SpawnState.SpawnComplete && GlobalData.AliveEnemyCount == 0)
                {
                    GameController.Instance.OnLastEnemyDieCall(this);
                }
                
                if (GameController.Instance.State != GameState.LevelFailed && GameController.Instance.State != GameState.LevelCompleted)
                {
                    Destroy(gameObject, 0.1f);
                }
            }
        }

        private void OnEnable()
        {
            EvaluatePosition(0f);
            GlobalData.AliveEnemyCount++;
            GameController.Instance.StateChangeEvent.AddListener(OnGameStateChange);
        }

        private void OnDisable()
        {
            GameController.Instance?.StateChangeEvent.RemoveListener(OnGameStateChange);
        }

        private void Update()
        {
            if (hitPoints > 0 && GameController.Instance.State == GameState.AFK)
            {
                EvaluatePosition(Time.deltaTime);
            }
        }

        private void OnGameStateChange(GameState newState)
        {
            switch (newState)
            {
                case GameState.LevelFailed:
                    OnDie.RemoveAllListeners();
                    break;
                case GameState.LevelCompleted:
                    OnDie.RemoveAllListeners();
                    Destroy(gameObject, 5f);
                    break;
            }
        }
    }
}