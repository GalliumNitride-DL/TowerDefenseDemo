using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace TowerDefenseDemo.UI
{
    public enum UIOperationStatus
    {
        None,
        DeployIdle,
        DeployTower,
        SelectTower,
        ConfirmSell,
        Pause,
        Complete
    }

    public static class GameplayUITracker
    {
        private static Stack<UIOperationStatus> UIOperations = new();
        public static UnityEvent<UIOperationStatus, UIOperationStatus> UIStatusChangeEvent = new();

        public static void ClearHistory()
        {
            UIOperations.Clear();
            UIStatusChangeEvent.RemoveAllListeners();
        }

        public static void PushStatus(UIOperationStatus status)
        {
            var oldStatus = GetCurrentStatus();
            UIOperations.Push(status);
            UIStatusChangeEvent.Invoke(oldStatus, status);
        }

        public static UIOperationStatus GetCurrentStatus() => UIOperations.TryPeek(out var status) ? status : UIOperationStatus.None;
        
        public static UIOperationStatus BackToPreviousStatus()
        {
            var oldStatus = UIOperations.Pop();
            var newStatus = GetCurrentStatus();
            UIStatusChangeEvent.Invoke(oldStatus, newStatus);
            return newStatus;
        }

        public static bool PopStatusIfEqual(UIOperationStatus status)
        {
            if (UIOperations.Peek() == status)
            {
                BackToPreviousStatus(); return true;
            }
            return false;
        }
    }
}