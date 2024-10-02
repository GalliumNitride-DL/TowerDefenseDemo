using System;
using System.Collections.Generic;

using TowerDefenseDemo.Gameplay;

using UnityEngine;

namespace TowerDefenseDemo.Persistence
{
    [Serializable]
    public class SubWaveInfo
    {
        public GameObject enemyPrefab;
        public float spawnDelay;
        public int spawnCount;
        public float spawnInterval;
    }

    [Serializable]
    public class WaveInfo
    {
        public List<SubWaveInfo> subWaves;
    }
}