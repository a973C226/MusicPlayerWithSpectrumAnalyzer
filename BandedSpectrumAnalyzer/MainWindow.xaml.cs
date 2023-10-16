using BandedSpectrumAnalyzer.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BandedSpectrumAnalyzer
{
    public partial class MainWindow : Window
    {
        #region Fields
        private bool inTimerPositionUpdate;
        private DispatcherTimer timer = new DispatcherTimer();
        private int trackDuration = 0;
        private long trackProgressStep = 0;
        private static bool playingNow = false;
        private string favoriteTracksDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"music\");
        public static List<string> favoriteTracksNames = new List<string>();
        public static List<string> favoriteTracksPaths = new List<string>();

        public string nowPlaylistPlay = "";

        public static string nowTrackName = "";
        public static string nowTrackPath = "";

        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            BassEngine bassEngine = BassEngine.Instance;

            UIHelper.Bind(bassEngine, "CanPlay", PlayButton, Button.IsEnabledProperty);
            UIHelper.Bind(bassEngine, "CanPause", PauseButton, Button.IsEnabledProperty);
            UIHelper.Bind(bassEngine, "ChannelLength", ProgressSlider, Slider.MaximumProperty);

            spectrumAnalyzer.RegisterSoundPlayer(bassEngine);
        }
        #endregion

        #region Public Methods
        public void UpdateProgressSlider()
        {
            timer.Stop();
            ProgressSlider.Value = 0;
            trackDuration = BassEngine.Instance.GetTrackDuration(nowTrackPath);
            maxTrackTime.Text = new TimeSpan(0, 0, (int)trackDuration).ToString().Substring(3);
            trackProgressStep = BassEngine.Instance.GetTrackLength(nowTrackPath) / trackDuration;
            timer.Start();
        }

        public void PlayTrack(string trackPath, string trackName)
        {
            BassEngine.Instance.Stop();
            nowTrackName = trackName;
            nowTrackPath = trackPath;
            FileText.Text = trackName;
            BassEngine.Instance.OpenFile(trackPath);
            BassEngine.Instance.Play();
            UpdateProgressSlider();
        }
        #endregion

        #region UI Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                timer.Tick += new EventHandler(timerTick);
                timer.Interval = new TimeSpan(0, 0, 1);

                int UCIndex = 0;
                DirectoryInfo favoriteTracksDir = new DirectoryInfo(favoriteTracksDirPath);
                FileInfo[] favoriteTracks = favoriteTracksDir.GetFiles("*.mp3");

                foreach (FileInfo file in favoriteTracks)
                {
                    favoriteTracksNames.Add(file.Name);
                    favoriteTracksPaths.Add(file.FullName);
                }

                foreach (SongItem playlistTrackUC in playlistTracksPanel.Children)
                {
                    playlistTrackUC.Title = favoriteTracksNames[UCIndex];
                    playlistTrackUC.Path = favoriteTracksPaths[UCIndex];
                    UCIndex++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The process failed: {0}", ex.ToString());
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (BassEngine.Instance.CanPlay)
            {
                BassEngine.Instance.Play();
                timer.Start();
                PlayButton.Visibility = Visibility.Collapsed;
                PauseButton.Visibility = Visibility.Visible;
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (BassEngine.Instance.CanPause)
            {
                BassEngine.Instance.Pause();
                timer.Stop();
                PlayButton.Visibility = Visibility.Visible;
                PauseButton.Visibility = Visibility.Collapsed;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            BassEngine.Instance.Stop();
            if (favoriteTracksNames.IndexOf(nowTrackName) + 1 < favoriteTracksNames.Count)
            {
                nowTrackName = favoriteTracksNames[favoriteTracksNames.IndexOf(nowTrackName) + 1];
                nowTrackPath = favoriteTracksPaths[favoriteTracksPaths.IndexOf(nowTrackPath) + 1];
            }
            else
            {
                nowTrackName = favoriteTracksNames[0];
                nowTrackPath = favoriteTracksPaths[0];
            }
            PlayTrack(nowTrackPath, nowTrackName);
        }        

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Filter = "(*.mp3)|*.mp3";
            if (openDialog.ShowDialog() == true)
            {
                PlayTrack(openDialog.FileName, openDialog.FileName);
            }
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!inTimerPositionUpdate && !playingNow)
            {
                BassEngine.Instance.SetChannelPosition((long)e.NewValue);
                nowTrackTime.Text = new TimeSpan(0, 0, (int)(ProgressSlider.Value / trackProgressStep)).ToString().Substring(3);
            }
        }

        private void timerTick(object sender, EventArgs e)
        {
            playingNow = true;
            TimeSpan t = TimeSpan.Parse("00:" + nowTrackTime.Text);
            nowTrackTime.Text = t.Add(new TimeSpan(0, 0, 1)).ToString().Substring(3);
            ProgressSlider.Value += trackProgressStep;
            playingNow = false;
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BassEngine.Instance.SetVolume((float)volumeSlider.Value);
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
