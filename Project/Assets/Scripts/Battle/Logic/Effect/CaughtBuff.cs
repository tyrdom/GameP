using cfg.battle;

namespace Battle.Logic.Effect
{
    public class CaughtBuff : IStunBuff
    {
        public CaughtBuff(SkillEffectCfg skillEffectConfig)
        {
            SkillEffectConfig = skillEffectConfig;
        }

       

        public BodyL OnBodyL { get; set; }

       

        public SkillEffectCfg SkillEffectConfig { get; }
        public int RemainingTime { get; set; }
        public void OnInstantiate(InstanceBuffInfo buffInfo)
        {
            throw new System.NotImplementedException();
        }

        public bool CanInputAct()
        {
         return  this.CanStunInputAct();
        }
        public bool UpATickAndCheckFinish()
        {
            throw new System.NotImplementedException();
        }
    }
}