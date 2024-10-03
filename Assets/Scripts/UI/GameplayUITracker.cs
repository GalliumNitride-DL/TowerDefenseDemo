using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseDemo.UI
{
    public enum UIOperationStatus
    {
        None,
        DeployIdle,
        DeployTower,
        SelectTower,
        ConfirmUpgrade,
        ConfirmSell,
        Pause,
    }

    public static class GameplayUITracker
    {
        private static Stack<UIOperationStatus> UIOperations = new();

        public static void ClearHistory() => UIOperations.Clear();

        public static void PushStatus(UIOperationStatus status) => UIOperations.Push(status);

        public static UIOperationStatus GetCurrentStatus() => UIOperations.TryPeek(out var status) ? status : UIOperationStatus.None;
        
        public static UIOperationStatus BackToPreviousStatus() => UIOperations.Pop();

        public static bool PopStatusIfEqual(UIOperationStatus status)
        {
            if (UIOperations.Peek() == status)
            {
                UIOperations.Pop(); return true;
            }
            return false;
        }
    }
}