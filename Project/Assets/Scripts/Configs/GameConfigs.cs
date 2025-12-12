using System.Collections.Generic;
using cfg;
using QFramework;
using UnityEngine;

namespace Configs
{
    public class GameConfigs : Singleton<GameConfigs>
    {
        private Tables _tables;

        public readonly Dictionary<string, SkillExtraCfg> SkillExtraCfgS = new();

        public Tables Tables
        {
            get
            {
                if (_tables == null)
                {
                    _tables = CfgReader.GetTables();
                    InitExtraCfgS();
                }

                return _tables;
            }
        }

        private void InitExtraCfgS()
        {
            InitSkillExtraCfgS();
        }

        private void InitSkillExtraCfgS()
        {
            foreach (var skillActCfg in Tables.TbSkillActCfg.DataList)
            {
                var id = skillActCfg.Id;
                var @alias = skillActCfg.Alias;
                var tz = 0;
                var tx = 0;
                var lastMoveIdx = -1;
                var lastMoveTime = 0;
                var firstLaunchTime = -1;
                for (var i = 0; i < skillActCfg.MoveChangeTimeS.Count - 1; i++)
                {
                    var moveChangeTime1 = skillActCfg.MoveChangeTimeS[i].ChangeTime;
                    var moveChangeTime2 = skillActCfg.MoveChangeTimeS[i + 1].ChangeTime;
                    var moveChangeTime = moveChangeTime2 - moveChangeTime1;
                    tx += moveChangeTime * skillActCfg.MoveChangeTimeS[i].XInt / 1000;
                    tz += moveChangeTime * skillActCfg.MoveChangeTimeS[i].ZInt / 1000;
                    if (i == skillActCfg.MoveChangeTimeS.Count - 2)
                    {
                        lastMoveTime = moveChangeTime;
                        lastMoveIdx = i;
                    }
                }

                if (skillActCfg.LaunchTimeToMediaAlias.Count > 0)
                {
                    var launchTimeToMediaAlia = skillActCfg.LaunchTimeToMediaAlias[0];
                    firstLaunchTime = launchTimeToMediaAlia.LaunchTime / Instance.Tables.TbCommonCfg.TickTime;
                }

                var skillExtraCfg = new SkillExtraCfg(id, @alias, tz, tx, lastMoveIdx, lastMoveTime, firstLaunchTime);
                SkillExtraCfgS[alias] = skillExtraCfg;
            }
        }
    }

    public struct SkillExtraCfg

    {
        public readonly int Id;
        public readonly string Alias;
        public readonly int TotalZMovement;
        public readonly int TotalXMovement;
        public readonly int LastMoveIdx;
        public readonly int LastMoveTime;
        public readonly int FirstLaunchBaseTough;

        public SkillExtraCfg(int id, string alias, int totalZMovement, int totalXMovement, int lastMoveIdx,
            int lastMoveTime,
            int firstLaunchBaseTough)
        {
            Id = id;
            Alias = alias;
            TotalZMovement = totalZMovement;
            TotalXMovement = totalXMovement;
            LastMoveIdx = lastMoveIdx;
            LastMoveTime = lastMoveTime;
            FirstLaunchBaseTough = firstLaunchBaseTough;
        }
    }
}