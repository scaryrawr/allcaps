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
            this.inProgress = new ConcurrentDictionary<string, ObservableSpeechResult>();

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
        private readonly ConcurrentDictionary<string, ObservableSpeechResult> inProgress;

        public ISpeechRecognizer Recognizer
        {
            get => this.recognizer;
            private set
            {
                // unregister callbacks if any
                if (this.recognizer != null)
                {
                    this.recognizer.SpeechRecognized -= this.OnRecognition;
                    this.recognizer.SpeechPredicted -= this.OnPrediction;
                }

                this.recognizer = value;

                if (this.recognizer != null)
                {
                    this.recognizer.SpeechRecognized += this.OnRecognition;
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
                    var temp = evt.Result.Text.Split(' ').Reverse().ToArray();
                    int toTake = Math.Min(4, temp.Length);
                    this.Preview.Text = $"{string.Join(" ", temp.Take(toTake).Reverse())}...";
                    ObservableSpeechResult speechResult = null;
                    if (this.inProgress.TryGetValue(evt.ResultId, out speechResult))
                    {
                        speechResult.Offset = evt.Result.Offset;
                        speechResult.Text = $"{evt.Result.Text}...";
                    }
                    else
                    {
                        speechResult = new ObservableSpeechResult
                        {
                            Text = $"{evt.Result.Text}...",
                            Offset = evt.Result.Offset
                        };

                        if (this.inProgress.TryAdd(evt.ResultId, speechResult))
                        {
                            this.SpeechList.Items.Add(speechResult);
                        }
                    }

                    this.SpeechList.ScrollIntoView(speechResult);
                });
            }
            catch (TaskCanceledException)
            {
                // expected for now.
            }
        }

        private void OnRecognition(object sender, RecognitionEventArgs evt)
        {
            if (!string.IsNullOrEmpty(evt.Result.Text.Trim()))
            {
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (this.inProgress.TryRemove(evt.ResultId, out var updatable))
                        {
                            updatable.Offset = evt.Result.Offset;
                            updatable.Text = evt.Result.Text;
                            this.SpeechList.ScrollIntoView(updatable);
                        }
                    });
                }
                catch (TaskCanceledException)
                {
                    // expected for now
                }
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

            this.RecognizerLabel.Text = this.Recognizer.RecognizerName;
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
