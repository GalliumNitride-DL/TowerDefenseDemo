using System;
using System.Collections.Generic;

using TowerDefenseDemo.UI;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TowerDefenseDemo.Gameplay
{

    /// <summary>
    /// DeployController is responsible for the tower select & ugrade raycast in the Deploy phase.
    /// </summary>
    public class DeployController : SingletonBehaviour<DeployController>
    {
        public bool isDeploying => GameplayUITracker.GetCurrentStatus() == UIOperationStatus.DeployTower;
        public bool isSelected => GameplayUITracker.GetCurrentStatus() == UIOperationStatus.SelectTower;
        private GameObject currentlyDeployingPrefab;
        private DefenseTowerBase currentlySelectedTower;

        [SerializeField] private float towerHeight;

        private const int RaycastLayerMask = ~0 ^ (1 << 2); // Ignore Layer 2, which is "Ignore Raycast"

#region UI Events
        public void TryUpdateDeployStatus(GameObject towerPrefab)
        {
            if (GameController.Instance.State != GameState.Deploying) { return; }
            if (isDeploying && currentlyDeployingPrefab == towerPrefab)
            {
                Destroy(currentlyDeployingPrefab);
                currentlyDeployingPrefab = null;
                GameplayUITracker.BackToPreviousStatus();
                return;
            }

            // TODO: Add Money Detection

            if (GameplayUITracker.GetCurrentStatus() == UIOperationStatus.DeployIdle)
            {
                GameplayUITracker.PushStatus(UIOperationStatus.DeployTower);
                currentlyDeployingPrefab = Instantiate(towerPrefab);
                return;
            }

            if (GameplayUITracker.GetCurrentStatus() == UIOperationStatus.SelectTower)
            {
                DeselectCurrentTower();
                return;
            }
        }
#endregion

        private Vector2Int RaycastFromCamera()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit, Mathf.Infinity, RaycastLayerMask, QueryTriggerInteraction.Ignore);

            var hitPosition = hit.point;
            return new Vector2Int(Mathf.RoundToInt(hitPosition.x /GlobalData.BlockLength), Mathf.RoundToInt(hitPosition.z /GlobalData.BlockLength));
        }

        private void SelectTower(DefenseTowerBase tower)
        {
            if (GameplayUITracker.GetCurrentStatus() != UIOperationStatus.DeployIdle) { return; }
            GameplayUITracker.PushStatus(UIOperationStatus.SelectTower);
            currentlySelectedTower = tower;
            currentlySelectedTower.OnTowerSelected();
        }

        private void DeselectCurrentTower()
        {
            if (!GameplayUITracker.PopStatusIfEqual(UIOperationStatus.SelectTower)) { return; }
            currentlySelectedTower.OnTowerDeselected();
            currentlySelectedTower = null;
        }

        private void Update()
        {
            if (GameController.Instance.State != GameState.Deploying) { return; }
            var mousePressed = Input.GetMouseButtonDown(0);
            if (isDeploying)
            {
                var c = RaycastFromCamera();
                var previewCoord = new Vector3(c.x * GlobalData.BlockLength, towerHeight, c.y * GlobalData.BlockLength);

                if (MapBuilder.CanDeployTowerAt(c)) { currentlyDeployingPrefab.transform.position = previewCoord; }

                if (mousePressed)
                {
                    if (MapBuilder.TryDeployTowerAt(currentlyDeployingPrefab, c))
                    {
                        GameplayUITracker.BackToPreviousStatus();
                        currentlyDeployingPrefab = null;
                    }
                }
            }
            else if (mousePressed && !EventSystem.current.IsPointerOverGameObject())
            {
                if (isSelected)
                {
                    DeselectCurrentTower();
                }
                else
                {
                    var c = RaycastFromCamera();
                    var tower = MapBuilder.GetTowerAt(c);

                    if (tower)
                    {
                        SelectTower(tower);
                        // Todo: UI
                    }
                }
                Debug.Log(isSelected);
            }
        }
    }
}