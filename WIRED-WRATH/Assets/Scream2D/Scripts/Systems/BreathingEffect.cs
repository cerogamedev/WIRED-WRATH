using UnityEngine;
using DG.Tweening;

namespace Scream2D.Systems
{
    public class BreathingEffect : MonoBehaviour
    {
        [SerializeField] private float breatheScale = 1.05f;
        [SerializeField] private float cycleDuration = 2f;
        [SerializeField] private Ease breatheEase = Ease.InOutSine;

        private void Start()
        {
            // Bio-mechanical 'breathing' animation for platforms or background elements
            transform.DOScale(transform.localScale * breatheScale, cycleDuration)
                .SetEase(breatheEase)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}
