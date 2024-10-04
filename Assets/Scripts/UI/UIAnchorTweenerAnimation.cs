using DG.Tweening;

using UnityEngine;

namespace TowerDefenseDemo.UI
{
    public class UIAnchorTweenerAnimation : UIAnimation
    {
        [SerializeField] private Vector3 closePos;
        [SerializeField] private Vector3 openPos;
        [SerializeField] private float duration;
        [SerializeField] private AnimationCurve inCurve;
        [SerializeField] private AnimationCurve outCurve;
        private RectTransform rectTransform;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        public override void PlayIn() => rectTransform.DOAnchorPos3D(openPos, duration).SetEase(inCurve).OnComplete(InCompleteCallback);

        public override void PlayOut() => rectTransform.DOAnchorPos3D(closePos, duration).SetEase(outCurve).OnComplete(OutCompleteCallback);
    }
}