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

        private int previousSegmentIndex = 0;
        private float spawnedTime = 0;

        private void Update()
        {
            EvaluatePosition();
        }

        private void EvaluatePosition()
        {
            var currentTime = Time.time - spawnedTime;
            var segments = GameController.Instance.CurrentLevelData.enemyRoadSegments;
            var previousSegment = segments[previousSegmentIndex];
            var previousSegmentsTime = (previousSegment.x + previousSegment.y) / speed;

            var currentSegment = segments[previousSegmentIndex + 1];
            var currentSegmentTime = (currentSegment.x + currentSegment.y) / speed;
            
            if (currentTime > currentSegmentTime && previousSegmentIndex < segments.Count - 1)
            {
                previousSegmentIndex++;
                currentTime -= currentSegmentTime;

                previousSegment = currentSegment;
                currentSegment = segments[previousSegmentIndex + 1];

                previousSegmentsTime = currentSegmentTime;
                currentSegmentTime = (currentSegment.x + currentSegment.y) / speed;
            }

            var t = (currentTime - previousSegmentsTime) / (currentSegmentTime - previousSegmentsTime);
            var l = Vector2.Lerp(previousSegment, currentSegment, t);
            transform.position = new(l.x * GameController.BlockLength, l.y * GameController.BlockLength, height);
        }
    }
}