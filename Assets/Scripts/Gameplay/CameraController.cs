using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseDemo.Gameplay
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform arm;
        [SerializeField] private Vector3 referenceRotation;
        [SerializeField] private Vector3 deployingRotation;
        [SerializeField] [Range(0, 180)] private float maxDeviateAngleWidth;
        [SerializeField] [Range(0, 90)] private float maxDeviateAngleHeight;
        [SerializeField] private float smoothTime;
        [SerializeField] private float distance;

        private Vector3 targetRotation;
        private Vector4 currentVelocity;

        private void OnEnable()
        {
            GameController.Instance.StateChangeEvent.AddListener(OnGameStateChange);
        }

        private void Update()
        {
            if (GameController.Instance.State == GameState.Deploying || GameController.Instance.State == GameState.AFK)
            {
                transform.localPosition = Vector3.back * distance;
                if (GameController.Instance.State == GameState.AFK)
                {
                    var mousePos = Input.mousePosition;
                    var y = (mousePos.x / Screen.width) * maxDeviateAngleWidth;
                    var x = - (mousePos.y / Screen.height) * maxDeviateAngleHeight;
                    targetRotation = referenceRotation + new Vector3(x, y, 0f);
                }
                arm.localRotation = QuaternionUtil.SmoothDamp(arm.localRotation, Quaternion.Euler(targetRotation), ref currentVelocity, smoothTime);
            }
        }

        private void OnDisable()
        {
            GameController.Instance?.StateChangeEvent.RemoveListener(OnGameStateChange);
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
            }
        }
    }
}