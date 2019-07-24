using AllCaps.Input;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AllCaps.Recognition
{
    public class AzureSpeechRecognizer : ISpeechRecognizer
    {
        private readonly WaveStream stream;
        private readonly PushAudioInputStream pushStream;
        private readonly SpeechRecognizer recognizer;
        private string resultId;
        private readonly object lockObj;
        private CancellationTokenSource cancelTokenSource;
        private Task worker;

        public AzureSpeechRecognizer(string key, string region, WaveStream stream)
        {
            var speechConfig = SpeechConfig.FromSubscription(key, region);
            this.stream = NormalizeStream(stream);
            this.pushStream = AudioInputStream.CreatePushStream();
            this.recognizer = new SpeechRecognizer(speechConfig, AudioConfig.FromStreamInput(this.pushStream));
            this.resultId = Guid.NewGuid().ToString();
            this.lockObj = new object();

            this.recognizer.Recognized += (snd, evt) =>
            {
                string id = null;
                lock (this.lockObj)
                {
                    id = this.resultId;
                    this.resultId = Guid.NewGuid().ToString();
                }

                this.SpeechRecognized?.Invoke(this, new RecognitionEventArgs(evt, id));
            };

            this.recognizer.Recognizing += (snd, evt) =>
            {
                string id = null;
                lock (this.lockObj)
                {
                    id = this.resultId;
                }

                this.SpeechPredicted?.Invoke(this, new RecognitionEventArgs(evt, id));
            };

            this.recognizer.Canceled += (snd, evt) =>
            {
                Debug.Fail("lost recognizer");
            };
        }

        private static WaveStream NormalizeStream(WaveStream stream)
        {
            WaveStream audioStream = stream;
            if (audioStream.WaveFormat.BitsPerSample != 16)
            {
                audioStream = new Wave32To16CaptureStream(stream);
            }

            if (audioStream.WaveFormat.Channels != 1)
            {
                audioStream = new MonoCaptureStream(audioStream);
            }

            if (audioStream.WaveFormat.BitsPerSample != 16000)
            {
                audioStream = new DownsampleCaptureStream(audioStream);
            }

            return audioStream;
        }

        public event EventHandler<RecognitionEventArgs> SpeechRecognized;
        public event EventHandler<RecognitionEventArgs> SpeechPredicted;

        public async Task StartAsync()
        {
            this.cancelTokenSource = new CancellationTokenSource();
            await this.recognizer.StartContinuousRecognitionAsync();
            this.worker = this.PushStreamAsync(this.cancelTokenSource.Token);
        }

        public async Task StopAsync()
        {
            await this.recognizer.StopContinuousRecognitionAsync();
            this.cancelTokenSource?.Cancel();

            if (this.worker != null)
            {
                await this.worker;
            }
        }

        private async Task PushStreamAsync(CancellationToken token)
        {
            await Task.Run(() =>
            {
                byte[] buffer = new byte[1000];
                while (!token.IsCancellationRequested)
                {
                    int read = this.stream.Read(buffer, 0, buffer.Length);
                    this.pushStream.Write(buffer, read);
                }
            });
        }

        #region IDisposable Support
        private bool isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    this.recognizer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                isDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AzureSpeechRecognizer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
