using static System.Net.Mime.MediaTypeNames;

namespace Alarm_v2
{
    public static class M3u8
    {
        public static string[] ReadList(string path)
        {
            string[] text = File.ReadAllLines(path, System.Text.Encoding.UTF8);
            return RemoveComments(text);
        }

        public static string[] GetListFromString(string str)
        {
            string[] list = str.Split('\n');
            return RemoveComments(list);
        }

        public static string[] RemoveComments(string[] list)
        {
            List<string> result = [];
            foreach (string line in list)
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
