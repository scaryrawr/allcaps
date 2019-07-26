using NAudio.Wave;
using System;

namespace AllCaps.Input
{
    public enum MonoTransform
    {
        Average,
        Left,
        Right
    }

    public class MonoCaptureStream : EndlessWaveStream
    {
        private readonly WaveStream stream;

        public MonoCaptureStream(WaveStream stream, MonoTransform strat = MonoTransform.Average)
        {
            this.stream = stream;
            if (stream.WaveFormat.BitsPerSample != 16)
            {
                throw new ArgumentException("Only works on 16 bit streams");
            }

            if (stream.WaveFormat.Channels != 2)
            {
                throw new ArgumentException("Only works on 2 channel streams");
            }

            this.MonoTransform = strat;

            this.WaveFormat = new WaveFormat(stream.WaveFormat.SampleRate, stream.WaveFormat.BitsPerSample, 1);
        }

        public MonoTransform MonoTransform { get; }

        public override WaveFormat WaveFormat { get; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            byte[] buff = new byte[count * 2];

            this.stream.Read(buff, 0, buff.Length);

            byte[] transformed = this.Merge(buff);
            Buffer.BlockCopy(transformed, 0, buffer, offset, transformed.Length);

            return transformed.Length;
        }

        private byte[] Merge(byte[] bytes)
        {
            short[] asShort = new short[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, asShort, 0, bytes.Length);
            short[] res = new short[asShort.Length / 2];
            for (int x = 0; x < res.Length; ++x)
            {
                res[x] = Merge(asShort[x * 2], asShort[x * 2 + 1]);
            }

            byte[] resBytes = new byte[asShort.Length];
            Buffer.BlockCopy(res, 0, resBytes, 0, resBytes.Length);

            return resBytes;
        }

        private short Merge(short left, short right)
        {
            switch (this.MonoTransform)
            {
                case MonoTransform.Average: return (short)((left / 2) + (right / 2));
                case MonoTransform.Left: return left;
                case MonoTransform.Right: return right;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
