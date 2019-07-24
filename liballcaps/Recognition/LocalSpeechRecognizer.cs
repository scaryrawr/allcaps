using AllCaps.Input;
using NAudio.Wave;
using System;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Threading.Tasks;

namespace AllCaps.Recognition
{
    public class LocalSpeechRecognizer : ISpeechRecognizer
    {
        private readonly SpeechRecognitionEngine recognizer;

        public LocalSpeechRecognizer(WaveStream stream)
        {
            this.recognizer = new SpeechRecognitionEngine();
            this.recognizer.LoadGrammar(new DictationGrammar());
            this.resultId = Guid.NewGuid().ToString();
            this.lockObj = new object();

            this.recognizer.SpeechRecognized += (sndr, evt) =>
            {
                string id = null;
                lock (this.lockObj)
                {
                    id = this.resultId;
                    this.resultId = Guid.NewGuid().ToString();
                }

                this.SpeechRecognized?.Invoke(this, new RecognitionEventArgs(evt, id));
            };

            this.recognizer.SpeechHypothesized += (snder, evt) =>
            {
                string id = null;
                lock (this.lockObj)
                {
                    id = this.resultId;
                }

                this.SpeechPredicted?.Invoke(this, new RecognitionEventArgs(evt, id, DateTime.Now - this.startTime));
            };

            WaveStream audioStream = stream;
            if (audioStream.WaveFormat.BitsPerSample != 16)
            {
                audioStream = new Wave32To16CaptureStream(stream);
            }

            this.recognizer.SetInputToAudioStream(audioStream, new SpeechAudioFormatInfo(audioStream.WaveFormat.SampleRate, (AudioBitsPerSample)audioStream.WaveFormat.BitsPerSample, (AudioChannel)audioStream.WaveFormat.Channels));
        }

        public async Task StartAsync()
        {
            this.startTime = DateTime.Now;
            this.recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        public async Task StopAsync()
        {
            this.recognizer.RecognizeAsyncStop();
        }

        public event EventHandler<RecognitionEventArgs> SpeechRecognized;

        public event EventHandler<RecognitionEventArgs> SpeechPredicted;

        private string resultId;

        private DateTime startTime;

        private readonly object lockObj;

        public string RecognizerName => nameof(LocalSpeechRecognizer);

        #region IDisposable Support
        private bool isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.recognizer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.isDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LocalSpeechRecognizer()
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
