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
        public float reward;
        public DamageType immuneDamageType = DamageType.Decoy;
        public UnityEvent<Enemy> OnDie = new();

        [HideInInspector] public int currentSegmentIndex;
        [HideInInspector] public float currentSegmentTime;

        private void EvaluatePosition(float dt)
        {
            var segments = GameController.Instance.CurrentLevelData.enemyRoadSegments;
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

            transform.position = new(c.x * GameController.BlockLength, height, c.y * GameController.BlockLength);
        }

        public void TakeDamage(int damage, DamageType damageType)
        {
            if (damageType == immuneDamageType || damageType == DamageType.Decoy) { return; }
            hitPoints = Mathf.Max(0, hitPoints - damage);
            if (hitPoints == 0)
            {
                OnDie.Invoke(this);
            }
        }

        private void OnEnable()
        {
            EvaluatePosition(0f);
        }

        private void Update()
        {
            if (hitPoints > 0)
            {
                EvaluatePosition(Time.deltaTime);
            }
            
        }
    }
}