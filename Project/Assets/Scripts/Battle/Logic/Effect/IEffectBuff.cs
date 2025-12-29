using System;
using cfg;
using cfg.battle;
using Configs;

namespace Battle.Logic.Effect
{
    public interface IEffectBuff

    {
        BodyL OnBodyL { get; set; }

        bool UpATickAndCheckFinish();
        SkillEffectCfg SkillEffectConfig { get; }

        int RemainingTime { get; set; }
        void OnInstantiate(InstanceBuffInfo buffInfo);
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
                    if (obj.OnBodyL.StunBuff == stunBuff)
                    {
                        obj.OnBodyL.StunBuff = null;
                        obj.OnBodyL.BodyMono.ResetPassiveMove();
                        obj.OnBodyL.BodyMono.ResetSkillMove();
                    }

                    break;
                case StatusBuff statusBuff:
                    obj.OnBodyL.RemoveStatusBuff(statusBuff);
                    break;
            }
        }
    }
}