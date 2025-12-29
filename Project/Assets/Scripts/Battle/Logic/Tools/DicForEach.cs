using System.Collections.Generic;

namespace Battle.Logic.Tools
{
    public class DicForEach<TKt, TVT>
    {
        private readonly Dictionary<TKt, TVT> _dic;
        private readonly List<TVT> _list;

        public DicForEach()
        {
            _list = new List<TVT>();
            _dic = new Dictionary<TKt, TVT>();
        }

        public DicForEach(Dictionary<TKt, TVT> dic)
        {
            _dic = dic;
            _list = new List<TVT>();
            foreach (var item in dic)
            {
                _list.Add(item.Value);
            }
        }

        public void ForEach(System.Action<TVT> action)
        {
            foreach (var item in _list)
            {
                action(item);
            }
        }


        public void Remove(TKt key)
        {
            _dic.Remove(key);
            _list.Remove(_dic[key]);
        }

        public void AddOrUpdate(TKt key, TVT value)
        {
            if (_dic.TryGetValue(key, out var oldValue))
            {
                _list.Remove(oldValue);
            }

            _dic[key] = value;
            _list.Add(value);
        }


        public void Clear()
        {
            _dic.Clear();
            _list.Clear();
        }

        public bool TryGetValue(TKt key, out TVT value)
        {
            value = default;
            return _dic.TryGetValue(key, out value);
        }

        public List<TVT> GetList()
        {
            return _list;
        }

        public bool ContainsKey(TKt cfgTargetBuffFilter)
        {
            return _dic.ContainsKey(cfgTargetBuffFilter);
        }
    }
}