﻿using System.Text.Json;
using System.Text;

namespace Alarm_v2
{
    /// <summary>
    /// 防重复平衡模块 v2
    /// </summary>
    public class PB
    {
        readonly Dictionary<string, int> timeCount = [];
        readonly Dictionary<string, int> pCount = [];

        readonly Func<string> provider;
        readonly string fp;

        string currentPath = string.Empty;
        string currentMapped = string.Empty;
        int currentTime = 0;

        public PB(string path, Func<string> provider)
        {
            fp = path;
            this.provider = provider;
            string json;
            try
            {
                json = File.ReadAllText(path, Encoding.UTF8);
            }
            catch (Exception)
            {
                json = string.Empty;
            }
            if (string.IsNullOrEmpty(json))
            {
                timeCount = [];
                return;
            }
            try
            {
                Dictionary<string, int>? des = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                if (des != null) { timeCount = des; }
            }
            catch (Exception)
            {
                timeCount = [];
            }
        }

        public static string NameOf(string path) =>
            path.ToLower().StartsWith("mapped://") ?
            path :
            Path.GetFileNameWithoutExtension(path);

        const double p0 = 0.8;
        const double dt = 10;
        const int ct = 10;

        public static double P(int x)
        {
            return Math.Pow(p0, Math.Ceiling(x / (double)ct));
        }

        public IEnumerable<KeyValuePair<string, int>> GetData()
        {
            return timeCount.OrderByDescending(x => x.Value);

        }

        public string GetItem()
        {
            string r;
            bool h;
            IncHandler();
            do
            {
                r = provider.Invoke();
                h = true;
                int t = timeCount.GetValueOrDefault(NameOf(r), 0);
                for (int i = 0; i < t; i += ct)
                {
                    double p = Random.Shared.NextDouble();
                    if (p > p0)
                    {
                        if (t < dt * (p - p0))
                        {
                            t = 0;
                        }
                        else
                        {
                            t -= (int)Math.Floor(dt * (p - p0));
                        }
                        timeCount[NameOf(r)] = t;
                        h = false;
                        break;
                    }
                }
            } while (!h);

            return r;
        }

        public async void IncAsync(string path)
        {
            await Task.Run(() => Inc(path));
        }

        public void Inc(string path, bool noTimePassed = false)
        {
            const double p0 = 0.006;
            const int x0 = 73;
            const double dp = 0.06;
            timeCount[NameOf(path)] = timeCount.GetValueOrDefault(NameOf(path), 0) + 1;
            if(!noTimePassed)
            {
                foreach (var k in timeCount.Keys)
                {
                    pCount[k] = pCount.GetValueOrDefault(NameOf(k), 0) + 1;
                    if (Random.Shared.NextDouble() < p0 + (pCount[k] > x0 ? pCount[k] - x0 : 0) * dp)
                    {
                        if (timeCount[k] > 0)
                        {
                            timeCount[k]--;
                        }
                        else
                        {
                            timeCount.Remove(k);
                        }
                        pCount[k] = 0;
                    }
                }
            }
        }

        public void Inc_2(string path, string mapped)
        {
            if (currentPath == path && currentMapped == mapped)
            {
                currentTime++;
            }
            else if(currentPath == string.Empty)
            {
                currentPath = path;
                currentMapped = mapped;
                currentTime = 1;
            }
            else
            {
                IncHandler();
                Inc_2(path, mapped);
            }
        }

        public void IncHandler()
        {
            for (; currentTime > 0; currentTime--)
            {
                Inc(currentPath);
                if(NameOf(currentPath) != NameOf(currentMapped))
                {
                    Inc(currentMapped, true);
                }
            }
            currentPath = string.Empty;
            currentMapped = string.Empty;
        }

        public void Save()
        {
            IncHandler();
            File.WriteAllText(fp, JsonSerializer.Serialize(timeCount), Encoding.UTF8);
        }
    }
}
