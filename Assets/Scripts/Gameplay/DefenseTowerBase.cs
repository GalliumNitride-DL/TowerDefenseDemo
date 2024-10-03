using DG.Tweening;

using System.Collections;
using System.Collections.Generic;

using TowerDefenseDemo.Persistence;

using UnityEngine;

namespace TowerDefenseDemo.Gameplay
{
    public abstract class DefenseTowerBase : MonoBehaviour
    {
        public int price = 100;
        [SerializeField] protected DamageType damageType = DamageType.Impact;
        [SerializeField] protected float attackInterval;
        [SerializeField] private GameObject rangeIndicator;
        [SerializeField] private float animDuration = 0.5f;

        public float range // WTF?
        {
            get => capsuleCollider.radius;
            set => capsuleCollider.radius = value;
        }
        
        protected Enemy lockedEnemy = null;
        private List<Enemy> enemiesInRange = new();
        private float elapsedTime; // For attack periods
        private CapsuleCollider capsuleCollider;

        protected virtual void OnEnable()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            GameController.Instance.StateChangeEvent.AddListener(OnGameStateChange);
        }

        protected virtual void Update()
        {
            elapsedTime += Time.deltaTime;
            if (GameController.Instance.State == GameState.AFK && lockedEnemy != null && elapsedTime >= attackInterval)
            {
                Attack();
                elapsedTime = 0f;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var e = other.GetComponent<Enemy>();
            if (!e || GameController.Instance.State != GameState.AFK) { return; }
            e.OnDie.AddListener(OnEnemyDied);
            enemiesInRange.Add(e);
            if (enemiesInRange.Count == 1) { Lock(); }
        }

        private void OnTriggerExit(Collider other)
        {
            var e = other.GetComponent<Enemy>();
            if (!e) { return; }
            e.OnDie.RemoveListener(OnEnemyDied);
            enemiesInRange.Remove(e);
            if (e == lockedEnemy) { Lock(); }
        }

        protected virtual void OnDisable()
        {
            GameController.Instance.StateChangeEvent.RemoveListener(OnGameStateChange);
        }

        protected abstract void Attack();

        public abstract float GetDPS();

        private void Lock()
        {
            if (enemiesInRange.Count == 0)
            {
                lockedEnemy = null;
                return;
            }
            if (enemiesInRange.Count == 1)
            {
                lockedEnemy = enemiesInRange[0];
                return;
            }

            enemiesInRange.Sort((lhs, rhs) => 
            {
                if (lhs.currentSegmentIndex != rhs.currentSegmentIndex) return -lhs.currentSegmentIndex.CompareTo(rhs.currentSegmentIndex);
                else return -(lhs.currentSegmentTime * lhs.speed).CompareTo(rhs.currentSegmentTime * rhs.speed);
            });
            lockedEnemy = enemiesInRange[0];
        }

        protected virtual void OnEnemyDied(Enemy enemy)
        {
            enemiesInRange.Remove(enemy);
            if (enemy == lockedEnemy) { Lock(); }
        }

        private void OnGameStateChange(GameState newState)
        {
            switch (newState)
            {
                case GameState.Deploying:
                case GameState.LevelFailed:
                case GameState.LevelCompleted:
                    enemiesInRange.Clear();
                    lockedEnemy = null;
                    break;
            }
        }

        public virtual void OnTowerSelected()
        {
            rangeIndicator.transform.DOKill(true);
            rangeIndicator.SetActive(true);
            rangeIndicator.transform.DOScale(new Vector3(range * 2f, 0.1f, range * 2f), animDuration).SetEase(Ease.OutBounce);
        }

        public virtual void OnTowerDeselected()
        {
            rangeIndicator.transform.DOScale(Vector3.zero, animDuration).SetEase(Ease.InBounce).OnComplete(() => rangeIndicator.SetActive(false));
        }

        public virtual void OnDeploy()
        {
            rangeIndicator.transform.DOScale(Vector3.zero, animDuration).SetEase(Ease.InBounce).OnComplete(() => rangeIndicator.SetActive(false));
        }
    }
}