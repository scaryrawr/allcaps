using Microsoft.CognitiveServices.Speech;

namespace AllCaps.Recognition
{
    public class RecognitionEventArgs
    {
        public RecognitionEventArgs(SpeechRecognitionEventArgs evt)
        {
            this.Text = evt.Result.Text;
        }

        internal RecognitionEventArgs(System.Speech.Recognition.RecognitionEventArgs evt)
        {
            this.Text = evt.Result.Text;
        }

        public string Text { get; }
    }
}
