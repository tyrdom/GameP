using cfg.battle;

namespace Battle.Logic.Effect
{
    public class PushBackBuff : IStunBuff
    {
        public SkillEffectCfg SkillEffectConfig { get; }
        public int RemainingTime { get; set; }
        public void OnInstantiate(InstanceBuffInfo buffInfo)
        {
            throw new System.NotImplementedException();
        }

        public BodyL OnBodyL { get; set; }
        public PushBackBuff(SkillEffectCfg skillEffectConfig)
        {
            SkillEffectConfig = skillEffectConfig;
           
            
        }

        public void ResetState()
        {
            OnBodyL = null;
            RemainingTime = 0;
        }

        public bool CanInputAct()
        {
            return  this.CanStunInputAct();
        }

   

        public void UpATick()
        {
            throw new System.NotImplementedException();
        }

      
    }
}