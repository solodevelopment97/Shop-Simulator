using System;
using DG.Tweening;
using UnityEngine;

namespace Animations
{
    public static class PickupMover
    {
        public static void AnimateToTarget(GameObject ghost, Transform target, float duration, Action onComplete)
        {
            ghost.transform.SetParent(null);
            var seq = DOTween.Sequence();
            seq.Append(ghost.transform.DOMove(target.position, duration).SetEase(Ease.InOutQuad));
            seq.Join(ghost.transform.DORotateQuaternion(target.rotation, duration).SetEase(Ease.InOutQuad));
            seq.OnComplete(() =>
            {
                onComplete?.Invoke();
                GameObject.Destroy(ghost);
            });
        }
    }
}
