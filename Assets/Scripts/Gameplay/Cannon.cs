using Cysharp.Threading.Tasks;

using DG.Tweening;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace TowerDefenseDemo.Gameplay
{
    public class Cannon : DefenseTowerBase
    {
        [SerializeField] private GameObject bullet;
        [SerializeField] private GameObject artillery;
        [SerializeField] private int bulletDamage;
        [SerializeField] private float bulletFlyTime;

        private Vector3 bulletOriginalPos;

        protected override async void Attack()
        {
            await LaunchBullet();
        }

        public override float GetDPS() => bulletDamage / attackInterval;

        private void Start()
        {
            bulletOriginalPos = bullet.transform.localPosition;
        }

        private async UniTask LaunchBullet()
        {
            var t = 0f;
            var enemy = lockedEnemy;
            bullet.SetActive(true);
            while (enemy && t < bulletFlyTime && (GameController.Instance.State == GameState.AFK || GameController.Instance.State == GameState.Paused))
            {
                var d = Time.deltaTime / (bulletFlyTime - t);
                bullet.transform.position = Vector3.Lerp(bullet.transform.position, enemy.transform.position, d);
                t += Time.deltaTime;
                artillery.transform.LookAt(enemy.transform, Vector3.up);
                await UniTask.Yield();
            }
            
            await UniTask.WaitForFixedUpdate(); // To prevent null reference
            if (enemy && t >= bulletFlyTime && (GameController.Instance.State == GameState.AFK || GameController.Instance.State == GameState.Paused))
            {
                enemy.TakeDamage(bulletDamage, damageType);
            }
            if (GameController.Instance.State == GameState.AFK || GameController.Instance.State == GameState.Deploying)
            {
                bullet.SetActive(false);
                bullet.transform.localPosition = bulletOriginalPos;
            }
        }
    }
}