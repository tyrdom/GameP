using Battle.Logic.Media;
using cfg.battle;

namespace Battle.Logic.Effect
{
    public class StatusBuff : IEffectBuff
    {
        public SkillEffectCfg SkillEffectConfig { get; }
        public int RemainingTime { get; set; }

        public void OnInstantiate(InstanceBuffInfo buffInfo)
        {
            throw new System.NotImplementedException();
        }

        public BodyL OnBodyL { get; set; }

        public StatusBuff(SkillEffectCfg skillEffectConfig)
        {
            SkillEffectConfig = skillEffectConfig;
        }


        public void UpATick()
        {
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

        public InstanceBuffInfo(BodyL onBodyL, string buffAlias, MediaL fromMedia)
        {
            OnBodyL = onBodyL;

            BuffAlias = buffAlias;
            FromMedia = fromMedia;
        }
    }
}