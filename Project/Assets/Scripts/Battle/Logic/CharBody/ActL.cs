using System;
using Battle.Logic.AllManager;
using Battle.Logic.Media;
using cfg;
using cfg.battle;
using Configs;
using UnityEngine;

public class ActL
{
    private WeaponTypeCfg _currentWeaponType;

    private SkillActCfg _currentSkillCfg;

    private SkillExtraCfg _currentSkillExtraCfg;
    public readonly BodyL BodyL;

    private ActStatus _nowActStatus = ActStatus.IdleOrWalk;

    public int CurrentActTime;

    public int NowMovIdx;

    public int NowMediaLaunchIdx;

    public int CurActTough;

    public void UpATick()
    {
        if (CanLaunchSkill())
        {
            var operate = BattleLogicMgr.Instance.UseOperate(BodyL.InstanceId);
            if (operate != null)
            {
                LaunchSkill(operate);
            }
        }

        if (_currentSkillCfg != null && _currentSkillCfg.SkillMaxTime <= CurrentActTime)
        {
            UpNowSkill();
        }
    }

    private void UpNowSkill()
    {
        DoChangeMovement();

        var launchTimeToMediaAlia = _currentSkillCfg.LaunchTimeToMediaAlias[NowMediaLaunchIdx];

        if (launchTimeToMediaAlia.LaunchTime <= CurrentActTime)
        {
            NowMediaLaunchIdx++;
            DoSkillActMediaLaunch(launchTimeToMediaAlia);
        }


        if (CurrentActTime == 0)
        {
            CurActTough = _currentSkillCfg.BaseTough == 0
                ? _currentSkillExtraCfg.FirstLaunchBaseTough
                : _currentSkillCfg.BaseTough;
        }
        else
        {
            CurActTough += GameConfigs.Instance.Tables.TbCommonCfg.TickToughGrow;
        }


        CurrentActTime += BattleLogicMgr.upTickDeltaTimeMs;
    }

    private void DoChangeMovement()
    {
        if (NowMovIdx >= _currentSkillCfg.MoveChangeTimeS.Count)
        {
            return;
        }

        var movementChangeCfg = _currentSkillCfg.MoveChangeTimeS[NowMovIdx];
        if (movementChangeCfg.ChangeTime <= CurrentActTime)
        {
            var interval =
                NowMovIdx == _currentSkillCfg.MoveChangeTimeS.Count - 2
                    ? _currentSkillCfg.MoveChangeTimeS[^1].ChangeTime - movementChangeCfg.ChangeTime
                    : -1;

            BodyL.DoSkillActMoveChange(movementChangeCfg, interval);
            NowMovIdx++;
        }
    }

    private void DoSkillActMediaLaunch(LauncherCfg launchCfg)
    {
        var offSet = new Vector3(launchCfg.OffSetXInt / 1000f, 0, launchCfg.OffSetZInt / 1000f);
        var launchCfgRotateY = launchCfg.RotateY;
        var launchCfgAtkDir = launchCfg.AtkDir;
        foreach (var launchCfgMediaAlia in launchCfg.MediaAlias)
        {
            LaunchAMedia(launchCfgMediaAlia, launchCfgAtkDir, offSet, launchCfgRotateY);
        }
    }

    private void LaunchAMedia(string launchCfgMediaAlia, AtkDirType launchCfgAtkDir, Vector3 offSet,
        int launchCfgRotateY)
    {
        var mediaCfg = GameConfigs.Instance.Tables.TbMediaCfg.GetByAlias(launchCfgMediaAlia);
        if (mediaCfg == null)
        {
            throw new Exception($"MediaCfg is null check cfg{launchCfgMediaAlia}");
        }

        var mediaCfgId = mediaCfg.Id;

        var instanceMediaInfo = new InstanceMediaInfo
            (mediaCfgId, BattleLogicMgr.Instance.GenerateId(GOType.Media), launchCfgAtkDir);
        var ownerInfo = new OwnerInfo(BodyL);

        var mediaL = MediaL.Alloc(instanceMediaInfo, ownerInfo);

        LaunchMedia(mediaL, offSet, launchCfgRotateY, BodyL);
    }

