using System;
using System.Collections.Generic;
using Battle.Logic.AllManager;
using UnityEngine;

namespace Battle.Logic.Media
{
    public class SpeedMediaCollider : MonoBehaviour
    {
        public LayerMask layerMask;

        public float radius;

        public Vector3 offset;

        private MediaL _mediaL;

        // 1. 预分配缓冲区
        private RaycastHit[] _hitsBuffer = new RaycastHit[20];

        // 2. 创建静态比较器，避免每次排序都new对象（进一步优化性能）
        private static readonly IComparer<RaycastHit> DistanceComparer = new DistanceComparer();

        public void UpATick()
        {
            var mediaMono = GetComponent<MediaMono>();

            var movement = mediaMono.nowVelocity * BLogicMono.UpTickDeltaTimeSec;

            var startPt = mediaMono.transform.position + offset;
            var b = radius <= 0;

            if (_mediaL.TotalHitCount <= 0) return;
            if (_mediaL.TotalHitCount <= 1)
            {
                if (b)
                {
                    Physics.Raycast(startPt, transform.forward, out RaycastHit hitInfo1, movement.magnitude,
                        layerMask);
                    mediaMono.OnCastHit(hitInfo1);
                }
                else
                {
                    if (Physics.SphereCast(startPt, radius, transform.forward, out RaycastHit hitInfo2,
                            movement.magnitude,
                            layerMask))
                    {
                        mediaMono.OnCastHit(hitInfo2);
                    }
                }
            }
            else
            {
                var hitCount = b
                    ? Physics.RaycastNonAlloc(startPt, transform.forward, _hitsBuffer,
                        movement.magnitude,
                        layerMask)
                    : Physics.SphereCastNonAlloc(startPt, radius, transform.forward, _hitsBuffer,
                        movement.magnitude,
                        layerMask);
                if (hitCount == _hitsBuffer.Length)
                {
                    ResizeBuffer();
                    // 注意：缓冲区已变大，需要重新进行射线检测来获取完整结果
                    // 或者根据游戏逻辑，你可以选择只记录警告，本次忽略超出的部分
                    Debug.LogWarning(
                        $"缓冲区已满，已从 {_hitsBuffer.Length / 2} 扩容至 {_hitsBuffer.Length}。本次检测可能不完整，建议重新检测。");
                    // 这里选择重新检测以确保当帧逻辑正确
                    hitCount = b
                        ? Physics.RaycastNonAlloc(startPt, transform.forward, _hitsBuffer,
                            movement.magnitude,
                            layerMask)
                        : Physics.SphereCastNonAlloc(startPt, radius, transform.forward, _hitsBuffer,
                            movement.magnitude,
                            layerMask);
                }

                if (hitCount <= 0) return;
                var min = Math.Min(_mediaL.TotalHitCount, hitCount);
                Array.Sort(_hitsBuffer, 0, min, DistanceComparer);
                for (int i = 0; i < min; i++)
                {
                    var raycastHit = _hitsBuffer[i];
                    mediaMono.OnCastHit(raycastHit);
                }
            }
        }

        private void ResizeBuffer()
        {
            int newSize = _hitsBuffer.Length * 2; // 常见的扩容因子是2
            RaycastHit[] newBuffer = new RaycastHit[newSize];
            // 可选：将旧数据拷贝到新数组（但对于射线结果通常不需要，因为会立即用新数据覆盖）
            // System.Array.Copy(_hitsBuffer, newBuffer, _hitsBuffer.Length);
            _hitsBuffer = newBuffer; // 替换引用
            Debug.Log($"射线命中缓冲区已扩容至: {newSize}");
        }

        public void SetL(MediaL mediaL)
        {
            _mediaL = mediaL;
        }
    }

    internal class DistanceComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit x, RaycastHit y)
        {
            var distanceComparison = x.distance.CompareTo(y.distance);
            if (distanceComparison != 0) return distanceComparison;
            var xInstanceId = x.transform.TryGetComponent<BodyLMono>(out var xMono) ? xMono.BodyL.InstanceId : 0;
            var yInstanceId = y.transform.TryGetComponent<BodyLMono>(out var yMono) ? yMono.BodyL.InstanceId : 0;
            var colliderInstanceIDComparison = xInstanceId.CompareTo(yInstanceId);
            return colliderInstanceIDComparison != 0
                ? colliderInstanceIDComparison
                : x.triangleIndex.CompareTo(y.triangleIndex);
        }
    }
}