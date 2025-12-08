using Battle.Logic.AllManager;

namespace Battle.Logic.Effect
{
    public interface IStunBuff : IEffectBuff
    {
        bool CanInputAct();
    }

    public static class StunBuffExtension
    {
        public static bool CanStunInputAct(this IStunBuff stunBuff)
        {
            return stunBuff.SkillEffectConfig.Parameters[1] >= stunBuff.RemainingTime;
        }

        public static IStunBuff Alloc(InstanceBuffInfo buffInfo)
        {
            var effectBuff = BattleLogicMgr.Instance.BuffPool.Allocate(buffInfo.BuffAlias);
            
            
            var effectBuffRemainingTime = effectBuff.SkillEffectConfig.Parameters[0];
            
            effectBuff.RemainingTime = effectBuffRemainingTime;
            effectBuff.OnInstantiate(buffInfo);
            return effectBuff as IStunBuff;
        }
    }
}