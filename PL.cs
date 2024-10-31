using System.Diagnostics.CodeAnalysis;

namespace Alarm_v2
{
    /// <summary>
    /// 防近似过滤模块 v2
    /// </summary>
    public class PL(Mapping mapping)
    {
        readonly List<string> mlist = [];
        string[]? mplist = null;
        readonly Mapping _map = mapping;
        public readonly Tree tree = new();
        string[] last = [];
        public double ADCC { get; private set; } = 0;
        bool adccChanged = true;

        public string GetItem()
        {
            if (mplist == null)
            {
                ReMap();
            }
            if (mplist.Length == 0) return string.Empty;
            string rt;
            double x;
            string[] c;
            string[] p;
            int k;
            int dcc;
            double dx;
            double dx0;
            do
            {
                k = 0;
                dx = 1;
                rt = mplist[Random.Shared.Next(mplist.Length)];
                _map.Resolve(rt, out string[] rl);
                foreach(var r in rl)
                {
                    c = SplitPath(r);
                    p = Parent(c, last);
                    k = tree.GetNode(p)?.Count ?? 0;
                    dcc = ((Tree?)tree.GetNode(c[..^1]))?.DirectChildrenCount() ?? 0;
                    dx0 = (double)(k - 1) / mplist.Length / DCCPunishment(dcc);
                    dx = Math.Min(dx0, dx);
                }
                x = Random.Shared.NextDouble();
            }
            while (k != 0 && x > dx);
            return rt;
        }

        public void Add(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                mlist.Add(path);
                tree.AddEndPath(SplitPath(path));
                adccChanged = true;
            }
        }

        public void SetLast(string path)
        {
            last = SplitPath(path);
        }

        [MemberNotNull(nameof(mplist))]
        public void ReMap()
        {
            mplist = _map.Map(mlist).ToArray();
        }

        public double DCCPunishment(int dcc)
        {
            double adcc = CalcuateADCC();
            return (4 * adcc + 3 * dcc) / (5 * adcc + 2 * dcc);
        }

        public static string[] SplitPath(string path)
        {
            return path.Trim().Replace("\\", "/").Split('/', StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] Parent(string[] path1, string[] path2)
        {
            int i = 0;
            for (i = 0; i < Math.Min(path1.Length, path2.Length); i++)
            {
                if (path1[i] != path2[i])
                    break;
            }
            return path1[..i];
        }

        public double CalcuateADCC()
        {
            if (adccChanged)
            {
                ADCC = tree.ADCC();
                adccChanged = false;
            }
            return ADCC;
        }
    }
}
