using Battle.Logic.Media;
using cfg.battle;

namespace Battle.Logic.Effect
{
    public class StatusBuff : IEffectBuff
    {
        public SkillEffectCfg SkillEffectConfig { get; }
        public int RemainingTime { get; set; }

        public bool TryOnAdd(InstanceBuffInfo buffInfo)
        {
            throw new System.NotImplementedException();
        }

        public BodyL BuffOnBodyL { get; set; }

        public StatusBuff(SkillEffectCfg skillEffectConfig)
        {
            SkillEffectConfig = skillEffectConfig;
        }


        public bool UpATickAndCheckFinish()
        {
            return false;
        }

        public static StatusBuff Alloc(InstanceBuffInfo instanceBuffInfo)
        {
            throw new System.NotImplementedException();
        }
    }

    public struct InstanceBuffInfo
    {
        public readonly string BuffAlias;
        public readonly BodyL OnBodyL;

        public readonly MediaL FromMedia;

        public readonly int FixedTime;

        public InstanceBuffInfo(BodyL onBodyL, string buffAlias, MediaL fromMedia, int fixedTime)
        {
            OnBodyL = onBodyL;
            BuffAlias = buffAlias;
            FromMedia = fromMedia;
            FixedTime = fixedTime;
        }
    }
}