using cfg.battle;

namespace Battle.Logic.Effect
{
    public class PullBuff : IStunBuff
    {
        public BodyL BuffOnBodyL { get; set; }

        public SkillEffectCfg SkillEffectConfig { get; }
        public int RemainingTime { get; set; }

        public bool TryOnAdd(InstanceBuffInfo buffInfo)
        {
            throw new System.NotImplementedException();
        }

        public PullBuff(SkillEffectCfg skillEffectConfig)
        {
            SkillEffectConfig = skillEffectConfig;
        }

        public bool CanInputAct()
        {
            return this.CanStunInputAct();
        }


        public bool UpATickAndCheckFinish()
        {
            throw new System.NotImplementedException();
        }
    }
}