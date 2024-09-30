using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseDemo.Persistence
{
    public class LevelData : ScriptableObject
    {
        public List<Vector2Int> enemyRoadSegments = new();

        public List<WaveInfo> waveInfos = new();
    }
}