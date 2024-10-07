namespace Alarm_v2
{
    internal static class AppConfig
    {
        public static event Action<int> OnExit = (_) => { };

        public static void Exit(int exitCode)
        {
            OnExit.Invoke(exitCode);
            Environment.Exit(exitCode);
        }

        
    }
}
