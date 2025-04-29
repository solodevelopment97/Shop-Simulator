using System;
using DG.Tweening;
using UnityEngine;

namespace Animations
{
    public static class PickupMover
    {
        /// <summary>
        /// Animasi objek “terbang” dari posisi asal ke target (holdPoint).
        /// Setelah selesai, panggil onComplete dan hancurkan ghost.
        /// </summary>
        /// <param name="ghost">GameObject visual (ghost) yang di-animate</param>
        /// <param name="target">Transform tujuan (biasanya holdPoint pemain)</param>
        /// <param name="duration">Durasi tween dalam detik</param>
        /// <param name="onComplete">Action dijalankan setelah tween selesai</param>
        public static void AnimateToTarget(GameObject ghost, Transform target, float duration, Action onComplete)
        {
            // pastikan ghost terlepas dari parent
            ghost.transform.SetParent(null);

            // Sequence: move + rotate, bisa ditambah scale atau fade
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
