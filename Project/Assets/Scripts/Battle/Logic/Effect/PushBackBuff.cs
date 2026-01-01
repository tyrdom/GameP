using cfg.battle;

namespace Battle.Logic.Effect
{
    public class PushBackBuff : IStunBuff
    {
        public SkillEffectCfg SkillEffectConfig { get; }
        public int RemainingTime { get; set; }
        public void OnAdd(InstanceBuffInfo buffInfo)
        {
            throw new System.NotImplementedException();
        }

        public void OnRemove()
        {
            throw new System.NotImplementedException();
        }

        public BodyL BuffOnBodyL { get; set; }
        public PushBackBuff(SkillEffectCfg skillEffectConfig)
        {
            SkillEffectConfig = skillEffectConfig;
           
            
        }

        public void ResetState()
        {
            BuffOnBodyL = null;
            RemainingTime = 0;
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