using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alarm_v2
{
    public class Player(DirectSoundOut output, bool memstream = false)
    {
        readonly DirectSoundOut output = output;
        AudioFileReader? reader;
        IWaveProvider? wave;

        readonly bool memstream = memstream;

        public PlaybackState PlaybackState => output.PlaybackState;
        public TimeSpan PlaybackPosition => output.PlaybackPosition;

        public void Play(string file)
        {
            reader = new AudioFileReader(file);
            if (memstream)
            {
                wave = new MemoryWaveStream(reader);
                reader.Dispose();
                reader = null;
            }
            else
            {
                wave = reader;
            }
            output.Init(wave);
            output.Play();
        }

        public void Stop()
        {
            output.Stop();
            reader?.Dispose();
            reader = null;
            if(wave is IDisposable disposable)
            {
                disposable.Dispose();
            }
            wave = null;
            output.Dispose();
        }
    }

    public class MemoryWaveStream : IWaveProvider, IDisposable
    {
        public WaveFormat WaveFormat { get; private init; }

        public int Read(byte[] buffer, int offset, int count)
            => memstream?.Read(buffer, offset, count) ?? 0;

        MemoryStream? memstream = new();

        public MemoryWaveStream(AudioFileReader reader)
        {
            WaveFormat = reader.WaveFormat;
            reader.CopyTo(memstream);
            memstream.Seek(0, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            memstream?.Dispose();
            memstream = null;
            GC.SuppressFinalize(this);
        }
    }
}
