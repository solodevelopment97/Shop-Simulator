using System;
using DG.Tweening;
using UnityEngine;

namespace Animations
{
    public static class ItemMover
    {
        /// <summary>
        /// Animasi “ghost” terbang, lalu panggil onComplete.
        /// Ghost akan dihancurkan, real–logic di onComplete.
        /// </summary>
        public static void AnimateFly(
            GameObject ghost,
            Vector3 targetPos,
            float duration,
            Action onComplete = null)
        {
            // simpan scale asli
            Vector3 originalScale = ghost.transform.localScale;

            // start kecil (50%)
            ghost.transform.localScale = originalScale;

            Sequence seq = DOTween.Sequence();
            seq.Append(ghost.transform.DOMove(targetPos, duration).SetEase(Ease.OutQuad));
            seq.Join(ghost.transform.DOScale(originalScale, duration));
            seq.OnComplete(() =>
            {
                onComplete?.Invoke();
                GameObject.Destroy(ghost);
            });
        }
    }
}
