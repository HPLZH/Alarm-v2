using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alarm_v2
{
    public class Mapping
    {
        readonly Dictionary<string, List<string>> mapd = [];
        readonly Dictionary<string, string> map = [];

        public Mapping(Dictionary<string,string> map)
        {
            this.map = map;
            Build();
        }

        public void Build()
        {
            mapd.Clear();
            foreach(var (k,v) in map)
            {
                if(mapd.TryGetValue(v, out var list))
                {
                    list.Add(k);
                }
                else
                {
                    mapd[v] = [k];
                }
            }
        }

        public bool Map(string key, out string mapped)
        {
            if(map.TryGetValue(key,out var v))
            {
                mapped = v;
                return true;
            }
            else
            {
                mapped = key;
                return false;
            }
        }

        public IEnumerable<string> Map(IEnumerable<string> list)
        {
            foreach(var k in list)
            {
                Map(k, out var v);
                yield return v;
            }
        }

        public bool Resolve(string mapped, out string[] li)
        {
            if(mapd.TryGetValue(mapped, out var list))
            {
                li = [.. list];
                return true;
            }
            else
            {
                li = [mapped];
                return false;
            }
        }

        public string ResolveR(string mapped)
        {
            if(Resolve(mapped, out var list))
            {
                return list[Random.Shared.Next(list.Length)];
            }
            else
            {
                return mapped;
            }
        }

    }
}
