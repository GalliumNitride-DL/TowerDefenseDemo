using System.Collections;
using System.Collections.Generic;

using TowerDefenseDemo.Gameplay;
using TowerDefenseDemo.Persistence;

using UnityEngine;

namespace TowerDefenseDemo
{
    public static class GlobalData
    {
        public const float BlockLength = 10f;
        public static LevelData CurrentLevelData;
        public static int AliveEnemyCount;
        public static int Money;
    }
}