using System;
using cfg;
using cfg.battle;
using Configs;

namespace Battle.Logic.Effect
{
    public interface IEffectBuff

    {
        BodyL BuffOnBodyL { get; set; }

        bool UpATickAndCheckFinish();
        SkillEffectCfg SkillEffectConfig { get; }

        int RemainingTime { get; set; }
        bool TryOnAdd(InstanceBuffInfo buffInfo);
    }

    public static class EffectBuffExtensions
    {
        public static IEffectBuff CreateBlank(string arg)
        {
            var skillEffectCfg = GameConfigs.Instance.Tables.TbEffectCfg.GetByAlias(arg);
            if (skillEffectCfg == null)
            {
                throw new Exception($"Skill effect config not found {arg}");
            }

            return skillEffectCfg.EffectType switch
            {
                EffectType.Push => new PushBackBuff(skillEffectCfg),
                EffectType.Pull => new PullBuff(skillEffectCfg),
                EffectType.Caught => new CaughtBuff(skillEffectCfg),
                _ => new StatusBuff(skillEffectCfg)
            };
        }

        public static void Release(IEffectBuff obj)
        {
            switch (obj)
            {
                case IStunBuff stunBuff:
                    if (obj.BuffOnBodyL.StunBuff == stunBuff)
                    {
                        obj.BuffOnBodyL.StunBuff = null;
                        obj.BuffOnBodyL.BodyLMono.ResetPassiveMove();
                        obj.BuffOnBodyL.BodyLMono.ResetSkillMove();
                    }

                    break;
                case StatusBuff statusBuff:
                    obj.BuffOnBodyL.RemoveStatusBuff(statusBuff);
                    break;
            }
        }
    }
}