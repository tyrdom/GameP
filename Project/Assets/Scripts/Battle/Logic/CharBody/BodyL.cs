using System;
using System.Collections.Generic;
using Battle.Logic;
using Battle.Logic.AllManager;
using Battle.Logic.Effect;
using Battle.Logic.Map;
using Battle.Logic.Media;
using Battle.Logic.Tools;
using cfg;
using cfg.battle;
using Configs;


public class BodyL
{
    public readonly BodyLMono BodyLMono;

    private readonly ActL _actL;

    public readonly BodyValueStatus BodyValueStatus;

    public int Team;

    public int InstanceId;

    public readonly BodyCfg Cfg;

    public int PauseTime = 0;

    public IStunBuff StunBuff = null;

    public readonly DicForEach<string, StatusBuff> StatusBuffDict = new();

    public readonly List<MediaL> TempMediaList = new();

    public BodyStatus BodyStatus;


    public static BodyL CreateBlank(int cfgId)
    {
        var dataById = GameConfigs.Instance.Tables.TbBodyCfg.GetById(cfgId);
        var bodyMono = BLogicMono.Instance.CreateBodyMonoInPool(dataById);
        var bodyL = new BodyL(bodyMono, dataById);

        return bodyL;
    }

    public static BodyL Alloc(InstanceCharInfo instanceCharInfo)
    {
        var bodyL = BattleLogicMgr.Instance.BodyPool.Allocate(instanceCharInfo.bodyCfgId);
        bodyL.InstanceData(instanceCharInfo);
        return bodyL;
    }

    public static void Release(BodyL obj)
    {
        obj.Reset();
        obj.BodyLMono.Release();
        BattleLogicMgr.Instance.InstanceIdToBodyDic.Remove(obj.InstanceId);
    }

    private void Reset()
    {
        Team = -1;
        InstanceId = -1;
        _actL.Reset();
        BodyValueStatus.Reset();
        StunBuff = null;
        StatusBuffDict.Clear();

        BodyLMono.ResetPassiveMove();
    }


    public int GetFixCastStunTime()
    {
        return _actL.GetFixCastStunTime();
    }

    public void SpawnToPoint(PointMono point)
    {
        var transform = point.transform;
        var transform1 = BodyLMono.transform;
        transform1.position = transform.position;
        transform1.rotation = transform.rotation;
        BodyLMono.gameObject.SetActive(true);
    }

    public BodyL(BodyLMono bodyLMono, BodyCfg dataById)
    {
        BodyLMono = bodyLMono;
        Cfg = dataById;
        BodyLMono.BodyL = this;
        InstanceId = -1;
        Team = -1;
        BodyValueStatus = new BodyValueStatus(this);
        _actL = new ActL(this);
    }

    private void InstanceData(InstanceCharInfo instanceCharInfo)
    {
        InstanceId = instanceCharInfo.instanceId;
        Team = instanceCharInfo.team;
        BodyStatus = BodyStatus.Active;
        BodyLMono.gameObject.name = Cfg.Alias + "_" + InstanceId;
    }

    public void UpATick()
    {
        BodyValueStatus.UpATick();
        StatusBuffUpATick();
        if (PauseTime > 0)
        {
            PauseTime -= BattleLogicMgr.upTickDeltaTimeMs;
            //todo AnimationPause
            return;
        }

        StunBuff?.UpATickAndCheckFinish();


        _actL.UpATick();
        BodyLMono.UpATick();
    }

    private void StatusBuffUpATick()
    {
        var statusBuffList = StatusBuffDict.GetList();
        for (var index = statusBuffList.Count - 1; index >= 0; index--)
        {
            var statusBuff = statusBuffList[index];
            var upATick = statusBuff.UpATickAndCheckFinish();
            if (!upATick) continue;
            statusBuff.Release();
        }
    }


    private void AddStatusBuff(SkillEffectCfg skillEffectCfg, MediaL mediaL, int fixTime)
    {
        var instanceBuffInfo = new InstanceBuffInfo(this, skillEffectCfg.Alias, mediaL, fixTime);
        EffectExtension.Alloc(instanceBuffInfo, out var statusBuff);
        StatusBuffDict.AddOrUpdate(statusBuff.SkillEffectConfig.Alias, statusBuff as StatusBuff);
    }

