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
        private const float AnimDuration = 1f;
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
                    if (!isRoadFlag[i, j])
                    {
                        var tile = levelData.emptyTile;
                        var p = new Vector3(i * GlobalData.BlockLength, tile.transform.localScale.y / 2, j * GlobalData.BlockLength);
                        var tileObject = GameObject.Instantiate(tile, p, Quaternion.identity, parent);
                    }
                }
            }

            parent.localScale = new(1f, 0f, 1f);
            await parent.DOScaleY(1f, AnimDuration).AsyncWaitForCompletion();
        }

        public static bool CanDeployTowerAt(Vector2Int c) => c.x < MapLength && c.y < MapLength && !isRoadFlag[c.x, c.y] && !towers.ContainsKey(c);

        public static bool IsTowerOccupiedAt(Vector2Int c) => towers.ContainsKey(c);

        public static DefenseTowerBase GetTowerAt(Vector2Int c) => towers[c];

        public static bool TryDeployTowerAt(GameObject towerObject, Vector2Int c)
        {
            if (!CanDeployTowerAt(c)) return false;
            var tower = towerObject.GetComponent<DefenseTowerBase>();
            if (!tower) return false;
            towers[c] = tower;
            return true;
        }

    }
}