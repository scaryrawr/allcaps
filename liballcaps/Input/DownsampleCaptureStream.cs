using NAudio.Wave;
using System;

namespace AllCaps.Input
{
    public class DownsampleCaptureStream : EndlessWaveStream
    {
        public DownsampleCaptureStream(WaveStream stream)
        {
            this.stream = stream;

            // HACK: expecting bits to be 16 and channels to be 1.
            this.WaveFormat = new WaveFormat(16000, stream.WaveFormat.BitsPerSample, stream.WaveFormat.Channels);
        }

        public MonoTransform MonoTransform { get; }

        private readonly WaveStream stream;

        public override WaveFormat WaveFormat { get; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int resampleStep = this.stream.WaveFormat.SampleRate / 16000;

            byte[] buff = new byte[count * resampleStep];
            this.stream.Read(buff, 0, buff.Length);
            byte[] res = this.Resample(buff, resampleStep);

            Buffer.BlockCopy(res, 0, buffer, offset, count);

            return count;
        }

        private byte[] Resample(byte[] buff, int resampleStep)
        {
            short[] asShort = new short[buff.Length * 2];

            Buffer.BlockCopy(buff, 0, asShort, 0, buff.Length);
            short[] res = new short[asShort.Length / resampleStep];

            for(int x = 0; x < res.Length; ++x)
            {
                res[x] = asShort[x * resampleStep];
            }

            byte[] resBytes = new byte[res.Length * 2];
            Buffer.BlockCopy(res, 0, resBytes, 0, resBytes.Length);

            return resBytes;
        }
    }
}
