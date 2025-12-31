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
            return stunBuff.SkillEffectConfig.Parameters[0] >= stunBuff.RemainingTime;
        }

        public static bool TryAllocAndOnAdd(InstanceBuffInfo buffInfo, out IEffectBuff stunBuff)
        {
            stunBuff = null;

            IEffectBuff effectBuff = BattleLogicMgr.Instance.BuffPool.Allocate(buffInfo.BuffAlias);
            var effectBuffRemainingTime = effectBuff.SkillEffectConfig.Parameters[0];
            effectBuff.RemainingTime = effectBuffRemainingTime;

            var tryAllocAndAdd = effectBuff.TryOnAdd(buffInfo);
            if (!tryAllocAndAdd)
            {
                BattleLogicMgr.Instance.BuffPool.Free(effectBuff.SkillEffectConfig.Alias, effectBuff);
            }

            stunBuff = effectBuff;
            return tryAllocAndAdd;
        }
    }
}