    private static void LaunchMedia(MediaL mediaL, Vector3 offSet, int rotateY, BodyL bodyL)
    {
        var transform = bodyL.BodyMono.transform;
        var transform1 = mediaL.MediaMono.transform;
        transform1.position = transform.TransformPoint(offSet);
        transform1.rotation = transform.rotation;
        transform1.Translate(offSet);
        transform1.Rotate(0, rotateY, 0);
    }

    private void LaunchSkill(Operate operate)
    {
        switch (operate.OpAction)
        {
            case OpAction.None:
                if (_currentWeaponType.NoneDic.TryGetValue(_nowActStatus, out var noneCfg))
                {
                    LoadSkill(noneCfg);
                }

                break;
            case OpAction.OpAct1:
                if (_currentWeaponType.Op1Dic.TryGetValue(_nowActStatus, out var opAct1Cfg))
                {
                    LoadSkill(opAct1Cfg);
                }

                break;
            case OpAction.OpAct2:
                if (_currentWeaponType.Op2Dic.TryGetValue(_nowActStatus, out var opAct2Cfg))
                {
                    LoadSkill(opAct2Cfg);
                }

                break;
            case OpAction.OpAct3:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void LoadSkill(SkillLaunchCfg skillActCfg)
    {
        var byAlias = GameConfigs.Instance.Tables.TbSkillActCfg.GetByAlias(skillActCfg.SkillActAlias);
        NowMovIdx = 0;
        NowMediaLaunchIdx = 0;
        _currentSkillCfg = byAlias ??
                           throw new Exception($"SkillActCfg is null check cfg{skillActCfg.SkillActAlias}");
        CurrentActTime = 0;
        _nowActStatus = skillActCfg.NextComboStatus;
        _currentSkillExtraCfg = GameConfigs.Instance.SkillExtraCfgS[skillActCfg.SkillActAlias];

        var lockMediaAlias = _currentSkillCfg.LockMediaAlias;
        LaunchAMedia(lockMediaAlias, AtkDirType.Other, Vector3.zero, 0);
    }


    private bool CanLaunchSkill()
    {
        var bodyLActive = BodyL.StunBuff == null && BodyL.BodyStatus == BodyStatus.Active;
        var b = _currentSkillCfg == null || _currentSkillCfg.SkillMustTime <= CurrentActTime;
        return bodyLActive && b;
    }

    public ActL(BodyL bodyL)
    {
        BodyL = bodyL;
    }

    public void Init()
    {
    }

    public bool CanInputAct()
    {
        var b = _currentSkillCfg == null || _currentSkillCfg.ComboInputCacheStartTime <= CurrentActTime;
        return b;
    }

    public void Reset()
    {
        _currentSkillCfg = null;
        CurrentActTime = 0;
        NowMovIdx = 0;
        _nowActStatus = ActStatus.IdleOrWalk;
        BodyL.BodyMono.skillMoveVelocity = Vector3.zero;
    }

    public void JudgeAtk(MediaL mediaL)
    {
        var b1 = BodyL.BodyStatus == BodyStatus.Disable;
        if (b1)
        {
            return;
        }

        var b2 = BodyL.BodyStatus == BodyStatus.Dead;
        {
            //todo deadBody    
        }

        var b = BodyL.StunBuff != null;
        if (b)
        {
        }

        var maxTough = GameConfigs.Instance.Tables.TbCommonCfg.MaxTough;
        var minTough = GameConfigs.Instance.Tables.TbCommonCfg.MinTough;
        var b3 = mediaL.AtkTough < minTough;

        throw new NotImplementedException();
    }
}