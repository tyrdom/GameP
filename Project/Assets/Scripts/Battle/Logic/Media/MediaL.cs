using System;
using System.Collections.Generic;
using Battle.Logic.AllManager;
using cfg;
using cfg.battle;
using Configs;
using QFramework;
using UnityEngine;

namespace Battle.Logic.Media
{
    public class HitRecord
    {
        public int HitCount;
        public int HitCD;
    }

    public class MediaL
    {
        public BodyL Owner = null;
        
        public int InstanceId = -1;
        public readonly MediaCfg Cfg;

        public readonly MediaMono MediaMono;

        public int AtkTough;
        private AtkDirType _atkDirType;
        private int _lifeTime = 0;

        private readonly Dictionary<int, HitRecord> _hitRecords = new();
        private readonly List<HitRecord> _hitRecordsList = new();
        public int TotalHitCount = 0;

        private int _movIdx = 0;

        private readonly List<BodyL> _triggerBodyLsThisTick = new();

        private readonly SortByDistance _sortByDistance;

        public MediaL(MediaMono mediaMono, MediaCfg mediaCfg)
        {
            MediaMono = mediaMono;
            mediaMono.MediaL = this;
            Cfg = mediaCfg;
            var speedMediaCollider = MediaMono.GetComponent<SpeedMediaCollider>();
            if (speedMediaCollider != null)
            {
                speedMediaCollider.SetL(this);
            }

            _sortByDistance = new SortByDistance(this);
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
            TotalHitCount = Cfg.HitMaxCountTotal > 0 ? Cfg.HitMaxCountTotal : int.MaxValue;
            AtkTough = Cfg.CustemToughPower == 0 ? Owner.GetNowTough() : Cfg.CustemToughPower;
            _hitRecords.Clear();
            _hitRecordsList.Clear();
            _triggerBodyLsThisTick.Clear();
        }

        private void Reset()
        {
            _lifeTime = 0;
            TotalHitCount = 0;
            Owner = null;
            InstanceId = -1;
            _movIdx = 0;
        }

        public static MediaL CreateBlank(int cfgId)
        {
            var mediaCfg = GameConfigs.Instance.Tables.TbMediaCfg.GetById(cfgId);
            if (mediaCfg == null)
            {
                throw new Exception("MediaCfg not found");
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

            SpeedColliderCheck();

            StillColliderCheck();

            var alive = CheckLifeTime();
            if (alive)
            {
                if (Cfg.ColliderShapeType == ColliderShape.SphereCast)
                {
                    DoMovement();
                }
            }
            else
            {
                Release();
            }

            MediaMono.UpATick();
            _lifeTime -= BattleLogicMgr.upTickDeltaTimeMs;
            foreach (var hitRecord in _hitRecordsList)
            {
                if (hitRecord.HitCD > 0)
                {
                    hitRecord.HitCD -= BattleLogicMgr.upTickDeltaTimeMs;
                }
            }
        }

        private void StillColliderCheck()
        {
            if (TotalHitCount <= 0) return;
            if (_triggerBodyLsThisTick.Count == 0) return;
            var min = Math.Min(TotalHitCount, _triggerBodyLsThisTick.Count);
            _triggerBodyLsThisTick.Sort(0, min, _sortByDistance);
            for (int i = 0; i < min; i++)
            {
                _triggerBodyLsThisTick[i].OnMediaHit(this);
            }

            _triggerBodyLsThisTick.Clear();
        }

        private class SortByDistance : IComparer<BodyL>
        {
            public SortByDistance(MediaL mediaL)
            {
                _mediaL = mediaL;
            }

            private readonly MediaL _mediaL;

            public int Compare(BodyL x, BodyL y)
            {
                return
                    x.BodyLMono.transform.Distance(_mediaL.MediaMono.transform)
                        .CompareTo(y.BodyLMono.transform.Distance(_mediaL.MediaMono.transform));
            }
        }

        private void Release()
        {
            Owner.TempMediaList.Remove(this);
            BattleLogicMgr.Instance.MediaPool.Free(Cfg.Id, this);
        }

        private bool CheckLifeTime()
        {
            var b = _lifeTime > 0;
            var b1 = TotalHitCount > 0;
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

        private void SpeedColliderCheck()
        {
            var highSpeedMedia = MediaMono.GetComponent<SpeedMediaCollider>();
            if (highSpeedMedia != null)
            {
                highSpeedMedia.UpATick();
            }
        }


      


        public bool CanLEffect(BodyL bodyL)
        {
            if (TotalHitCount <= 0)
            {
                return false;
            }

            var bodyLInstanceId = bodyL.InstanceId;
            var bodyLTeam = bodyL.Team;

            var b1 = bodyLInstanceId == Owner.InstanceId;

            var bb = RelationShipCheck(bodyLTeam, b1);
            if (!bb)
                return false;

            var cfgTargetStatusFilter = (int)Cfg.TargetStatusFilter
                                        & (int)bodyL.BodyStatus;
            if (cfgTargetStatusFilter == 0)
                return false;

            if (!Cfg.TargetBuffFilter.IsNullOrEmpty() && !bodyL.ExistsBuff(Cfg.TargetBuffFilter))
                return false;

            if (_hitRecords.TryGetValue(bodyLInstanceId, out var hitRecord))
            {
                var canLEffect = hitRecord.HitCD <= 0 && hitRecord.HitCount > 0;
                return canLEffect;
            }

            _hitRecords[bodyLInstanceId] = new HitRecord
            {
                HitCount = Cfg.HitMaxCountEach,
                HitCD = 0
            };
            _hitRecordsList.Add(_hitRecords[bodyLInstanceId]);
            return true;
        }

        public void DoEffect(BodyL bodyL, bool dodged)
        {
            var hitRecord = _hitRecords[bodyL.InstanceId];
            if (!dodged)
            {
                hitRecord.HitCount--;
            }

            hitRecord.HitCD += Cfg.HitGapTime;
        }

        private bool RelationShipCheck(int bodyLTeam, bool b1)
        {
            var b2 = bodyLTeam == Owner.Team;
            var a = b1 ? (int)(TargetTypeTag.Self) : 0;
            var b = b2 && !b1 ? (int)(TargetTypeTag.OtherAlly) : 0;
            var c = !b2 ? (int)(TargetTypeTag.Enemy) : 0;
            var t = a + b + c;

            var cfgTargetType = (int)Cfg.TargetType & t;
            var bb = cfgTargetType != 0;
            return bb;
        }

        public void AddTickHitBody(BodyL bodyL)
        {
            _triggerBodyLsThisTick.Add(bodyL);
        }

        public int GatherAtk()
        {
            return Owner.GatherAtk();
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