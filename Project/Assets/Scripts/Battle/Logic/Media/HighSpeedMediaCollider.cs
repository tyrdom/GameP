using Battle.Logic.AllManager;
using UnityEngine;

namespace Battle.Logic.Media
{
    public class HighSpeedMediaCollider : MonoBehaviour
    {
        public LayerMask layerMask;

        public float radius;

        public Vector3 offset;


        public void UpATick()
        {
            var mediaMono = GetComponent<MediaMono>();

            var movement = mediaMono.nowVelocity * BLogicMono.UpTickDeltaTimeSec;

            var startPt = mediaMono.transform.position + offset;

            if (radius <= 0)
            {
                Physics.Raycast(startPt, transform.forward, out var hitInfo1, movement.magnitude, layerMask);
                mediaMono.OnCastHit(hitInfo1);
            }
            else
            {
                if (Physics.SphereCast(startPt, radius, transform.forward, out var hitInfo2, movement.magnitude,
                        layerMask))
                {
                    mediaMono.OnCastHit(hitInfo2);
                }
            }
        }
    }
}