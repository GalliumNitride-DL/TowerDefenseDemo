using System.Collections.Generic;

using TowerDefenseDemo.Persistence;

using UnityEngine;


namespace TowerDefenseDemo.Gameplay
{
    public enum SpawnState
    {
        UnInitialized,
        WaitingSpawn,
        Spawning,
        SpawnComplete
    }

    public class EnemySpawner : SingletonBehaviour<EnemySpawner>
    {
        public SpawnState State => state;
        private SpawnState state = SpawnState.UnInitialized;

        private Transform spawnParent;
        private SortedList<float, GameObject> spawnList = new();
        private IEnumerator<KeyValuePair<float, GameObject>> iter;
        private float spawnStartTime = 0;
        

        /// <summary>
        /// Initializes the internal Spawn List of the enemies with the given Wave Information.
        /// </summary>
        /// <param name="waveInfo">The WaveInfo given</param>
        public void InitializeSpawner(WaveInfo waveInfo)
        {
            spawnList.Clear();
            spawnStartTime = 0;
            if (spawnParent == null) { spawnParent = new GameObject("Enemies").transform; }
            var subWaves = waveInfo.subWaves;
            foreach (var sw in subWaves)
            {
                float t = sw.spawnDelay;
                for (int i = 0; i < sw.spawnCount; i++)
                {
                    var ts = t + i * sw.spawnDelay;
                    while (spawnList.ContainsKey(ts)) { ts += 0.01f; } // handle duplicate keys
                    spawnList[ts] = sw.enemyPrefab;
                }
            }
            iter = spawnList.GetEnumerator();
            iter.MoveNext(); // Move IEnumerator to the first element of SortedList
            state = SpawnState.WaitingSpawn;
        }

        public void StartSpawn()
        {
            if (state == SpawnState.WaitingSpawn) // prevent unwanted calls
            {
                spawnStartTime = Time.time;
                state = SpawnState.Spawning;
            }
        }

        private void Update()
        {
            if (state == SpawnState.Spawning && GameController.Instance.State == GameState.AFK)
            {
                var currentSpawnTime = Time.time - spawnStartTime;
                while (iter.Current.Key < currentSpawnTime)
                {
                    Instantiate(iter.Current.Value, parent: spawnParent);
                    var isComplete = !iter.MoveNext();
                    if (isComplete)
                    {
                        state = SpawnState.SpawnComplete;
                        break;
                    }
                }
            }
        }
    }
}