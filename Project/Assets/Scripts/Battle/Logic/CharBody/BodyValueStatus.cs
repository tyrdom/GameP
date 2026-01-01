using System;
using System.Collections.Generic;
using Battle.Logic.AllManager;
using Battle.Logic.Tools;
using cfg.battle;

namespace Battle.Logic
{
    public class BodyValueStatus
    {
        public class BarUnit
        {
            private int _maxValue;
            private int _nowValue;
            private int _regDelay;
            private int _nowRegDelay;
            private int _regPerSec;
            private int _restValue;

            public BarUnit(int maxValue, int regPerSec, int regDelay)
            {
                _maxValue = maxValue;
                _nowValue = maxValue;
                _regPerSec = regPerSec;
                _regDelay = regDelay;
                _nowRegDelay = 0;
                _restValue = 0;
            }

            public bool Cost(int cost)
            {
                if (_nowValue < cost) return false;
                _nowValue -= cost;
                _nowRegDelay = _regDelay;
                return true;
            }

            public int NowValue
            {
                get => _nowValue;
            }


            public void Tick()
            {
                if (_regPerSec <= 0)
                {
                    return;
                }

                if (_nowRegDelay > 0)
                {
                    _nowRegDelay = -BattleLogicMgr.upTickDeltaTimeMs;
                    return;
                }

                if (_nowValue >= _maxValue) return;
                var upTickDeltaTimeMs = _regPerSec * BattleLogicMgr.upTickDeltaTimeMs;
                _nowValue += upTickDeltaTimeMs / 1000;
                _restValue += upTickDeltaTimeMs % 1000;
                while (_restValue > 1000)
                {
                    _nowValue++;
                    _restValue -= 1000;
                }

                if (_nowValue > _maxValue)
                {
                    _nowValue = _maxValue;
                }
            }

            public void Reset()
            {
                _nowValue = _maxValue;
                _nowRegDelay = 0;
                _restValue = 0;
            }
        }


        private enum BarValueType
        {
            Hp,
            Gp,
            Sp
        }


        private readonly BodyL _bodyL;
        private readonly DicForEach<BarValueType, BarUnit> _barUnitDic = new();


        public BodyValueStatus(BodyL bodyL)
        {
            _bodyL = bodyL;
            var bodyLCfg = bodyL.Cfg;
            _barUnitDic.AddOrUpdate(BarValueType.Hp, new BarUnit(bodyLCfg.MaxHp, 0, 0));
            _barUnitDic.AddOrUpdate(BarValueType.Gp,
                new BarUnit(bodyLCfg.MaxGp, bodyLCfg.GpRegDelayTime, bodyLCfg.GpRegPerSec));
            _barUnitDic.AddOrUpdate(BarValueType.Sp,
                new BarUnit(bodyLCfg.MaxSp, bodyLCfg.SpRegDelayTime, bodyLCfg.SpRegPerSec));
        }


        public void UpATick()
        {
            _barUnitDic.ForEach(barUnit => barUnit.Tick());
        }

        public bool CanCostAndCost(SkillActCfg byAlias)
        {
            if (!_barUnitDic.TryGetValue(BarValueType.Sp, out var barUnit)) return false;
            if (barUnit.NowValue < byAlias.SpCost) return false;
            barUnit.Cost(byAlias.SpCost);
            return true;
        }

        public void Reset()
        {
            _barUnitDic.ForEach(barUnit => barUnit.Reset());
        }

        public void ReBorn()
        {
        }

        public int GatherAtk()
        {
            throw new NotImplementedException();
        }
    }
}