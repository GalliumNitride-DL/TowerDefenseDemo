using Cysharp.Threading.Tasks;

using DG.Tweening;

using System;
using System.Collections.Generic;

using TowerDefenseDemo.Persistence;

using UnityEngine;

namespace TowerDefenseDemo.Gameplay
{

    /// <summary>
    /// MapBuilder maintains each tile and defense tower on the map
    /// </summary>
    public static class MapBuilder
    {
        private const int MapLength = 20;
        private const float AnimDuration = 1.5f;
        private static bool[,] isRoadFlag;
        private static Dictionary<Vector2Int, DefenseTowerBase> towers = new();

        public static async UniTask BuildMap(LevelData levelData) // Determine if it's road or empty, and instantiate tiles
        {
            isRoadFlag = new bool[MapLength, MapLength];
            towers.Clear();

            var segments = levelData.enemyRoadSegments;
            for (int i = 0; i < segments.Count - 1; i++)
            {
                var dir = segments[i + 1] - segments[i];
                var d = Mathf.Abs(dir.x + dir.y); // distance (measured in 10 unity units)
                dir /= d; // normalize
                for (int j = 0; j <= d; j++)
                {
                    var c = segments[i] + j * dir;
                    Debug.Log(c);
                    isRoadFlag[c.x, c.y] = true;
                }
            }

            var parent = new GameObject("Tiles").transform;
            
            for (int i = 0; i < MapLength; i++)
            {
                for (int j = 0; j < MapLength; j++)
                {
                    GameObject tile;
                    if (!isRoadFlag[i, j])
                    {
                        tile = levelData.emptyTile;
                    }
                    else if (i == segments[0].x && j == segments[0].y)
                    {
                        tile = levelData.spawnPointTile;
                    }
                    else if (i == segments[segments.Count - 1].x && j == segments[segments.Count - 1].y)
                    {
                        tile = levelData.HeadquartersTile;
                    }
                    else continue;
                    var p = new Vector3(i * GlobalData.BlockLength, tile.transform.localScale.y / 2, j * GlobalData.BlockLength);
                    var tileObject = GameObject.Instantiate(tile, p, Quaternion.identity, parent);

                    var s = tileObject.transform.localScale;
                    tileObject.transform.localScale = Vector3.zero;
                    tileObject.transform.DOScale(s, AnimDuration);

                    await UniTask.Delay(1);
                }
            }
        }

        public static bool CanDeployTowerAt(Vector2Int c) => c.x < MapLength && c.y < MapLength && !isRoadFlag[c.x, c.y] && !towers.ContainsKey(c);

        public static bool IsTowerOccupiedAt(Vector2Int c) => towers.ContainsKey(c);

        public static DefenseTowerBase GetTowerAt(Vector2Int c) => towers.TryGetValue(c, out var tower) ? tower : null;

        public static bool RemoveTowerAt(Vector2Int c) => towers.Remove(c);

        public static bool TryDeployTowerAt(DefenseTowerBase tower, Vector2Int c)
        {
            if (!CanDeployTowerAt(c)) return false;
            towers[c] = tower;
            tower.OnDeploy();
            return true;
        }

    }
}