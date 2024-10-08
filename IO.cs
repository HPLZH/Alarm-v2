using System.Text;

namespace Alarm_v2
{
    public class IO
    {
        public static readonly IO Shared = new();

        public string HistoryPath { get; set; } = "";

        readonly StringWriter writer = new();

        public void WriteHistory(string text)
        {
            DateTime now = DateTime.Now;
            Console.WriteLine($"[{now:HH:mm:ss}] {text}");
            writer.WriteLine($"[{now:HH:mm:ss}] {text}");
            if (File.Exists(HistoryPath))
            {
                File.AppendAllText(HistoryPath, $"[{now:yyyy-MM-dd HH:mm:ss}] {text}\n");
            }
        }

        public void WriteText(string text)
        {
            Console.WriteLine(text);
            writer.WriteLine(text);
        }

        public string GetContent()
        {
            return writer.ToString();
        }

        public static void CWrite(string text)
        {
            (int l, int t) = Console.GetCursorPosition();
            Console.Write(text);
            Console.SetCursorPosition(l, t);
        }

        public static void CWrite(string text, int left, int top)
        {
            Console.Write(text);
            Console.SetCursorPosition(left, top);
        }
    }
}
