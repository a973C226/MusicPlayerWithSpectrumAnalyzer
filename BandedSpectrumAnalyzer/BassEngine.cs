using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Un4seen.Bass;

namespace BandedSpectrumAnalyzer
{
    public class BassEngine : INotifyPropertyChanged, ISpectrumPlayer
    {
        #region Fields
        private static BassEngine instance;
        private DispatcherTimer positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private int streamHandle;
        private TagLib.File fileTag;
        private bool canPlay;
        private bool canPause;
        private bool isPlaying;
        private bool canStop;
        private long channelLength = 0;
        private long currentChannelPosition = 0;
        private int maxFFT = (int)(BASSData.BASS_DATA_AVAILABLE | BASSData.BASS_DATA_FFT4096);
        private int sampleFrequency = 44100;
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructor
        private BassEngine()
        {
            Initialize();
        }
        #endregion

        #region Singleton Instance
        public static BassEngine Instance
        {
            get
            {
                if (instance == null)
                    instance = new BassEngine();
                return instance;
            }
        }
        #endregion

        #region Public Methods
        public int GetFFTFrequencyIndex(int frequency)
        {
            return Utils.FFTFrequency2Index(frequency, 4096, sampleFrequency);
        }

        public int GetFFTData(float[] fftDataBuffer)
        {
            return Bass.BASS_ChannelGetData(ActiveStreamHandle, fftDataBuffer, maxFFT);
        }

        public void SetChannelPosition(long position)
        {
            position = Math.Max(0, Math.Min(position, ChannelLength));
            ChannelPosition = position;
            Bass.BASS_ChannelSetPosition(ActiveStreamHandle, ChannelPosition);
        }

        public void Stop()
        {
            ChannelPosition = 0;
            if (ActiveStreamHandle != 0)
            {
                Bass.BASS_ChannelStop(ActiveStreamHandle);
                Bass.BASS_ChannelSetPosition(ActiveStreamHandle, ChannelPosition);
            }
            this.IsPlaying = false;
            this.CanStop = false;
            this.CanPlay = true;
            this.CanPause = false;
        }

        public void Pause()
        {
            if (IsPlaying && CanPause)
            {
                Bass.BASS_ChannelPause(ActiveStreamHandle);
                this.IsPlaying = false;
                this.CanPlay = true;
                this.CanPause = false;
            }
        }

        public void Play()
        {
            if (CanPlay)
            {
                PlayCurrentStream();
                this.IsPlaying = true;
                this.CanPause = true;
                this.CanPlay = false;
                this.CanStop = true;
            }
        }

        public bool OpenFile(string path)
        {
            Stop();

            if (ActiveStreamHandle != 0)
                Bass.BASS_StreamFree(ActiveStreamHandle);

            if (System.IO.File.Exists(path))
            {
                // Create Stream
                int newStreamHandle = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);
                if (newStreamHandle != 0)
                {
                    ActiveStreamHandle = newStreamHandle;
                    BASS_CHANNELINFO info = new BASS_CHANNELINFO();
                    Bass.BASS_ChannelGetInfo(ActiveStreamHandle, info);
                    sampleFrequency = info.freq;
                    this.FileTag = TagLib.File.Create(path);
                    this.ChannelLength = Bass.BASS_ChannelGetLength(ActiveStreamHandle, 0);
                    this.CanPlay = true;
                    return true;
                }
                else
                {
                    this.ActiveStreamHandle = 0;
                    this.FileTag = null;
                    this.CanPlay = false;
                }
            }
            return false;
        }

        public long GetTrackLength(string path)
        {
            return Bass.BASS_ChannelGetLength(ActiveStreamHandle, 0);
        }

        public void SetVolume(float volume) 
        {
            Bass.BASS_SetVolume(volume);
        }

        public int GetTrackDuration(string filePath)
        {
            MediaPlayer player = new MediaPlayer();
            player.Open(new Uri(filePath));
            while (!player.NaturalDuration.HasTimeSpan)
            {

            }
            double trackLength = player.NaturalDuration.TimeSpan.TotalSeconds;
            return (int)trackLength;
        }
        #endregion

