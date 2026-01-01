using Battle.Logic.AllManager;

namespace Battle.Logic.Effect
{
    public interface IStunBuff : IEffectBuff
    {
        bool CanInputAct();
    }

    public static class EffectExtension
    {
        public static bool CanStunInputAct(this IStunBuff stunBuff)
        {
            return stunBuff.SkillEffectConfig.Parameters[0] >= stunBuff.RemainingTime;
        }

        public static void Alloc(InstanceBuffInfo buffInfo, out IEffectBuff stunBuff)
        {
            IEffectBuff effectBuff = BattleLogicMgr.Instance.BuffPool.Allocate(buffInfo.BuffAlias);
            var effectBuffRemainingTime = effectBuff.SkillEffectConfig.Parameters[0];
            effectBuff.RemainingTime = effectBuffRemainingTime;
            effectBuff.OnAdd(buffInfo);
            stunBuff = effectBuff;
        }
    }
}