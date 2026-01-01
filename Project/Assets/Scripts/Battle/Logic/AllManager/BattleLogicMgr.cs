using System;
using System.Collections.Generic;
using Battle.Logic.Effect;
using Battle.Logic.Map;
using Battle.Logic.Media;
using Battle.Logic.Tools;
using Configs;
using Input;
using QFramework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battle.Logic.AllManager
{
    public enum GOType
    {
        Player,
        Enemy,
        Media,
        Item
    }

    public static class BattleLogicConfig
    {
        public const int PlayerTeam = 1;
        public const int EnemyTeam = 2;
    }

    public class BattleLogicMgr : Singleton<BattleLogicMgr>
    {
        public static int upTickDeltaTimeMs;

        List<MapActor> _mapActors = new();

        private static readonly Dictionary<GOType, (int start, int end)> IdRanges = new()
        {
            { GOType.Player, (1000, 1999) }, // 玩家段: 1000-1999
            { GOType.Enemy, (20000, 29999) }, // 敌人段: 20000-29999  
            { GOType.Item, (40000, 49999) }, // 物品段: 40000-49999
            { GOType.Media, (50000, 59999) }, // 媒体段: 50000-59999
        };

        private readonly Dictionary<GOType, int> CurrentIds = new();

        public bool Paused = false;

        public readonly DicPool<int, BodyL> BodyPool = new();
        public readonly DicPool<int, MediaL> MediaPool = new();

        public readonly DicPool<string, IEffectBuff> BuffPool = new();


        public DicForEach<int, BodyL> InstanceIdToBodyDic = new();

        public readonly List<MediaL> MediaList = new();

        public readonly Dictionary<int, Operate> InstanceIdToOperateDic = new();

        private int _localPlayerId = -1;

        private BodyL _localPlayerBody = null;

        private void SegmentedInstanceId()
        {
            // 初始化每个类型的当前ID
            foreach (var type in Enum.GetValues(typeof(GOType)))
            {
                CurrentIds[(GOType)type] = IdRanges[(GOType)type].start;
            }
        }

        public int GenerateId(GOType type)
        {
            int nextId = CurrentIds[type];
            var range = IdRanges[type];

            // 检查ID段是否用完
            if (nextId > range.end)
            {
                Debug.LogError($"{type} ID段已用完! 范围: {range.start}-{range.end} ,从起始开始");
                nextId = range.start;
            }

            CurrentIds[type] = nextId + 1;
            return nextId;
        }

        public void Init()
        {
            Physics.simulationMode = SimulationMode.Script;
            SegmentedInstanceId();
            BodyPool.InitFunc(BodyL.CreateBlank, BodyL.Release);
            MediaPool.InitFunc(MediaL.CreateBlank, MediaL.Release);
            BuffPool.InitFunc(EffectBuffExtensions.CreateBlank, EffectBuffExtensions.Release);
            Paused = true;
        }

        public void AllocCreep(CreepInfo creepInfo)
        {
            var creepInstanceId = GenerateId(GOType.Enemy);
            var instanceInfo =
                new InstanceCreepInfo(creepInfo.CreepCfgId, creepInstanceId, BattleLogicConfig.EnemyTeam);
        }

        public void AllocLocalPlayer(GameStartInfo gameStartInfo)
        {
            var playerId = GenerateId(GOType.Player);
            _localPlayerId = playerId;
            InstanceIdToOperateDic[playerId] = new Operate { Move = Vector2.zero, OpAction = OpAction.None };
            InputMgr.Actions.Player.Move.performed += OnLocalActMove;
            InputMgr.Actions.Player.Attack.performed += _ => OnLocalAct(OpAction.OpAct1);
            InputMgr.Actions.Player.Act2.performed += _ => OnLocalAct(OpAction.OpAct2);

            var localPlayerCfgAlias = gameStartInfo.localPlayerCfgAlias;
            var byAlias = GameConfigs.Instance.Tables.TbBodyCfg.GetByAlias(localPlayerCfgAlias);
            if (byAlias == null)
            {
                Debug.LogError($"找不到本地玩家配置: {localPlayerCfgAlias}");
                return;
            }

            var weaponTypeCfg = GameConfigs.Instance.Tables.TbWeaponTypeCfg.GetByAlias(gameStartInfo.weaponsCfgAlias);
            if (weaponTypeCfg == null)
            {
                Debug.LogError($"找不到武器配置: {gameStartInfo.weaponsCfgAlias}");
                return;
            }


            var instanceInfo =
                new InstanceCharInfo(byAlias.Id, weaponTypeCfg.Id, playerId, BattleLogicConfig.PlayerTeam);
            var bodyL = BodyL.Alloc(instanceInfo);
            _localPlayerBody = bodyL;
            InstanceIdToBodyDic.AddOrUpdate(playerId, bodyL);
        }

        private void OnLocalAct(OpAction opAct)
        {
            if (_localPlayerBody == null) return;
            var canInputAct = _localPlayerBody.CanInputAct();


            if (InstanceIdToOperateDic.TryGetValue(_localPlayerId, out var operate))
            {
                operate.OpAction = canInputAct ? opAct : OpAction.None;
            }
        }

        private void OnLocalActMove(InputAction.CallbackContext obj)
        {
            var move = obj.ReadValue<Vector2>();
            if (InstanceIdToOperateDic.TryGetValue(_localPlayerId, out var operate))
            {
                operate.Move = move;
            }
        }

        public void LoadLevel(int levelId)
        {
        }

        public Operate UseOperate(int instanceId)
        {
            if (!InstanceIdToOperateDic.TryGetValue(instanceId, out var operate)) return Operate.None;
            InstanceIdToOperateDic[instanceId] = Operate.None;
            return operate;
        }

        public void GoATick()
        {
            if (Paused)
            {
                return;
            }

            foreach (var bodyL in InstanceIdToBodyDic.GetList())
            {
                bodyL.UpATick();
            }

            var updateDeltaTimeMs = BLogicMono.UpTickDeltaTimeSec;
            Physics.Simulate(updateDeltaTimeMs);

            foreach (var media in MediaList)
            {
                media.UpATick();
            }
        }
    }

    public class InstanceCreepInfo
    {
        public InstanceCreepInfo(int creepInfoCreepCfgId, int creepInstanceId, int enemyTeam)
        {
            throw new NotImplementedException();
        }
    }

    public class CreepInfo
    {
        public int CreepCfgId;
        public string AgentCfgAlias;
    }

    public struct GameStartInfo
    {
        public string levelAlias;
        public string localPlayerCfgAlias;
        public string weaponsCfgAlias;
    }

    public record Operate
    {
        public Vector2 Move;
        public OpAction OpAction;
        public static Operate None { get; } = new() { Move = Vector2.zero, OpAction = OpAction.None };
    }

    public enum OpAction
    {
        None,
        OpAct1,
        OpAct2,
        Dash
    }
}