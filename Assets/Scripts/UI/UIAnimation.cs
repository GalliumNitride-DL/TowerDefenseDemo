using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseDemo.UI
{
    [Serializable]
    public class UIStatusChangeTrigger // Didn't override Equals and GetHashCode, maybe do it someday
    {
        public UIOperationStatus oldStatus;
        public UIOperationStatus newStatus;

        public static UIStatusChangeTrigger None = new UIStatusChangeTrigger
        {
            oldStatus = UIOperationStatus.None, newStatus = UIOperationStatus.None 
        };

        public static bool operator== (UIStatusChangeTrigger lhs, (UIOperationStatus, UIOperationStatus) rhs)
        {
            return lhs.oldStatus == rhs.Item1 && lhs.newStatus == rhs.Item2;
        }
        public static bool operator!= (UIStatusChangeTrigger lhs, (UIOperationStatus, UIOperationStatus) rhs)
        {
            return lhs.oldStatus != rhs.Item1 || lhs.newStatus != rhs.Item2;
        }
    }

    public abstract class UIAnimation : MonoBehaviour
    {
        public bool isOpened { get; private set; }
        private bool finalIsOpened;
        private bool isAnimationPlaying;
        public bool isManualTrigger => triggerCondition.Count == 0;
        [SerializeField] private List<UIStatusChangeTrigger> triggerCondition = new();

        protected virtual void Awake()
        {

                GameplayUITracker.UIStatusChangeEvent.AddListener(OnUIStatusChange);

        }

        protected virtual void OnDestroy()
        {
            GameplayUITracker.UIStatusChangeEvent.RemoveListener(OnUIStatusChange);
        }

        private void OnUIStatusChange(UIOperationStatus oldStatus, UIOperationStatus newStatus)
        {
            foreach (var condition in triggerCondition)
            {
                if (condition == (oldStatus, newStatus))
                {
                    finalIsOpened = true;
                    if (!isAnimationPlaying) { isAnimationPlaying = true; PlayIn(); }
                }
                else if (condition == (newStatus, oldStatus))
                {
                    finalIsOpened = false;
                    if (!isAnimationPlaying) { isAnimationPlaying = true; PlayOut(); }
                }
            }
        }

        public abstract void PlayIn();
        public abstract void PlayOut();

        public virtual void InCompleteCallback()
        {
            isOpened = true;
            if (finalIsOpened != isOpened) { PlayOut(); }
            else { isAnimationPlaying = false; }
        }

        public virtual void OutCompleteCallback()
        {
            isOpened = false;
            if (finalIsOpened != isOpened) { PlayIn(); }
            else { isAnimationPlaying = false; }
        }
    }
}