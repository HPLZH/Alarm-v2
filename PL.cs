namespace Alarm_v2
{
    /// <summary>
    /// 防近似过滤模块 v2
    /// </summary>
    public class PL
    {
        readonly List<string> mlist = [];
        public readonly Tree tree = new();
        string[] last = [];
        public double ADCC { get; private set; } = 0;
        bool adccChanged = true;

        public string GetItem()
        {
            if (mlist.Count == 0) return string.Empty;
            string r;
            double x;
            string[] c;
            string[] p;
            int k;
            int dcc;
            do
            {
                r = mlist[Random.Shared.Next(mlist.Count)];
                x = Random.Shared.NextDouble();
                c = SplitPath(r);
                p = Parent(c, last);
                k = tree.GetNode(p)?.Count ?? 0;
                dcc = ((Tree?)tree.GetNode(c[..^1]))?.DirectChildrenCount() ?? 0;
            }
            while (k != 0 && x > (double)(k - 1) / mlist.Count / DCCPunishment(dcc));
            last = c;
            return r;
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
