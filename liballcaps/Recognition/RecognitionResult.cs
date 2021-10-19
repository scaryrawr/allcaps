using System;

namespace AllCaps.Recognition
{
    public class RecognitionResult
    {
        public RecognitionResult(string text, TimeSpan offset, TimeSpan duration)
        {
            this.Text = text;
            this.Offset = offset;
            this.Duration = duration;
        }

        public string Text { get; }

        public TimeSpan Offset { get; }

        public TimeSpan Duration { get; }
    }
}
