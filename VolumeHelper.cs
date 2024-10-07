using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Alarm_v2
{
    public class VolumeHelper
    {
        public static readonly VolumeHelper Shared = new();

        readonly Dictionary<string, float> volState = [];
        readonly Dictionary<string, bool> muteState = [];

        readonly MMDeviceEnumerator enumerator = new();

        MMDeviceCollection SpeakDevices => enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        public bool SaveVolume(MMDevice device, bool force = false)
        {
            if (force)
            {
                volState[device.ID] = device.AudioEndpointVolume.MasterVolumeLevelScalar;
                return true;
            }
            else
            {
                return volState.TryAdd(device.ID, device.AudioEndpointVolume.MasterVolumeLevelScalar);
            }
        }

        public static void SetVolume(MMDevice device, float volume)
        {
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
        }

        public void RestoreVolume(MMDevice device)
        {
            if (volState.TryGetValue(device.ID, out var volume))
            {
                SetVolume(device, volume);
                volState.Remove(device.ID);
            }
        }

        public bool SaveMute(MMDevice device, bool force = false)
        {
            if (force)
            {
                muteState[device.ID] = device.AudioEndpointVolume.Mute;
                return true;
            }
            else
            {
                return muteState.TryAdd(device.ID, device.AudioEndpointVolume.Mute);
            }
            
        }

        public static void SetMute(MMDevice device, bool mute)
        {
            device.AudioEndpointVolume.Mute = false;
        }

        public void RestoreMute(MMDevice device)
        {
            if (muteState.TryGetValue(device.ID, out var mute))
            {
                SetMute(device, mute);
                muteState.Remove(device.ID);
            }
        }

        public void Foreach(Action<MMDevice> action, string search = "")
        {
            foreach(MMDevice device in SpeakDevices)
            {
                if (search == "" || device.ID.Contains(search))
                {
                    action(device);
                }
            }
        }

        public Action<MMDevice> SaveAndSetVolume(float volume)
        {
            return device =>
            {
                SaveVolume(device);
                SetVolume(device, volume);
            };
        }

        public Action<MMDevice> SaveAndSetMute(bool mute)
        {
            return device =>
            {
                SaveMute(device);
                SetMute(device, mute);
            };
        }

        public Action<MMDevice> SaveAndSet(float volume, bool mute)
        {
            return device =>
            {
                SaveVolume(device);
                SaveMute(device);
                SetVolume(device, volume);
                SetMute(device, mute);
            };
        }

        public void Restore(MMDevice device)
        {
            RestoreVolume(device);
            RestoreMute(device);
        }

        public static void ListDevice(Action<string> writeLine)
        {
            var devices = DirectSoundOut.Devices;
            foreach (var device in devices)
            {
                writeLine("ModuleName  : " + device.ModuleName);
                writeLine("Description : " + device.Description);
                writeLine("GUID        : " + device.Guid);
                writeLine("");
            }
        }
    }
}
