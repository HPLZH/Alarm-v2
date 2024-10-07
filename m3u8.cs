namespace Alarm_v2
{
    public static class M3u8
    {
        public static string[] ReadList(string path)
        {
            string[] text = File.ReadAllLines(path, System.Text.Encoding.UTF8);
            List<string> result = [];
            foreach (string line in text)
            {
                string s = line.Trim();
                if (s.Length > 0 && !s.StartsWith('#'))
                {
                    result.Add(s);
                }
            }
            return [.. result];
        }
    }
}
