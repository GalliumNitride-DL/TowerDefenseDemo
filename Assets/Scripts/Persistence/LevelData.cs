using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefenseDemo.Persistence
{
    [CreateAssetMenu(menuName = "Persistences/LevelData")]
    public class LevelData : ScriptableObject
    {
        public List<Vector2Int> enemyRoadSegments = new();

        public List<WaveInfo> waveInfos = new();

        public int startMoney;
        public GameObject emptyTile;
        public GameObject spawnPointTile;
        public GameObject HeadquartersTile;
    }
}