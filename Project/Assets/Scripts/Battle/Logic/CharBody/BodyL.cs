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
            statusBuffList.RemoveAt(index);
            StatusBuffDict.Remove(statusBuff.SkillEffectConfig.Alias);
        }
    }

    public void AddEffectOrBuff(SkillEffectCfg skillEffectCfg, MediaL mediaL, int fixTime)
    {
        switch (skillEffectCfg.EffectType)
        {
            case EffectType.Push:
                if (StunBuff is not CaughtBuff)
                {
                    AddStunBuff(skillEffectCfg, mediaL, fixTime);
                }

                break;
            case EffectType.Pull:
                if (StunBuff is not CaughtBuff)
                {
                    AddStunBuff(skillEffectCfg, mediaL, fixTime);
                }

                break;
            case EffectType.Caught:
                AddStunBuff(skillEffectCfg, mediaL, fixTime);
                break;
            case EffectType.StandardDamage:
                MakeEffect(skillEffectCfg, mediaL);
                break;
            case EffectType.Lock:
                break;
            case EffectType.Undefine:
            default:
                AddStatusBuff(skillEffectCfg, mediaL, fixTime);
                break;
        }
    }

    private void MakeEffect(SkillEffectCfg skillEffectCfg, MediaL mediaL)
    {
        throw new NotImplementedException();
    }

    private void AddStatusBuff(SkillEffectCfg skillEffectCfg, MediaL mediaL, int fixTime)
    {
        var instanceBuffInfo = new InstanceBuffInfo(this, skillEffectCfg.Alias, mediaL, fixTime);
        var statusBuff = StatusBuff.Alloc(instanceBuffInfo);
        StatusBuffDict.AddOrUpdate(statusBuff.SkillEffectConfig.Alias, statusBuff);
    }

    private void AddStunBuff(SkillEffectCfg skillEffectCfg, MediaL mediaL, int fixTime)
    {
        var instanceBuffInfo = new InstanceBuffInfo(this, skillEffectCfg.Alias, mediaL, fixTime);
        var addOk = StunBuffExtension.TryAllocAndOnAdd(instanceBuffInfo, out var aStunBuff);
        if (!addOk)
        {
            return;
        }

        _actL.OnStun();
        StunBuff = aStunBuff as IStunBuff;
  
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


        if (mediaL.Cfg.MediaType is MediaType.Melee or MediaType.Range)
        {
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
                case AtkResult.FailBeParried://
                    break;
                case AtkResult.SuccessOnNormal:
                    break;
                case AtkResult.SuccessOnBreak:
                    break;
                case AtkResult.FailBeCountered:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (mediaL.Cfg.MediaType is MediaType.Lock)
        {
        }
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