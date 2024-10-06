using Cysharp.Threading.Tasks;

using DG.Tweening;

using TowerDefenseDemo.Persistence;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefenseDemo.Menu
{
    public class MainMenuUIController : MonoBehaviour
    {
        [SerializeField] private GameObject currentlyActivePanel;
        [SerializeField] private float animDuration;

        private bool isPlaying = false;

        public async void TransitionTo(GameObject targetPanel)
        {
            if (isPlaying) { return; }
            isPlaying = true;
            await transform.DORotate(new Vector3(0f, 90f, 0f), animDuration / 2).SetEase(Ease.InSine).AsyncWaitForCompletion();
            currentlyActivePanel.SetActive(false);
            transform.eulerAngles = new Vector3(0f, -90f, 0f);
            targetPanel.SetActive(true);
            currentlyActivePanel = targetPanel;
            await transform.DORotate(Vector3.zero, animDuration / 2).SetEase(Ease.OutSine).AsyncWaitForCompletion();
            isPlaying = false;
        }

        public void Exit() => Application.Quit();

        public void EnterLevel(LevelData levelData)
        {
            GlobalData.CurrentLevelData = levelData;
            SceneManager.LoadScene("Level01");
        }


    }
}