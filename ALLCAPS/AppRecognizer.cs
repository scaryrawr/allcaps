using AllCaps.Input;
using AllCaps.Recognition;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Threading.Tasks;

namespace ALLCAPS
{
    public class LoopbackRecognizer : ISpeechRecognizer
    {
        private readonly WasapiCapture capture;
        private readonly WaveCaptureStream stream;
        private readonly ISpeechRecognizer recognizer;

        public event EventHandler<RecognitionEventArgs> SpeechRecognized;
        public event EventHandler<RecognitionEventArgs> SpeechPredicted;

        public LoopbackRecognizer()
        {
            this.capture = new WasapiLoopbackCapture();
            this.stream = new WaveCaptureStream(this.capture);
            this.recognizer = new LocalSpeechRecognizer(this.stream);
            this.recognizer.SpeechPredicted += (snd, evt) => this.SpeechPredicted?.Invoke(snd, evt);
            this.recognizer.SpeechRecognized += (snd, evt) => this.SpeechRecognized?.Invoke(snd, evt);
        }

        public LoopbackRecognizer(string key, string region)
        {
            this.capture = new WasapiLoopbackCapture();
            this.stream = new WaveCaptureStream(this.capture);
            this.recognizer = new AzureSpeechRecognizer(key, region, this.stream);
            this.recognizer.SpeechPredicted += (snd, evt) => this.SpeechPredicted?.Invoke(snd, evt);
            this.recognizer.SpeechRecognized += (snd, evt) => this.SpeechRecognized?.Invoke(snd, evt);
        }

        public async Task StartAsync()
        {
            this.capture.StartRecording();
            await this.recognizer.StartAsync();
        }

        public async Task StopAsync()
        {
            await this.recognizer.StopAsync();
            this.capture.StopRecording();
        }

        #region IDisposable Support
        private bool isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.recognizer.Dispose();
                    this.stream.Dispose();
                    this.capture.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.isDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LoopbackRecognizer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
