using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alarm_v2
{
    public class Player(DirectSoundOut output)
    {
        readonly DirectSoundOut output = output;
        AudioFileReader? reader;

        public PlaybackState PlaybackState => output.PlaybackState;
        public TimeSpan PlaybackPosition => output.PlaybackPosition;

        public void Play(string file)
        {
            reader = new AudioFileReader(file);
            output.Init(reader);
            output.Play();
        }

        public void Stop()
        {
            output.Stop();
            reader?.Dispose();
            output.Dispose();
        }
    }
}
