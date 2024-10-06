using Cysharp.Threading.Tasks;

using DG.Tweening;

using UnityEngine;


namespace TowerDefenseDemo.Gameplay
{
    public class Rocket : DefenseTowerBase
    {
        [SerializeField] private GameObject rocket, rocketRangeIndicator;
        [SerializeField] private int rocketDamage;
        [SerializeField] private float rocketRange;
        [SerializeField] private float rocketSpeed;
        [SerializeField] private float deviationWeight;
        [SerializeField] private float explodeThreshold;
        [SerializeField] private float maxLaunchTime;

        private Collider[] results = new Collider[100]; // buffer for NonAlloc
        private bool isLaunching;
        private float launchTime;
        private Vector3 rocketPos;

        public override float GetDPS() => 0;

        private void Start()
        {
            rocketPos = rocket.transform.localPosition;
        }

        protected override async void Attack()
        {
            if (!isLaunching)
            {
                await LaunchRocket();
            }
        }

        private async UniTask LaunchRocket()
        {
            
            launchTime = Time.time;
            isLaunching = true;
            var speed = Vector3.up * rocketSpeed;

            rocket.transform.localPosition = rocketPos;
            rocket.SetActive(true);

            while (Time.time <= launchTime + maxLaunchTime && lockedEnemy && (GameController.Instance.State == GameState.AFK || GameController.Instance.State == GameState.Paused))
            {
                var dir = lockedEnemy.transform.position - rocket.transform.position;
                var dv = (dir.normalized - speed.normalized) * deviationWeight; // Acceleration

                speed = (speed + dv * Time.deltaTime).normalized * rocketSpeed;
                
                rocket.transform.position += speed * Time.deltaTime;
                rocket.transform.rotation = Quaternion.LookRotation(speed);

                if (dir.sqrMagnitude <= explodeThreshold * explodeThreshold)
                {
                    break;
                }
                else await UniTask.WaitForFixedUpdate();
            }

            if (lockedEnemy && (GameController.Instance.State == GameState.AFK || GameController.Instance.State == GameState.Paused))
            {
                IndicatorPulse();
                CameraController.Instance.ShakeCamera(1f, 1f);

                Physics.OverlapSphereNonAlloc(rocket.transform.position, rocketRange, results, -1, QueryTriggerInteraction.Ignore);
                foreach (var result in results)
                {
                    if (!result) { continue; }
                    var e = result.GetComponent<Enemy>();
                    if (!e) { continue; }
                    e.TakeDamage(rocketDamage, damageType);
                }
            }
            rocket.SetActive(false);
            
            isLaunching = false;
        }

        private void IndicatorPulse()
        {
            rocketRangeIndicator.SetActive(true);
            rocketRangeIndicator.transform.localScale = new(rocketRange * 2, 0.1f, rocketRange * 2);
            rocketRangeIndicator.transform.position = rocket.transform.position;

            rocketRangeIndicator.transform.DOScale(new Vector3(0f, 0.1f, 0f), 0.5f).OnComplete(() => rocketRangeIndicator.SetActive(false));
        }
    }
}