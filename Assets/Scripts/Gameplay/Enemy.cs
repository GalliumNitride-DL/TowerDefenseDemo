using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseDemo.Gameplay
{
    public class Enemy : MonoBehaviour
    {
        [Tooltip("Blocks per second")]
        public float speed;
        public float hitPoints;
        public float height;
        public DamageType immuneDamageType = DamageType.Decoy;

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

        private void OnEnable()
        {
            EvaluatePosition(0f);
        }

        private void Update()
        {
            EvaluatePosition(Time.deltaTime);
        }
    }
}