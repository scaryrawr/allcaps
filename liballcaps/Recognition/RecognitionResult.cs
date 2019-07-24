using System;

namespace AllCaps.Recognition
{
    public class RecognitionResult
    {
        public RecognitionResult(string text, TimeSpan offset)
        {
            this.Text = text;
            this.Offset = offset;
        }

        public string Text { get; }

        public TimeSpan Offset { get; }
    }
}