    private void AddStunBuff(SkillEffectCfg skillEffectCfg, MediaL mediaL, int fixTime)
    {
        var instanceBuffInfo = new InstanceBuffInfo(this, skillEffectCfg.Alias, mediaL, fixTime);
        EffectExtension.Alloc(instanceBuffInfo, out var effectBuff);
        _actL.OnStun();
        StunBuff = effectBuff as IStunBuff;
    }

    public bool CanInputAct()
    {
        if (StunBuff != null && !StunBuff.CanInputAct())
        {
            return false;
        }

        return _actL.CanInputAct();
    }

    public void OnMediaHit(MediaL mediaL)
    {
        if (!mediaL.CanLEffect(this))
        {
            return;
        }


        var result = _actL.JudgeBeAtk(mediaL);
        switch (result)
        {
            case AtkResult.None:
                break;
            case AtkResult.SuccessOnStun:

                break;
            case AtkResult.Draw:
                break;
            case AtkResult.FailBeDodged:

                break;
            case AtkResult.FailBeParried: //
                break;
            case AtkResult.SuccessOnNormal:
                _actL.BodyL.GetEffectFromMedia(mediaL);
                break;
            case AtkResult.SuccessOnBreak:
                break;
            case AtkResult.FailBeCountered:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void GetEffectFromMedia(MediaL mediaL)
    {
        foreach (var cfgEffectAlia in mediaL.Cfg.EffectAlias)
        {
            var effectCfg = GameConfigs.Instance.Tables.TbEffectCfg.GetByAlias(cfgEffectAlia);
            if (effectCfg == null)
            {
                throw new Exception($"MediaCfg is null check cfg{cfgEffectAlia}");
            }

            var b = CanEffectAddToBody(effectCfg, mediaL, out var isStunBuff);
            if (!b)
            {
                continue;
            }

            var effectBuff = EffectBuffExtensions.Alloc(cfgEffectAlia);
            if (isStunBuff)
            {
                AddStunBuff(effectCfg, mediaL, mediaL.Owner.GetFixCastStunTime());
            }
            else
            {
                AddStatusBuff(effectCfg, mediaL, effectCfg.LastTime);
            }
        }
    }

    private bool CanEffectAddToBody(SkillEffectCfg effectCfg, MediaL mediaL, out bool stunBuff)
    {
        stunBuff = false;
        var effectCfgBodyStatusFilter = (int)effectCfg.BodyStatusFilter & (int)BodyStatus;
        var b = effectCfgBodyStatusFilter == 0;
        if (b)
        {
            return false;
        }

        var b1 = IsStunBuff(effectCfg);
        stunBuff = b1;
        if (b1 && StunBuff is CaughtBuff caughtBuff)
        {
            if (caughtBuff.WhoCaught != mediaL.Owner)
                return false;
        }

        return true;
    }

    private static bool IsStunBuff(SkillEffectCfg effectCfg)
    {
        return effectCfg.EffectType is EffectType.Caught or EffectType.Pull or EffectType.Push;
    }

    public void DoSkillActMoveChange(MovementChangeCfg movementChangeCfg, int lastMoveTime)
    {
        BodyLMono.SetSkillVelocity(movementChangeCfg, lastMoveTime);
    }

    public void RemoveStatusBuff(StatusBuff statusBuff)
    {
        StatusBuffDict.Remove(statusBuff.SkillEffectConfig.Alias);
    }

    public int GetNowTough()
    {
        return _actL.CurActTough;
    }

    public bool ExistsBuff(string cfgTargetBuffFilter)
    {
        if (StunBuff != null && StunBuff.SkillEffectConfig.Alias == cfgTargetBuffFilter)
        {
            return true;
        }

        return StatusBuffDict.ContainsKey(cfgTargetBuffFilter);
    }

    public void SetRigidBodyActive(bool b)
    {
        throw new NotImplementedException();
    }

    public void ForceLaunchSkill(int triggerSkillId, ActStatus actStatus)
    {
        _actL.ForceLaunchSkill(triggerSkillId, actStatus);
    }

    public void TakeDmg(MediaL buffInfoFromMedia)
    {
        int atk = buffInfoFromMedia.GatherAtk();
    }

    public int GatherAtk()
    {
return BodyValueStatus.GatherAtk();
 }
}


public struct InstanceCharInfo
{
    public InstanceCharInfo(int bodyCfgId, int weaponId, int instanceId, int team)
    {
        this.bodyCfgId
            = bodyCfgId;
        this.instanceId = instanceId;
        this.team = team;
        this.weaponId = weaponId;
    }

    public int bodyCfgId;
    public int instanceId;
    public int team;
    public int weaponId;
}