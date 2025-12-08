using Battle.Logic.Media;
using cfg.battle;
using Configs;
using QFramework;
using UnityEngine;

namespace Battle.Logic.AllManager
{
    public class BLogicMono : MonoSingleton<BLogicMono>
    {
        public static float UpTickDeltaTimeSec;

        public float lastTime = 0;

        [SerializeField] private Transform bodyPool;
        [SerializeField] private Transform bodyRoot;
        [SerializeField] private Transform mediaPool;
        [SerializeField] private Transform mediaRoot;

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            BattleLogicMgr.Instance.Init();
            UpTickDeltaTimeSec = GameConfigs.Instance.Tables.TbCommonCfg.TickTime/1000f;
            BattleLogicMgr.upTickDeltaTimeMs = GameConfigs.Instance.Tables.TbCommonCfg.TickTime;
        }

        private void Update()
        {
            lastTime += Time.deltaTime * 1000;


            while (lastTime >= BattleLogicMgr.upTickDeltaTimeMs)
            {
                lastTime -= BattleLogicMgr.upTickDeltaTimeMs;
                UpdateLogic();
            }
        }

        private static void UpdateLogic()
        {
            BattleLogicMgr.Instance.GoATick();
        }

        public BodyMono CreateBodyMonoInPool(BodyCfg dataById)
        {
            var resPath = dataById.ResPath;
            var loadBodyAsset = ResourceMgr.LoadBodyAsset(resPath);
            var bodyMono = Instantiate(loadBodyAsset, Vector3.zero, Quaternion.identity, bodyPool);
            bodyMono.gameObject.SetActive(false);
            return bodyMono.GetComponent<BodyMono>();
        }

        public MediaMono CreateMediaMonoInPool(MediaCfg mediaCfg)
        {
            var mediaMono = ResourceMgr.LoadMediaLAsset("Bullet");

            var newMedia = Instantiate(mediaMono, Vector3.zero, Quaternion.identity, mediaPool);

            var mediaMonoInPool = newMedia.GetComponent<MediaMono>();
            mediaMonoInPool.Init(mediaCfg, mediaPool);
            newMedia.SetActive(false);
            return mediaMonoInPool;
        }
    }

    public class ResourceMgr : MonoSingleton<ResourceMgr>
    {
        public static GameObject LoadBodyAsset(string resPath)
        {
            var go = Resources.Load<GameObject>($"Body/{resPath}");
            return go;
        }

        public static GameObject LoadMediaLAsset(string resPath)
        {
            var go = Resources.Load<GameObject>($"MediaL/{resPath}");
            return go;
        }

        public GameObject LoadMediaViewAsset(string mediaCfgViewResPath)
        {
            return Resources.Load<GameObject>($"MediaView/{mediaCfgViewResPath}");
        }
    }
}