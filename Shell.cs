using System.Diagnostics;

namespace Alarm_v2
{
    public class Shell
    {
        public required string file;
        public string[] args = ["$0"];
        public const string COMMAND_PLACEHOLDER = "$0";

        public virtual bool Execute(string command, out string result, string? input = null)
        {
            result = string.Empty;
            ProcessStartInfo psi = new()
            {
                FileName = file,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };
            foreach (string arg in args)
            {
                if (arg == COMMAND_PLACEHOLDER)
                {
                    psi.ArgumentList.Add(command);
                }
                else
                {
                    psi.ArgumentList.Add(arg);
                }
            }
            try
            {
                var proc = Process.Start(psi);
                if (proc != null)
                {
                    if(input != null)
                    {
                        proc.StandardInput.WriteLine(input);
                    }
                    proc.StandardInput.Close();
                    proc.WaitForExit();
                    result = proc.StandardOutput.ReadToEnd();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public bool Execute(ShellCommand command, out string result)
        {
            return Execute(command.command, out result, command.input);
        }

        public static bool TestTrue(string str)
        {
            str = str.Trim();
            if (bool.TryParse(str, out bool br)) return br;
            else if (int.TryParse(str, out int n)) return n == 0;
            else return false;
        }
    }

    public class NullShell : Shell
    {
        public readonly static NullShell Shared = new() { file = string.Empty };

        public override bool Execute(string command, out string result, string? input = null)
        {
            result = string.Empty;
            return false;
        }
    }

    public struct ShellCommand
    {
        public required string command;
        public string? input = null;

        public ShellCommand() { }
    }
}
