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
                    if (this.inProgress.TryGetValue(evt.ResultId, out ObservableSpeechResult speechResult))
                    {
                        speechResult.Offset = evt.Result.Offset;
                        speechResult.Text = $"{evt.Result.Text}...";
                        speechResult.Duration = evt.Result.Duration;
                    }
                    else
                    {
                        speechResult = new ObservableSpeechResult
                        {
                            Text = $"{evt.Result.Text}...",
                            Offset = evt.Result.Offset,
                            Duration = evt.Result.Duration
                        };
                    }

                    // TODO: We need a better way to handle run-on sentences, but a moving window is... cray
                    //// We only want to show the latest 3 seconds
                    //if (speechResult.Duration > TimeSpan.FromSeconds(3))
                    //{
                    //    var percentage = TimeSpan.FromSeconds(3).TotalMilliseconds / speechResult.Duration.TotalMilliseconds;
                    //    var words = speechResult.Text.Split(' ');
                    //    speechResult.Text = "";
                    //    for (int i = (int)(words.Length * (1 - percentage)); i < words.Length; ++i)
                    //    {
                    //        speechResult.Text += words[i] + ' ';
                    //    }
                    //}

                    this.DataContext = speechResult;
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

        bool mouseDown = false;
        Point downPosition = new Point(0, 0);
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mouseDown = true;
            downPosition = e.GetPosition(this);
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mouseDown = false;
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!mouseDown)
            {
                return;
            }
            var position = e.GetPosition(this);
            this.Left += position.X - downPosition.X;
            this.Top += position.Y - downPosition.Y;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = true;
        }
    }
}
