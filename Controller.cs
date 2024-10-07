namespace Alarm_v2
{
    public class Controller(Player player, Func<string> next)
    {
        public Func<string> Next { get; set; } = next;
        public Player Player { get; init; } = player;

        public event Action<string> OnSec = (_) => { };
        public event Action<string> OnNext = (_) => { };
        public event Action<string> BeforeNext = (_) => { };
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
                    OnNext.Invoke(fp);
                    Player.Play(fp);
                    while (Player.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                    {
                        await Task.Delay(1000);
                        OnSec.Invoke(fp);
                        if (stopCur || stopLoop)
                        {
                            break;
                        }
                    }
                    Player.Stop();
                    stopCur = false;
                    BeforeNext.Invoke(fp);
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
