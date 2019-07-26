using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using System;

namespace AllCaps.Input
{
    public class VoiceFilterCaptureStream : WaveCaptureStream
    {
        private readonly BiQuadFilter hiFilter;
        private readonly BiQuadFilter lowFilter;

        public VoiceFilterCaptureStream(WasapiCapture capture) : this(capture, TimeSpan.FromMilliseconds(250))
        {
        }

        public VoiceFilterCaptureStream(WasapiCapture capture, TimeSpan timeout) : base(capture, timeout)
        {
            this.hiFilter = BiQuadFilter.HighPassFilter(this.WaveFormat.SampleRate, 300, .45f);
            this.lowFilter = BiQuadFilter.LowPassFilter(this.WaveFormat.SampleRate, 3000, .45f);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            float[] transform = new float[count / sizeof(float)];
            Buffer.BlockCopy(buffer, offset, transform, 0, count);
            for(int x = 0; x < transform.Length; ++x)
            {
                transform[x] = this.hiFilter.Transform(transform[x]);
                transform[x] = this.lowFilter.Transform(transform[x]);
            }

            Buffer.BlockCopy(transform, 0, buffer, offset, count);

            return read;
        }
    }
}
