using System;
using System.IO;
using System.Threading;
using WzComparerR2.WzLib;
using NAudio.Wave;

namespace WzComparerR2.MapRender
{
    class Music : IDisposable
    {
        public Music(Wz_Sound sound)
        {
            this.soundData = sound.ExtractSound();
            mp3Reader = new Mp3FileReader(new MemoryStream(soundData));
            waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
            waveOut.Init(mp3Reader);
            waveOut.PlaybackStopped += OnPlaybackStopped;

            Music.GlobalVolumeChanged += this.OnGlobalVolumeChanged;
        }

        private void OnPlaybackStopped(object src, StoppedEventArgs e)
        {
            mp3Reader.Position = 0;
            waveOut.Play();
        }

        private WaveOut waveOut;
        private byte[] soundData;
        private Mp3FileReader mp3Reader;

        public bool IsLoop { get; set; }

        public PlayState State
        {
            get
            {
                var state = waveOut.PlaybackState;
                switch (state)
                {
                    case PlaybackState.Stopped: return PlayState.Stopped;
                    case PlaybackState.Playing: return PlayState.Playing;
                    case PlaybackState.Paused: return PlayState.Paused;
                    default: return PlayState.Unknown;
                }
            }
        }

        public float Volume
        {
            get
            {
                return waveOut.Volume;
            }
            set { waveOut.Volume = value * globalVol; }
        }

        public void Play()
        {
            waveOut.Play();
        }

        public void Pause()
        {
            waveOut.Pause();
        }

        public void Stop()
        {
            waveOut.Stop();
        }

        public void Dispose()
        {
            Music.GlobalVolumeChanged -= this.OnGlobalVolumeChanged;
            waveOut.PlaybackStopped -= OnPlaybackStopped;
            waveOut.Dispose();
            mp3Reader.Dispose();
        }

        public enum PlayState
        {
            Stopped = 0,
            Playing = 1,
            Paused = 2,
            Unknown = -1,
        }

        private void OnGlobalVolumeChanged(object sender, EventArgs e)
        {
            this.Volume = Volume;
        }

        #region Global Volume
        private static float globalVol = 1f;
        private static event EventHandler GlobalVolumeChanged;
        public static float GlobalVolume
        {
            get { return globalVol; }
            set
            {
                globalVol = value;
                GlobalVolumeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        #endregion
    }
}
