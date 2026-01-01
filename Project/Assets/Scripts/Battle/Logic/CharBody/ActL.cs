using System;
using System.Collections.Generic;
using Battle.Logic.AllManager;
using Battle.Logic.Media;
using cfg;
using cfg.battle;
using Configs;
using QFramework;
using UnityEngine;

public class ActL
{
    private List<BodyL> lockBreakTargetList = new();

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
        if (BodyL.BodyStatus == BodyStatus.Dead)
        {
            return;
        }

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
        var transform = bodyL.BodyLMono.transform;
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
            case OpAction.Dash:
                if (_currentWeaponType.DashDic.TryGetValue(_nowActStatus, out var dashCfg))
                {
                    LoadSkill(dashCfg);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void LoadSkill(SkillLaunchCfg skillActCfg)
    {
        var skillActAlias = skillActCfg.SkillActAlias;
        var nextComboStatus = skillActCfg.NextComboStatus;
        var byAlias = GameConfigs.Instance.Tables.TbSkillActCfg.GetByAlias(skillActAlias);
        if (byAlias == null)
        {
            throw new Exception($"SkillActCfg is null check cfg{skillActAlias}");
        }

        LoadSkill(byAlias, nextComboStatus);
    }

    private void LoadSkill(SkillActCfg byAlias, ActStatus nextComboStatus)
    {
        if (!BodyL.BodyValueStatus.CanCostAndCost(byAlias)) return;

        NowMovIdx = 0;
        NowMediaLaunchIdx = 0;
        _currentSkillCfg = byAlias;
        CurrentActTime = 0;

        _nowActStatus = nextComboStatus;
        _currentSkillExtraCfg = GameConfigs.Instance.SkillExtraCfgS[byAlias.Alias];

        var lockMediaAlias = _currentSkillCfg.LockMediaAlias;
        if (lockMediaAlias.IsNullOrEmpty())
        {
            return;
        }

        LaunchAMedia(lockMediaAlias, AtkDirType.Other, Vector3.zero, 0);
    }

    public void ForceLaunchSkill(int skillId, ActStatus actStatus)
    {
        var byAlias = GameConfigs.Instance.Tables.TbSkillActCfg.GetById(skillId);
        if (byAlias == null)
        {
            throw new Exception($"SkillActCfg is null check cfg{skillId}");
        }

        LoadSkill(byAlias, actStatus);
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
        var b = _currentSkillCfg == null || _currentSkillCfg.ComboInputCacheStartTime <= CurrentActTime ||
                CurrentActTime <= _currentSkillCfg.DodgeWindow;
        return b;
    }

    public void Reset()
    {
        _currentSkillCfg = null;
        CurrentActTime = 0;
        NowMovIdx = 0;
        _nowActStatus = ActStatus.IdleOrWalk;
        BodyL.BodyLMono.skillMoveVelocity = Vector3.zero;
    }

    public AtkResult JudgeBeAtk(MediaL mediaL)
    {
        var b1 = BodyL.BodyStatus == BodyStatus.Dead;
        if (b1)
        {
            return AtkResult.None;
        }

        var b2 = BodyL.BodyStatus == BodyStatus.Break;
        if (b2)
        {
            return AtkResult.SuccessOnBreak;
        }

        var b = BodyL.StunBuff != null;
        if (b)
        {
            return AtkResult.SuccessOnStun;
        }

        var maxTough = GameConfigs.Instance.Tables.TbCommonCfg.MaxTough;
        var minTough = GameConfigs.Instance.Tables.TbCommonCfg.MinTough;
        if (mediaL.AtkTough < minTough)
        {
            if (mediaL.Cfg.MediaType != MediaType.Range)
            {
                throw new Exception("Melee CanNot too low");
            }

            if (_currentSkillCfg == null) return AtkResult.FailBeCountered;
            if (CurrentActTime <= _currentSkillCfg.ParryWindow)
            {
                return AtkResult.FailBeCountered;
            }

            return CurrentActTime <= _currentSkillCfg.DodgeWindow ? AtkResult.FailBeDodged : AtkResult.FailBeCountered;
        }

        if (mediaL.AtkTough >= maxTough)
        {
            return AtkResult.SuccessOnNormal;
        }

        var midTough = GameConfigs.Instance.Tables.TbCommonCfg.MidTough;
        if (_currentSkillCfg == null)
        {
            return mediaL.AtkTough >= midTough ? AtkResult.FailBeParried : AtkResult.SuccessOnNormal;
        }

        if (mediaL.AtkTough > CurActTough || mediaL.AtkTough < CurActTough - midTough)
            return AtkResult.SuccessOnNormal;
        if (mediaL.AtkTough == CurActTough || mediaL.AtkTough == CurActTough - midTough)
        {
            return AtkResult.Draw;
        }

        if (CurrentActTime <= _currentSkillCfg.ParryWindow)
        {
            return AtkResult.FailBeCountered;
        }

        return CurrentActTime <= _currentSkillCfg.DodgeWindow
            ? AtkResult.FailBeDodged
            : AtkResult.FailBeCountered;
    }

    public void OnStun()
    {
        _currentSkillCfg = null;
        CurrentActTime = 0;
        NowMovIdx = 0;
        throw new NotImplementedException();
    }

    public int GetFixCastStunTime()
    {
        if
            (_currentSkillCfg == null) throw new Exception("_currentSkillCfg != null");

        return _currentSkillCfg.SkillMustTime - CurrentActTime;
    }
}


public enum AtkResult

{
    None,
    SuccessOnNormal,
    SuccessOnBreak,
    SuccessOnStun,
    Draw,
    FailBeDodged,
    FailBeParried,
    FailBeCountered,
}