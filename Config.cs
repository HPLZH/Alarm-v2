using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alarm_v2
{
    public class Config
    {
        internal static JsonSerializerOptions serializerOptions = new()
        {
            IncludeFields = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        };

        public required string playlist;
        public required Guid device;

        public int volume = -1;
        public string? pb;
        public bool pl = false;
        public string[] pfx = [];
        public string? log;

        public ExperimentalOptions? opts;

        public FileInfo PlayList() => new(playlist);
        public FileInfo? PBFile() => pb is null ? null : new(pb);
        public FileInfo? LogFile() => log is null ? null : new(log);

        public Shell? shell = null;
        public ExtraContent[] extra = [];

        public Dictionary<string, string> mapping = [];

        public Config() { }

        public static Config? Deserialize(FileInfo file)
        {
            var fs = file.OpenRead();
            return JsonSerializer.Deserialize<Config>(fs, serializerOptions);
        }
        
        public string Serialize()
        {
            return JsonSerializer.Serialize(this, serializerOptions);
        }

        public List<string> GetExtraContent()
        {
            List<string> content = [];
            foreach (var item in extra)
            {
                bool chr;
                foreach(var l in item.GetContent(shell ?? NullShell.Shared, out chr))
                {
                    content.Add(l);
                }
                if(chr && item.stopIfTrue)
                {
                    break;
                }
            }
            return content;
        }

        public class ExperimentalOptions
        {
            public bool memstream = false;
        }
    }
}
