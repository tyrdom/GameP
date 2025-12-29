using System;
using Battle.Logic.AllManager;
using cfg;
using cfg.battle;
using QFramework;
using UnityEngine;

namespace Battle.Logic.Media
{
    public class MediaMono : MonoBehaviour
    {
        static float defaultHeight = 1.0f;
        public MediaL MediaL;

        [SerializeField] private GameObject mediaView;

        private float _cosSectorAngle = -2;

        public Vector3 nowVelocity = Vector3.zero;

        public void UpATick()
        {
            transform.Translate(nowVelocity * BLogicMono.UpTickDeltaTimeSec);
        }

        public void Init(MediaCfg mediaCfg, Transform mediaPool)
        {
            var mediaCfgViewResPath = mediaCfg.ViewResPath;
            var loadMediaViewAsset = ResourceMgr.Instance.LoadMediaViewAsset(mediaCfgViewResPath);
            var instantiate = Instantiate(loadMediaViewAsset, Vector3.zero, Quaternion.identity, mediaPool);

            var component = GetComponent<Collider>();
            if (component != null)
            {
                Destroy(component);
            }

            switch (mediaCfg.ColliderShapeType)
            {
                case ColliderShape.None:
                    break;
                case ColliderShape.Sphere:
                    var sc = gameObject.AddComponent<SphereCollider>();
                    sc.center = new Vector3(mediaCfg.ColliderParam[0] / 1000f, defaultHeight / 2,
                        mediaCfg.ColliderParam[1] / 1000f);
                    sc.radius = mediaCfg.ColliderParam[2] / 1000f;
                    sc.isTrigger = true;
                    break;
                case ColliderShape.Box:
                    var bc = gameObject.AddComponent<BoxCollider>();
                    bc.center = new Vector3(mediaCfg.ColliderParam[0] / 1000f, defaultHeight,
                        mediaCfg.ColliderParam[1] / 1000f);
                    bc.size = new Vector3(mediaCfg.ColliderParam[2] / 1000f, defaultHeight / 2,
                        mediaCfg.ColliderParam[3] / 1000f);
                    bc.isTrigger = true;
                    break;
                case ColliderShape.Sector:
                    var ccc = gameObject.AddComponent<CapsuleCollider>();
                    ccc.center = new Vector3(mediaCfg.ColliderParam[0] / 1000f, defaultHeight / 2,
                        mediaCfg.ColliderParam[1] / 1000f);
                    ccc.radius = mediaCfg.ColliderParam[2] / 1000f;
                    ccc.height = defaultHeight;
                    _cosSectorAngle = Mathf.Cos(mediaCfg.ColliderParam[3] / 2f * Mathf.Deg2Rad);
                    ccc.isTrigger = true;
                    break;

                case ColliderShape.SphereCast:
                    var scc = gameObject.AddComponent<SpeedMediaCollider>();
                    scc.offset = new Vector3(mediaCfg.ColliderParam[0] / 1000f, defaultHeight / 2,
                        mediaCfg.ColliderParam[1] / 1000f);
                    scc.radius = mediaCfg.ColliderParam[2] / 1000f;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            instantiate.SetActive(false);
        }

        public bool TargetSectorFilter(Vector3 targetPos)
        {
            if (_cosSectorAngle < -1)
            {
                return true;
            }

            var transform1 = transform;
            var transformPosition = targetPos - transform1.position;
            var direction = new Vector3(transformPosition.x, 0, transformPosition.z).normalized;
            var cosValue = Vector3.Dot(direction, transform1.forward);
            return cosValue >= _cosSectorAngle;
        }

        public void OnCastHit(RaycastHit hitInfo)
        {
            var bodyMono = hitInfo.collider.GetComponent<BodyMono>();

            if (bodyMono != null)
            {
                bodyMono.BodyL.OnMediaHit(MediaL);
            }
        }

        public void Release()
        {
            nowVelocity = Vector3.zero;
            mediaView.SetActive(false); //todo fade out
        }
    }
}