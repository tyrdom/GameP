using System;
using Battle.Logic.AllManager;
using cfg;
using cfg.battle;
using Configs;
using UnityEngine;

namespace Battle.Logic.Media
{
    public class MediaL
    {
        public BodyL Owner = null;
        public int InstanceId = -1;
        public readonly MediaCfg Cfg;

        public readonly MediaMono MediaMono;

        private AtkDirType _atkDirType;
        private int _lifeTime = 0;
        private int _hitCd = 0;

        private int _hitCount = 0;


        private int _movIdx = 0;

        public MediaL(MediaMono mediaMono, MediaCfg mediaCfg)
        {
            MediaMono = mediaMono;
            mediaMono.MediaL = this;
            Cfg = mediaCfg;
        }


        public static MediaL Alloc(InstanceMediaInfo instanceCharInfo, OwnerInfo ownerInfo)
        {
            var allocate = BattleLogicMgr.Instance.MediaPool.Allocate(instanceCharInfo.CfgId);
            allocate.Instance(instanceCharInfo, ownerInfo);
            return allocate;
        }

        public static void Release(MediaL obj)
        {
            obj.Reset();
            obj.MediaMono.Release();
            BattleLogicMgr.Instance.MediaList.Remove(obj);
        }

        private void Instance(InstanceMediaInfo instanceMediaInfo, OwnerInfo ownerInfo)
        {
            InstanceId = instanceMediaInfo.InstanceId;
            Owner = ownerInfo.Owner;
            Owner.TempMediaList.Add(this);
            MediaMono.gameObject.name = $"{Cfg.Alias}_{InstanceId}";
            _atkDirType = instanceMediaInfo.AtkDirType;
            _lifeTime = Cfg.Duration;
            _hitCount = Cfg.HitMaxCount > 0 ? Cfg.HitMaxCount : int.MaxValue;
        }

        private void Reset()
        {
            _lifeTime = 0;
            _hitCount = 0;
            Owner = null;
            InstanceId = -1;
            _movIdx = 0;
        }

        public static MediaL CreateBlank(int cfgId)
        {
            var mediaCfg = GameConfigs.Instance.Tables.TbMediaCfg.GetById(cfgId);
            if (mediaCfg == null)
            {
                throw new System.Exception("MediaCfg not found");
            }

            var mediaMonoInPool = BLogicMono.Instance.CreateMediaMonoInPool(mediaCfg);
            var mediaL = new MediaL(mediaMonoInPool, mediaCfg);

            return mediaL;
        }


        public void UpATick()
        {
            if (InstanceId < 0)
            {
                return;
            }

            HighSpeedColliderCheck();

            var alive = CheckLifeTime();
            if (alive)
            {
                DoMovement();
            }
            else
            {
                Recycle();
            }

            MediaMono.UpATick();
            _lifeTime += BattleLogicMgr.upTickDeltaTimeMs;
            if (_hitCd > 0)
            {
                _hitCd -= BattleLogicMgr.upTickDeltaTimeMs;
            }
        }

        private void Recycle()
        {
            BattleLogicMgr.Instance.MediaPool.Free(Cfg.Id, this);
        }

        private bool CheckLifeTime()
        {
            var b = _lifeTime > 0;
            var b1 = _hitCount > 0;
            return b && b1;
        }

        private void DoMovement()
        {
            MovementChangeCfg movementChangeCfg = Cfg.Movement[_movIdx];
            if (movementChangeCfg.ChangeTime >= Cfg.Duration - _lifeTime)
            {
                _movIdx++;
                ChangeMovement(movementChangeCfg);
            }
        }

        private void ChangeMovement(MovementChangeCfg movementChangeCfg)
        {
            MediaMono.nowVelocity = new Vector3(movementChangeCfg.XInt / 1000f, 0, movementChangeCfg.ZInt / 1000f);
        }

        private void HighSpeedColliderCheck()
        {
            var highSpeedMedia = MediaMono.GetComponent<HighSpeedMediaCollider>();
            if (highSpeedMedia != null)
            {
                highSpeedMedia.UpATick();
            }
        }


        public void GetOwner(out BodyL owner)
        {
            owner = Owner;
        }


        public bool CanLEffect()
        {
            return _hitCount > 0 && _hitCd <= 0;
        }

        public void OnEffect()
        {
            _hitCount--;
            _hitCd += Cfg.HitGapTime;
        }
    }

    public readonly struct InstanceMediaInfo
    {
        public readonly int CfgId;
        public readonly int InstanceId;
        public readonly AtkDirType AtkDirType;

        public InstanceMediaInfo(int cfgId, int instanceId, AtkDirType atkDirType)
        {
            CfgId = cfgId;
            InstanceId = instanceId;
            AtkDirType = atkDirType;
        }
    }
}