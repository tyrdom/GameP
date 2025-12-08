using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace Battle.Logic.Tools
{
    public class DicPool<TK, TV>
    {
        private readonly Dictionary<TK, SimpleObjectPool<TV>> pool = new();

        private Func<TK, TV> _createFunc;
        private Action<TV> _onFreeFunc;

        public TV Allocate(TK id)
        {
            if (pool.TryGetValue(id, out var poolObject))
            {
                return poolObject.Allocate();
            }

            Debug.LogError($"could not find pool for id: {id}");
            var simpleObjectPool = new SimpleObjectPool<TV>(() => _createFunc(id), _onFreeFunc, 10);
            pool.Add(id, simpleObjectPool);
            return simpleObjectPool.Allocate();
        }

        public void Free(TK id, TV obj)
        {
            if (pool.TryGetValue(id, out var poolObject))
            {
                poolObject.Recycle(obj);
            }
        }

        public void InitFunc(Func<TK, TV> aCreateFunc, Action<TV> aOnFreeFunc)
        {
            _createFunc = aCreateFunc;
            _onFreeFunc = aOnFreeFunc;
        }

        public void InitAIdPool(TK id, int initNum)
        {
            InitIdPool(id, initNum);
        }

        public void Init(Func<TK, TV> aCreateFunc, Action<TV> aOnFreeFunc, List<TK> initIdList, int initNum)
        {
            _createFunc = aCreateFunc;
            _onFreeFunc = aOnFreeFunc;
            foreach (var id in initIdList)
            {
                InitIdPool(id, initNum);
            }
        }

        public void Init(Func<TK, TV> aCreateFunc, Action<TV> aOnFreeFunc, Dictionary<TK, int> initNumDict)
        {
            _createFunc = aCreateFunc;
            _onFreeFunc = aOnFreeFunc;
            foreach (var keyValuePair in initNumDict)
            {
                InitIdPool(keyValuePair.Key, keyValuePair.Value);
            }
        }


        private void InitIdPool(TK id, int initNum)
        {
            if (pool.TryGetValue(id, out var poolObject))
            {
                var o = _createFunc(id);
                for (int i = 0; i < initNum; i++)
                {
                    poolObject.Recycle(o);
                }
            }

            var simpleObjectPool = new SimpleObjectPool<TV>(() => _createFunc(id), _onFreeFunc, initNum);
            pool.Add(id, simpleObjectPool);
        }
    }
}