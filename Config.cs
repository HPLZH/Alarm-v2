using System.Text.Json;

namespace Alarm_v2
{
    public class Config
    {
        static JsonSerializerOptions serializerOptions = new()
        {
            IncludeFields = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        public required string playlist;
        public required Guid device;

        public int volume = -1;
        public string? pb;
        public bool pl = false;
        public string[] pfx = [];
        public string? log;

        public FileInfo PlayList() => new(playlist);
        public FileInfo? PBFile() => pb is null ? null : new(pb);
        public FileInfo? LogFile() => log is null ? null : new(log);

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
    }
}
