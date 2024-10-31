namespace Alarm_v2
{
    public class Controller(Player player, Func<string> next, Mapping mapping)
    {
        public Func<string> Next { get; set; } = next;
        public Player Player { get; init; } = player;

        /// <summary>
        /// 参数：文件路径，映射名
        /// </summary>
        public event Action<string, string> OnSec = (_, _) => { };
        public event Action<string, string> OnNext = (_, _) => { };
        public event Action<string, string> BeforeNext = (_, _) => { };
        public event Action OnStop = () => { };

        bool stopCur = false;
        bool stopLoop = false;

        public Task Start()
        {
            return Task.Run(async () =>
            {
                while (!stopLoop)
                {
                    string fp = Next();
                    string rfp = mapping.ResolveR(fp);
                    OnNext.Invoke(rfp, fp);
                    Player.Play(rfp);
                    while (Player.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                    {
                        await Task.Delay(1000);
                        OnSec.Invoke(rfp, fp);
                        if (stopCur || stopLoop)
                        {
                            break;
                        }
                    }
                    Player.Stop();
                    stopCur = false;
                    BeforeNext.Invoke(rfp, fp);
                    await Task.Delay(1000);
                }
                OnStop.Invoke();
            });
        }

        public void Stop()
        {
            stopLoop = true;
        }

        public void Pass()
        {
            if (Player.PlaybackPosition > TimeSpan.FromSeconds(5))
                stopCur = true;
        }
    }
}
