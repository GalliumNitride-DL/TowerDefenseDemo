using Cysharp.Threading.Tasks;

using DG.Tweening;

using System;
using System.Collections.Generic;

using TowerDefenseDemo.Persistence;

using UnityEngine;

namespace TowerDefenseDemo.Gameplay
{
    public static class MapBuilder
    {
        private const int MapLength = 20;
        private const float AnimDuration = 1f;
        public static bool[,] isRoadFlag = new bool[MapLength, MapLength];

        public static async UniTask BuildMap(LevelData levelData) // Determine if it's road or empty, and instantiate tiles
        {
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
                        var p = new Vector3(i * GameController.BlockLength, tile.transform.localScale.y / 2, j * GameController.BlockLength);
                        var tileObject = GameObject.Instantiate(tile, p, Quaternion.identity, parent);
                        DoAnimation(tileObject, tile.transform.localScale.y);
                    }
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(AnimDuration), ignoreTimeScale: false);
        }

        private static void DoAnimation(GameObject tile, float height)
        {
            var s = tile.transform.localScale;
            s.y = 0;
            tile.transform.localScale = s;

            tile.transform.DOScaleY(height, AnimDuration);
        }
    }
}