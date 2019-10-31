using AllCaps.Recognition;
using Microsoft.VisualBasic;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ALLCAPS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.Closing += (snd, evt) =>
            {
                this.ClearSpeechRecognizer();
            };

            this.Loaded += (snd, evt) =>
            {
                this.ResetSpeechRecognizer();
            };
        }

        private ISpeechRecognizer recognizer;
        private string configString;

        public ISpeechRecognizer Recognizer
        {
            get => this.recognizer;
            private set
            {
                // unregister callbacks if any
                if (this.recognizer != null)
                {
                    //this.recognizer.SpeechRecognized -= this.OnRecognition;
                    this.recognizer.SpeechPredicted -= this.OnPrediction;
                }

                this.recognizer = value;

                if (this.recognizer != null)
                {
                    //this.recognizer.SpeechRecognized += this.OnRecognition;
                    this.recognizer.SpeechPredicted += this.OnPrediction;
                }
            }
        }

        private void OnPrediction(object sender, RecognitionEventArgs evt)
        {
            Debug.WriteLine($"[{evt.Result.Offset:hh\\:mm\\:ss}]: {evt.Result.Text}");
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.Speech.Text = evt.Result.Text;
                });
            }
            catch (TaskCanceledException)
            {
                // expected for now.
            }
        }

        private void LaunchSettings(object sender, RoutedEventArgs e)
        {
            this.configString = Interaction.InputBox("Please input \"key,region\" leave empty for local:", "Use Azure Speech Recognition");

            this.ResetSpeechRecognizer();
        }

        private void ResetSpeechRecognizer()
        {
            this.ClearSpeechRecognizer();

            if (string.IsNullOrEmpty(this.configString))
            {
                this.Recognizer = new LoopbackRecognizer();
            }
            else
            {
                var parts = this.configString.Split(',');
                this.Recognizer = new LoopbackRecognizer(parts[0], parts[1]);
            }

            Task.Run(async () =>
            {
                await this.Recognizer.StartAsync();
            });
        }

        private void ClearSpeechRecognizer()
        {
            var temp = this.Recognizer;
            this.Recognizer = null;

            if (temp != null)
            {
                Task.Run(async () =>
                {
                    await temp.StopAsync();
                    temp.Dispose();
                });
            }
        }

        private void OnPause(object sender, RoutedEventArgs e)
        {
            this.ClearSpeechRecognizer();
        }

        private void OnPlay(object sender, RoutedEventArgs e)
        {
            this.ResetSpeechRecognizer();
        }
    }
}