        #region Event Handleres
        private void positionTimer_Tick(object sender, EventArgs e)
        {
            if (ActiveStreamHandle == 0)
            {
                ChannelPosition = 0;
            }
            else
            {
                ChannelPosition = Bass.BASS_ChannelGetPosition(ActiveStreamHandle, 0);
                if (ChannelPosition >= ChannelLength)
                    TrackEnded();
            }
        }
        #endregion

        #region Private Utility Methods
        private void Initialize()
        {
            positionTimer.Interval = TimeSpan.FromMilliseconds(100);
            positionTimer.Tick += new EventHandler(positionTimer_Tick);

            IsPlaying = false;

            Window mainWindow = Application.Current.MainWindow;
            WindowInteropHelper interopHelper = new WindowInteropHelper(mainWindow);

            if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_SPEAKERS, interopHelper.Handle))
            {
                int pluginAAC = Bass.BASS_PluginLoad("bass_aac.dll");
#if DEBUG
                BASS_INFO info = new BASS_INFO();
                Bass.BASS_GetInfo(info);
                Debug.WriteLine(info.ToString());
                BASS_PLUGININFO pluginInfo = Bass.BASS_PluginGetInfo(pluginAAC);
                foreach (BASS_PLUGINFORM f in pluginInfo.formats)
                    Debug.WriteLine("Type={0}, Name={1}, Exts={2}", f.ctype, f.name, f.exts);
#endif
            }
            else
            {
                MessageBox.Show(mainWindow, "Bass initialization error!");
                mainWindow.Close();
            }
        }

        private void PlayCurrentStream()
        {
            // Play Stream
            if (ActiveStreamHandle != 0 && Bass.BASS_ChannelPlay(ActiveStreamHandle, false))
            {
                BASS_CHANNELINFO info = new BASS_CHANNELINFO();
                Bass.BASS_ChannelGetInfo(ActiveStreamHandle, info);
            }
            else
            {
#if DEBUG
                Debug.WriteLine("Error={0}", Bass.BASS_ErrorGetCode());
#endif
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private void TrackEnded()
        {
            Stop();
        }
        #endregion

        #region Public Properties
        public int ActiveStreamHandle
        {
            get { return streamHandle; }
            protected set
            {
                int oldValue = streamHandle;
                streamHandle = value;
                if (oldValue != streamHandle)
                    NotifyPropertyChanged("ActiveStreamHandle");
            }
        }

        public TagLib.File FileTag
        {
            get { return fileTag; }
            set
            {
                TagLib.File oldValue = fileTag;
                fileTag = value;
                if (oldValue != fileTag)
                    NotifyPropertyChanged("FileTag");
            }
        }

        public bool CanPlay
        {
            get { return canPlay; }
            protected set
            {
                bool oldValue = canPlay;
                canPlay = value;
                if (oldValue != canPlay)
                    NotifyPropertyChanged("CanPlay");
            }
        }

        public bool CanPause
        {
            get { return canPause; }
            protected set
            {
                bool oldValue = canPause;
                canPause = value;
                if (oldValue != canPause)
                    NotifyPropertyChanged("CanPause");
            }
        }

        public bool CanStop
        {
            get { return canStop; }
            protected set
            {
                bool oldValue = canStop;
                canStop = value;
                if (oldValue != canStop)
                    NotifyPropertyChanged("CanStop");
            }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            protected set
            {
                bool oldValue = isPlaying;
                isPlaying = value;
                if (oldValue != isPlaying)
                    NotifyPropertyChanged("IsPlaying");
                positionTimer.IsEnabled = value;
            }
        }

        public long ChannelLength
        {
            get { return channelLength; }
            protected set
            {
                long oldValue = channelLength;
                channelLength = value;
                if (oldValue != channelLength)
                    NotifyPropertyChanged("ChannelLength");
            }
        }

        public long ChannelPosition
        {
            get { return currentChannelPosition; }
            protected set
            {
                long oldValue = currentChannelPosition;
                currentChannelPosition = value;
                if (oldValue != currentChannelPosition)
                    NotifyPropertyChanged("ChannelPosition");
            }
        }
        #endregion
    }
}
