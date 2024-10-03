using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace TowerDefenseDemo.Gameplay
{

    /// <summary>
    /// DeployController is responsible for the tower select & ugrade raycast in the Deploy phase.
    /// </summary>
    public class DeployController : SingletonBehaviour<DeployController>
    {
        public bool isDeploying { get; private set; } = false;
        private GameObject currentlyDeployingPrefab;

        [SerializeField] private float towerHeight;

        private const int RaycastLayerMask = ~0 ^ (1 << 2); // Ignore Layer 2, which is "Ignore Raycast"

        

#region UI Events
        public void TryUpdateDeployStatus(GameObject towerPrefab)
        {
            if (GameController.Instance.State != GameState.Deploying) { return; }
            if (isDeploying && currentlyDeployingPrefab == towerPrefab)
            {
                isDeploying = false;
                Destroy(currentlyDeployingPrefab);
                currentlyDeployingPrefab = null;
                return;
            }

            // TODO: Add Money Detection

            if (!isDeploying)
            {
                isDeploying = true;
                currentlyDeployingPrefab = Instantiate(towerPrefab);
            }
        }
#endregion

        private void Update()
        {
            if (GameController.Instance.State != GameState.Deploying) { return; }
            var mousePressed = Input.GetMouseButtonDown(0);
            if (isDeploying)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out var hit, Mathf.Infinity, RaycastLayerMask, QueryTriggerInteraction.Ignore);

                var hitPosition = hit.point;
                var c = new Vector2Int(Mathf.RoundToInt(hitPosition.x /GlobalData.BlockLength), Mathf.RoundToInt(hitPosition.z /GlobalData.BlockLength));
                var previewCoord = new Vector3(c.x * GlobalData.BlockLength, towerHeight, c.y * GlobalData.BlockLength);

                if (MapBuilder.CanDeployTowerAt(c)) { currentlyDeployingPrefab.transform.position = previewCoord; }

                if (mousePressed)
                {
                    if (MapBuilder.TryDeployTowerAt(currentlyDeployingPrefab, c))
                    {
                        isDeploying = false;
                        currentlyDeployingPrefab = null;
                    }
                }
            }
        }


    }
}