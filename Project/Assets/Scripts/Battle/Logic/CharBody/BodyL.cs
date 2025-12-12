using System.Collections.Generic;
using Battle.Logic;
using Battle.Logic.AllManager;
using Battle.Logic.Effect;
using Battle.Logic.Map;
using Battle.Logic.Media;
using cfg;
using cfg.battle;
using Configs;
using UnityEngine;

public class BodyL
{
    public readonly BodyMono BodyMono;

    public readonly ActL ActL;

    public BodyValueStatus BodyValueStatus;

    public int Team;

    public int InstanceId;

    public readonly BodyCfg Cfg;


    public IStunBuff StunBuff = null;

    public Dictionary<string, StatusBuff> StatusBuffDict = new();
    public List<StatusBuff> StatusBuffList = new();

    public List<MediaL> TempMediaList = new();

    public bool Alive;


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
        obj.BodyMono.Release();
        BattleLogicMgr.Instance.BodyList.Remove(obj);
    }

    private void Reset()
    {
        Team = -1;
        InstanceId = -1;
        ActL.Reset();
        BodyValueStatus.Reset();
        StunBuff = null;
        StatusBuffDict.Clear();
        StatusBuffList.Clear();
        BodyMono.ResetPassiveMove();
    }

    public void SpawnToPoint(PointMono point)
    {
        var transform = point.transform;
        var transform1 = BodyMono.transform;
        transform1.position = transform.position;
        transform1.rotation = transform.rotation;
        BodyMono.gameObject.SetActive(true);
    }

    public BodyL(BodyMono bodyMono, BodyCfg dataById)
    {
        BodyMono = bodyMono;
        Cfg = dataById;
        BodyMono.BodyL = this;
        InstanceId = -1;
        Team = -1;
        BodyValueStatus = new BodyValueStatus(this);
        ActL = new ActL(this);
    }

    private void InstanceData(InstanceCharInfo instanceCharInfo)
    {
        InstanceId = instanceCharInfo.instanceId;
        Team = instanceCharInfo.team;
        Alive = true;
        BodyMono.gameObject.name = Cfg.Alias + "_" + InstanceId;
    }

    public void UpATick()
    {
        BodyValueStatus.UpATick();
        StunBuff?.UpATick();

        for (var index = StatusBuffList.Count - 1; index >= 0; index--)
        {
            var statusBuff = StatusBuffList[index];
            statusBuff.UpATick();
        }


        ActL.UpATick();
        BodyMono.UpATick();
    }

    public void AddEffectBuff(SkillEffectCfg skillEffectCfg, MediaL mediaL)
    {
        switch (skillEffectCfg.EffectType)
        {
            case EffectType.Push:
                if (StunBuff is not CaughtBuff)
                {
                    AddStunBuff(skillEffectCfg, mediaL);
                }

                break;
            case EffectType.Pull:
                if (StunBuff is not CaughtBuff)
                {
                    AddStunBuff(skillEffectCfg, mediaL);
                }

                break;
            case EffectType.Caught:
                AddStunBuff(skillEffectCfg, mediaL);
                break;
            default:
                AddStatusBuff(skillEffectCfg, mediaL);
                break;
        }
    }

    private void AddStatusBuff(SkillEffectCfg skillEffectCfg, MediaL mediaL)
    {
        var instanceBuffInfo = new InstanceBuffInfo(this, skillEffectCfg.Alias, mediaL);
        var statusBuff = StatusBuff.Alloc(instanceBuffInfo);
        StatusBuffDict.Add(statusBuff.SkillEffectConfig.Alias, statusBuff);
        StatusBuffList.Add(statusBuff);
    }

    private void AddStunBuff(SkillEffectCfg skillEffectCfg, MediaL mediaL)
    {
        var instanceBuffInfo = new InstanceBuffInfo(this, skillEffectCfg.Alias, mediaL);
        var stunBuff = StunBuffExtension.Alloc(instanceBuffInfo);
        StunBuff = stunBuff;
        ActL.Reset();
    }

    public bool CanInputAct()
    {
        if (StunBuff != null && !StunBuff.CanInputAct())
        {
            return false;
        }

        return ActL.CanInputAct();
    }

    public void OnMediaHit(MediaL mediaL)
    {
        if (!mediaL.CanLEffect())
        {
            return;
        }
        
        mediaL.OnEffect();
        
        
        
    }

    public void DoSkillActMoveChange(MovementChangeCfg movementChangeCfg, int lastMoveTime)
    {
        
        BodyMono.SetSkillVelocity(movementChangeCfg, lastMoveTime);
  
    }

    public void RemoveStatusBuff(StatusBuff statusBuff)
    {
        StatusBuffDict.Remove(statusBuff.SkillEffectConfig.Alias);
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