using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alarm_v2
{
    public class ExtraContent
    {
        [JsonPropertyName("if")]
        public ShellCommand? cond = null;

        [JsonPropertyName("content")]
        public required List<JsonElement> content;

        [JsonPropertyName("break")]
        public bool stopIfTrue = false;

        public IEnumerable<string> GetContent(Shell shell, out bool check_result)
        {
            check_result = true;
            if(cond != null)
            {
                if (shell.Execute(cond.Value, out var rs0) &&
                    Shell.TestTrue(rs0)) { }
                else
                {
                    check_result= false;
                    return [];
                }
            }

            List<string> result = [];
            foreach(var item in content)
            {
                if(item.ValueKind == JsonValueKind.String)
                {
                    var s = item.GetString();
                    if (s != null) result.Add(s);
                }
                else
                {
                    try
                    {
                        var c = item.Deserialize<ShellCommand>(Config.serializerOptions);
                        if(shell.Execute(c, out var rs))
                        {
                            foreach(var ln in M3u8.GetListFromString(rs))
                            {
                                result.Add(ln);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                    
                }
            }
            return result;
        } 
    }
}
