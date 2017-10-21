using SPI_GPIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DinoSounds
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer timer;
        private MediaPlayer mediaplayer;
        private MediaPlayer silence;
        private int dinoSound = 0; // Will get incremented to 1 first time
        private const int lastSound = 11;
        private bool playing = false;
        public MainPage()
        {
            this.InitializeComponent();
            InitPlayer();
            InitIO();
            PlaySilence();
            PlaySound("init");
        }

        private async void PlaySilence()
        {
            silence = new MediaPlayer();
            silence.IsLoopingEnabled = true;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Sounds/Silence.wav"));
            var mediasource = MediaSource.CreateFromStorageFile(file);
            silence.Source = mediasource;
            silence.MediaEnded += Silence_MediaEnded;
            silence.Play();
        }

        private void Silence_MediaEnded(MediaPlayer sender, object args)
        {
            silence.Position = TimeSpan.Zero;
        }

        private void InitPlayer()
        {
            mediaplayer = new MediaPlayer();
            mediaplayer.AutoPlay = false;
            mediaplayer.MediaEnded += Mediaplayer_MediaEnded;
        }

        private async void PlaySound(string filename)
        {
            if (!playing)
            {
                playing = true;
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Sounds/" + filename + ".wav"));
                var mediasource = MediaSource.CreateFromStorageFile(file);
                mediaplayer.Source = mediasource;
                mediaplayer.Play();
            }
        }

        private void PlayNext()
        {
            if (!playing)
            {
                dinoSound++;
                if (dinoSound > lastSound)
                {
                    dinoSound = 1;
                }
                var file = "dino_" + dinoSound.ToString("D2");
                PlaySound(file);
            }
        }

        private void Mediaplayer_MediaEnded(MediaPlayer sender, object args)
        {
            playing = false;
        }

        private void initTimer()
        {
            // read timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200); //sample every 200mS
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void CheckInputs(UInt16 Inputs)
        {
            if (!playing)
            {
                var sw0 = (Inputs & 1 << PFDII.IN0) == 0;
                var sw1 = (Inputs & 1 << PFDII.IN1) == 0;
                var sw2 = (Inputs & 1 << PFDII.IN2) == 0;
                var sw3 = (Inputs & 1 << PFDII.IN3) == 0;
                var in4 = (Inputs & 1 << PFDII.IN4) == 0;
                var in5 = (Inputs & 1 << PFDII.IN5) == 0;
                var in6 = (Inputs & 1 << PFDII.IN6) == 0;
                var in7 = (Inputs & 1 << PFDII.IN7) == 0;
                if (sw0 || sw1 || sw2 || sw3 || in4 || in5 || in6 || in7)
                {
                    PlayNext();
                }
            }
        }

        // read GPIO and display it
        private void Timer_Tick(object sender, object e)
        {
            CheckInputs(MCP23S17.ReadRegister16());    // do something with the values
        }

        private async void InitIO()
        {
            try
            {
                await MCP23S17.InitSPI();

                MCP23S17.InitMCP23S17();
                MCP23S17.setPinMode(0x00FF); // 0x0000 = all outputs, 0xffff=all inputs, 0x00FF is PIFace Default
                MCP23S17.pullupMode(0x00FF); // 0x0000 = no pullups, 0xffff=all pullups, 0x00FF is PIFace Default
                MCP23S17.WriteWord(0x0000); // 0x0000 = no pullups, 0xffff=all pullups, 0x00FF is PIFace Default

                initTimer();
            }
            catch (Exception ex)
            {
                Diagnostic.Text = ex.Message;
            }
        }
    }
}
