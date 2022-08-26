﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using FlyleafLib;
using FlyleafLib.Controls;
using FlyleafLib.Controls.WPF;
using FlyleafLib.MediaPlayer;

namespace TestStramApp
{
    class ViewModel
    {
        #region Airspace / Windows / Controls / Events
        /// <summary>
        /// The Content Control which hosts WindowsFormsHost (useful for airspace issues &amp; change to fullscreen mode)
        /// </summary>
        public VideoView VideoView => Player.VideoView;

        /// <summary>
        /// The foreground window which will be created from VideoView and hosts our content (useful for airspace issues)
        /// </summary>
        public Window WindowFront => VideoView.WindowFront;

        /// <summary>
        /// The background window which hosts the VideoView (useful for airspace issues)
        /// </summary>
        public Window WindowBack => VideoView.WindowBack;

        ///// <summary>
        ///// WindowsFormsHost child control (to catch events and resolve airspace issues)
        ///// </summary>
        public Flyleaf WinFormsControl => Player.Control;
        #endregion

        #region ViewModel's Properties
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        private string _UserInput;
        public string UserInput
        {
            get => _UserInput;
            set { _UserInput = value; OnPropertyChanged(nameof(UserInput)); }
        }
        #endregion

        #region Using Library's Model & Basic ViewModel
        /// <summary>
        /// Player's functions (open new input or existing stream/play/pause/seek forewards or backwards etc.) + Plugins[Plugin_Name].[Media]Streams + Status [see Player.cs for details]
        /// </summary>
        public Player Player { get; set; }

        /// <summary>
        /// Global audio configuration (common Device, Session &amp; Master Volume/Mute) [see AudioMaster.cs]
        /// </summary>
        public AudioMaster AudioMaster => Master.AudioMaster;

        /// <summary>
        /// Player's Configuration ([media].[config_attribute]) [see Config.cs for details]
        /// </summary>
        public Config Config => Player?.Config;
        public Config.AudioConfig Audio => Config.Audio;    // Enabled, DelayTicks, Languages (by priority)
        public Config.SubtitlesConfig Subs => Config.Subtitles;// Enabled, DelayTicks, Languages (by priority), UseOnlineDatabases
        public Config.VideoConfig Video => Config.Video;    // AspectRatio, ClearColor, VSync
        public Config.DecoderConfig Decoder => Config.Decoder;  // HWAcceleration, VideoThreads + Buffering Configuration
        public Config.DemuxerConfig Demuxer => Config.Demuxer;  // [Media]FormatOpt + Buffering Configuration
        #endregion

        #region Initialize
        public static string SampleVideo { get; set; } = Utils.FileExistsBelow("Sample.mp4");

        /// <summary>
        /// ViewMode's Constructor
        /// </summary>
        public ViewModel()
        {
            // Registers FFmpeg Libraries
            Master.RegisterFFmpeg(":2");

            // Prepares Player's Configuration
            Config config = new Config();
            config.Demuxer.FormatOpt.Add("probesize", (50 * (long)1024 * 1024).ToString());
            config.Demuxer.FormatOpt.Add("analyzeduration", (10 * (long)1000 * 1000).ToString());
            config.Decoder.MaxVideoFrames = 1;
            config.Demuxer.FormatOpt.Add("rw_timeout", "5000000");
            config.Demuxer.FormatOpt.Add("tcp_nodelay", "1");
            config.Demuxer.FormatOpt.Add("refcounted_frames", "1");
            config.Demuxer.FormatOpt.Add("max_delay", "1");
            config.Player.Usage = Usage.LowLatencyVideo;

            // Initializes the Player
            Player = new Player(config);
            Player.OpenCompleted += Player_OpenCompleted;

            OpenVideo = new RelayCommand(OpenVideoAction);
            PauseVideo = new RelayCommand(PauseVideoAction);
            PlayVideo = new RelayCommand(PlayVideoAction);
            UserInput = SampleVideo;
        }
        #endregion

        #region Basic Open/Play/Pause Commands
        public ICommand OpenVideo { get; set; }
        public ICommand PauseVideo { get; set; }
        public ICommand PlayVideo { get; set; }
        public void OpenVideoAction(object param) { if (string.IsNullOrEmpty(UserInput)) UserInput = SampleVideo; Player.Open("udp://169.254.0.114:40005"); }
        public void PauseVideoAction(object param) { Player.Pause(); }
        public void PlayVideoAction(object param) { Player.Play(); }
        #endregion

        #region Events
        private void Player_OpenCompleted(object sender, Player.OpenCompletedArgs e)
        {
            if (e.Success && e.Type == MediaType.Video)
                Player.Play();

            // Raise null is required for Player/Session/Config properties without property change updates (Normally, this should be called only once at the end of every OpenCompleted -mainly for video-)
            OnPropertyChanged(null);
        }
        #endregion
    }
}