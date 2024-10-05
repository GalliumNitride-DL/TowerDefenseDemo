using DG.Tweening;

using System;
using System.Collections.Generic;

using TMPro;

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
        private UIOperationStatus UIStatus => GameplayUITracker.GetCurrentStatus();
        private GameObject currentlyDeployingPrefab;
        private DefenseTowerBase currentlySelectedTower;

        [SerializeField] private RectTransform detailPanel;
        [SerializeField] private TextMeshProUGUI rangeText;
        [SerializeField] private TextMeshProUGUI DPSText;
        [SerializeField] private TextMeshProUGUI sellButtonText;
        [SerializeField] private float animDuration;
        [SerializeField] private float towerHeight;

        private const int RaycastLayerMask = ~0 ^ (1 << 2); // Ignore Layer 2, which is "Ignore Raycast"

#region UI Events
        public void TryUpdateDeployStatus(GameObject towerPrefab)
        {
            if (GameController.Instance.State != GameState.Deploying) { return; }
            if (UIStatus == UIOperationStatus.DeployTower)
            {
                Destroy(currentlyDeployingPrefab);
                currentlyDeployingPrefab = null;
                GameplayUITracker.BackToPreviousStatus();
                return;
            }

            if (UIStatus == UIOperationStatus.DeployIdle)
            {
                GameplayUITracker.PushStatus(UIOperationStatus.DeployTower);
                currentlyDeployingPrefab = Instantiate(towerPrefab);
                return;
            }
        }

        public void Sell()
        {
            if (UIStatus == UIOperationStatus.SelectTower)
            {
                sellButtonText.text = "CONFIRM?";
                GameplayUITracker.PushStatus(UIOperationStatus.ConfirmSell);
            }
            else if (UIStatus == UIOperationStatus.ConfirmSell)
            {
                var tower = currentlySelectedTower.price;
                GlobalData.Money += currentlySelectedTower.price / 2;
                var t = currentlySelectedTower.transform.position;
                var c = new Vector2Int(Mathf.RoundToInt(t.x / 10f), Mathf.RoundToInt(t.z / 10f));
                MapBuilder.RemoveTowerAt(c);
                GameplayUITracker.BackToPreviousStatus();
                DeselectCurrentTower(true);
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
            InitPanel();
        }

        private void DeselectCurrentTower(bool destroy = false)
        {
            if (!GameplayUITracker.PopStatusIfEqual(UIOperationStatus.SelectTower)) { return; }
            currentlySelectedTower.OnTowerDeselected();
            if (destroy)
            {
                Destroy(currentlySelectedTower.gameObject);
            }
            currentlySelectedTower = null;
        }

        private void InitPanel()
        {
            if (UIStatus != UIOperationStatus.SelectTower) { return; }

            rangeText.text = (currentlySelectedTower.range / 10f).ToString();
            DPSText.text = currentlySelectedTower.GetDPS().ToString();
            sellButtonText.text = $"Sell for ${currentlySelectedTower.price / 2}";
        }

        private void DeconfirmSell()
        {
            if (!GameplayUITracker.PopStatusIfEqual(UIOperationStatus.ConfirmSell)) { return; }
            sellButtonText.text = $"Sell for ${currentlySelectedTower.price / 2}";
        }

        private void Update()
        {
            if (GameController.Instance.State != GameState.Deploying) { return; }
            var mousePressed = Input.GetMouseButtonDown(0);
            if (UIStatus == UIOperationStatus.DeployTower)
            {
                var c = RaycastFromCamera();
                var previewCoord = new Vector3(c.x * GlobalData.BlockLength, towerHeight, c.y * GlobalData.BlockLength);

                if (MapBuilder.CanDeployTowerAt(c)) { currentlyDeployingPrefab.transform.position = previewCoord; }

                if (mousePressed)
                {
                    var tower = currentlyDeployingPrefab.GetComponent<DefenseTowerBase>();
                    
                    if (GlobalData.Money >= tower.price && MapBuilder.TryDeployTowerAt(tower, c))
                    {
                        GlobalData.Money -= tower.price;
                        GameplayUITracker.BackToPreviousStatus();
                        currentlyDeployingPrefab = null;
                    }
                }
            }
            else if (mousePressed && !EventSystem.current.IsPointerOverGameObject())
            {
                if (UIStatus == UIOperationStatus.SelectTower)
                {
                    DeselectCurrentTower();
                }
                else if (UIStatus == UIOperationStatus.DeployIdle)
                {
                    var c = RaycastFromCamera();
                    var tower = MapBuilder.GetTowerAt(c);

                    if (tower)
                    {
                        SelectTower(tower);
                        // Todo: UI
                    }
                }
                else if (UIStatus == UIOperationStatus.ConfirmSell)
                {
                    DeconfirmSell();
                }
            }
        }
    }
}