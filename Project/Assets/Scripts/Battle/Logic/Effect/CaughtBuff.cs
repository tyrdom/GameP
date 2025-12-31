using System.Collections.Generic;
using Battle.Logic.AllManager;
using cfg;
using cfg.battle;
using UnityEngine;

namespace Battle.Logic.Effect
{
    public class CaughtBuff : IStunBuff
    {
        public CaughtBuff(SkillEffectCfg skillEffectConfig)
        {
            const int gap = 4;
            const int unitLength = 4;
            SkillEffectConfig = skillEffectConfig;
            _grabPoints = new List<GrabPoint>();
            var parameters = skillEffectConfig.Parameters;
            var parametersCount = (parameters.Count - gap) / unitLength;
            for (var i = 0; i < parametersCount; i++)
            {
                var time = parameters[gap + i * unitLength];
                var lastTime = i == 0 ? 0 : parameters[gap + (i - 1) * unitLength];
                var pTime = time - lastTime;
                var z = parameters[gap + i * unitLength + 1];
                var x = parameters[gap + i * unitLength + 2];
                var y = parameters[gap + i * unitLength + 3];
                _grabPoints.Add(new GrabPoint(pTime, z, x, y));
            }

            _triggerSkillTime = parameters[1];
            _triggerSkillId = parameters[2];
            _actStatus = (ActStatus)parameters[3];
        }

        private readonly List<GrabPoint> _grabPoints;

        private readonly int _triggerSkillTime;
        private readonly int _triggerSkillId;
        private readonly ActStatus _actStatus;
        public BodyL BuffOnBodyL { get; set; }

        private BodyL _whoCaught;

        public SkillEffectCfg SkillEffectConfig { get; }
        public int RemainingTime { get; set; }

        private int _nowPeriodicTime;

        private int _nowGrabTime;

        private int _nowPtIdx;

        public bool TryOnAdd(InstanceBuffInfo buffInfo)
        {
            RemainingTime = buffInfo.FixedTime;
            BuffOnBodyL = buffInfo.OnBodyL;
            _whoCaught = buffInfo.FromMedia.Owner;
            BuffOnBodyL.SetRigidBodyActive(false);
            _nowPeriodicTime = 0;
            _nowGrabTime = 0;
            var transform = _whoCaught.BodyLMono.transform;
            var transformPosition = transform.position;
            var transformRotation = transform.rotation;
            var position = transformPosition + _grabPoints[0].Pt;
            var newTransformRotation = _grabPoints[0].Rotate * transformRotation;
            BuffOnBodyL.BodyLMono.ForceSetRotate(newTransformRotation);
            BuffOnBodyL.BodyLMono.ForceSetPos(transform.TransformPoint(position));
            _nowPtIdx = 1;

            return true;
        }

        public bool CanInputAct()
        {
            return this.CanStunInputAct();
        }

        public bool UpATickAndCheckFinish()
        {
            RemainingTime = -BattleLogicMgr.upTickDeltaTimeMs;

            if (RemainingTime <= 0)
            {
                BuffOnBodyL.SetRigidBodyActive(true);
                return true;
            }

            _nowGrabTime += BattleLogicMgr.upTickDeltaTimeMs;
            if (_triggerSkillId != 0 && _nowGrabTime >= _triggerSkillTime)
            {
                _whoCaught.ForceLaunchSkill(_triggerSkillId, _actStatus);
            }

            if (_nowPtIdx >= _grabPoints.Count) return false;
            var caughtTrans = _whoCaught.BodyLMono.transform;
            _nowPeriodicTime += BattleLogicMgr.upTickDeltaTimeMs;

            var nowGrab = _grabPoints[_nowPtIdx];
            if (_nowPeriodicTime < nowGrab.PeriodTime)
            {
                var nowToPt = _grabPoints[_nowPtIdx - 1].Pt;
                var lastPt = nowGrab.Pt;
                var pos = Vector3.Lerp(lastPt, nowToPt, Mathf.Clamp01((float)_nowPeriodicTime / nowGrab.PeriodTime));
                BuffOnBodyL.BodyLMono.ForceSetPos(caughtTrans.TransformPoint(pos));
            }
            else
            {
                _nowPeriodicTime -= nowGrab.PeriodTime;
                _nowPtIdx++;
                if (_nowPtIdx >= _grabPoints.Count) return false;
                var newGrab = _grabPoints[_nowPtIdx];
                BuffOnBodyL.BodyLMono.ForceSetPos(caughtTrans.TransformPoint(newGrab.Pt));
                BuffOnBodyL.BodyLMono.ForceSetRotate(newGrab.Rotate * caughtTrans.rotation);
            }

            return false;
        }
    }

    internal readonly struct GrabPoint

    {
        public readonly int PeriodTime;
        public readonly Vector3 Pt;
        public readonly Quaternion Rotate;

        public GrabPoint(int periodTime, float z, float x, int y)
        {
            PeriodTime = periodTime;
            Pt = new Vector3(x, 0, z);
            Rotate = Quaternion.Euler(0, y, 0);
        }
    }
}