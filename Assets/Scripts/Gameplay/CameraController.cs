using DG.Tweening;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseDemo.Gameplay
{
    public class CameraController : SingletonBehaviour<CameraController>
    {
        [SerializeField] private Transform arm;
        [SerializeField] private Vector3 referenceRotation;
        [SerializeField] private Vector3 deployingRotation;
        [SerializeField] [Range(0, 180)] private float maxDeviateAngleWidth;
        [SerializeField] [Range(0, 90)] private float maxDeviateAngleHeight;
        [SerializeField] private float smoothTime;
        [SerializeField] private float distance;
        [SerializeField] private float gameOverDistance;
        [SerializeField] private float gameOverAnimTime;

        private Vector3 targetRotation;
        private Vector4 currentVelocity;

        private Vector3 followPos;

        private void Start()
        {
            GameController.Instance.StateChangeEvent.AddListener(OnGameStateChange);
            var segments = GlobalData.CurrentLevelData.enemyRoadSegments;
            followPos = new Vector3(segments[segments.Count - 1].x, 0f, segments[segments.Count - 1].y) * GlobalData.BlockLength;
        }

        private void Update()
        {
            switch (GameController.Instance.State)
            {
                case GameState.AFK:
                case GameState.Deploying:
                case GameState.LevelCompleted:
                case GameState.LevelFailed:
                    transform.localPosition = Vector3.back * distance;
                    if (GameController.Instance.State != GameState.Deploying)
                    {
                        var mousePos = Input.mousePosition;
                        var y = (mousePos.x / Screen.width) * maxDeviateAngleWidth;
                        var x = - (mousePos.y / Screen.height) * maxDeviateAngleHeight;
                        targetRotation = referenceRotation + new Vector3(x, y, 0f);
                    }
                    arm.localRotation = QuaternionUtil.SmoothDamp(arm.localRotation, Quaternion.Euler(targetRotation), ref currentVelocity, smoothTime);
                    break;
            }
        }

        private void OnDestroy()
        {
            GameController.Instance?.StateChangeEvent.RemoveListener(OnGameStateChange);
        }

        public void SetFollowPos(Vector3 v) => followPos = v;

        public void ShakeCamera(float quake, float duration)
        {
            var deltaPos = Vector3.zero;

            DOTween.To(() => quake, q =>
            {
                quake = q;
                transform.position -= deltaPos;
                deltaPos = Random.insideUnitSphere * quake;
                transform.position += deltaPos;
            }, 0f, duration).SetUpdate(UpdateType.Late).OnComplete(() => transform.position -= deltaPos);
        }

        private void OnGameStateChange(GameState newState)
        {
            switch (newState)
            {
                case GameState.Deploying:
                    targetRotation = deployingRotation;
                    break;
                case GameState.AFK:
                    targetRotation = referenceRotation;
                    break;
                case GameState.LevelFailed:
                case GameState.LevelCompleted:
                    arm.transform.DOLocalMove(followPos, gameOverAnimTime).SetUpdate(true);
                    DOTween.To(() => distance, v => distance = v, gameOverDistance, gameOverAnimTime).SetUpdate(true);
                    break;
                
            }
        }
    }
}