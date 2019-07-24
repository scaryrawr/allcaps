using Microsoft.CognitiveServices.Speech;
using System;

namespace AllCaps.Recognition
{
    public class RecognitionEventArgs
    {
        internal RecognitionEventArgs(SpeechRecognitionEventArgs evt, string id)
        {
            this.ResultId = id;
            this.Result = new RecognitionResult(evt.Result.Text, TimeSpan.FromTicks(evt.Result.OffsetInTicks));
        }

        internal RecognitionEventArgs(System.Speech.Recognition.RecognitionEventArgs evt, string resultId, TimeSpan? customStamp = null)
        {
            this.ResultId = resultId;
            this.Result = new RecognitionResult(evt.Result.Text, evt.Result?.Audio?.AudioPosition ?? customStamp ?? TimeSpan.Zero);
        }

        public string ResultId { get; }

        public RecognitionResult Result { get; }
    }
}
