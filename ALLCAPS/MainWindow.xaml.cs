using AllCaps.Recognition;
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

            this.Closing += async (snd, evt) =>
            {
                await this.Recognizer.StopAsync();
                var hndl = this.Recognizer;
                this.Recognizer = null;
                hndl.Dispose();
            };

            this.Recognizer = new LoopbackRecognizer();
            this.Loaded += async (snd, evt) =>
            {
                await this.Recognizer.StartAsync();
            };
        }

        private ISpeechRecognizer recognizer;
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
            catch (TaskCanceledException ex)
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
                catch (TaskCanceledException ex)
                {
                    // expected for now
                }
            }
        }
    }
}
