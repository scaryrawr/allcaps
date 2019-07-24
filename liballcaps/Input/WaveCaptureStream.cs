using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace AllCaps.Input
{
    public class WaveCaptureStream : WaveStream
    {
        private readonly TimeSpan bufferTimeout;
        private readonly WasapiCapture capture;
        private readonly ConcurrentQueue<byte[]> buffer = new ConcurrentQueue<byte[]>();
        private byte[] carry;
        private CancellationTokenSource cancellationTokenSource;

        public WaveCaptureStream(WasapiCapture capture) : this(capture, TimeSpan.FromMilliseconds(250))
        {
        }

        public WaveCaptureStream(WasapiCapture capture, TimeSpan bufferTimeout)
        {
            this.bufferTimeout = bufferTimeout;
            this.capture = capture;
            this.capture.DataAvailable += (snd, evt) =>
            {
                // HACK: CircularBuffer seems to let us read past the writer and get
                // noise. need to implement an efficient buffer.
                byte[] buff = new byte[evt.BytesRecorded];
                Buffer.BlockCopy(evt.Buffer, 0, buff, 0, buff.Length);
                this.buffer.Enqueue(buff);
            };

            this.cancellationTokenSource = new CancellationTokenSource();

            this.capture.RecordingStopped += (snd, evt) =>
            {
                this.cancellationTokenSource.Cancel();
            };
        }

        public override WaveFormat WaveFormat => this.capture.WaveFormat;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override long Length => -1;

        public override long Position
        {
            get => 0;
            set
            {
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var token = this.cancellationTokenSource.Token;
            int read = count;
            var start = DateTime.Now;
            while (count > 0 && DateTime.Now - start < this.bufferTimeout && !token.IsCancellationRequested)
            {
                if (this.carry != null)
                {
                    // Make sure we can handle really large carry
                    this.carry = BufferCopyWithCarry(buffer, ref offset, ref count, this.carry);
                }
                else if (this.buffer.TryDequeue(out byte[] buff))
                {
                    this.carry = BufferCopyWithCarry(buffer, ref offset, ref count, buff);
                }
                else
                {
                    Thread.Sleep(0);
                }
            }

            return read;
        }

        private static byte[] BufferCopyWithCarry(byte[] buffer, ref int offset, ref int count, byte[] buff)
        {
            byte[] carry = null;

            int copyable = Math.Min(count, buff.Length);
            Buffer.BlockCopy(buff, 0, buffer, offset, copyable);

            // need to carry over into next read
            if (count < buff.Length)
            {
                carry = new byte[buff.Length - count];
                Buffer.BlockCopy(buff, count, carry, 0, carry.Length);
            }

            count -= copyable;
            offset += copyable;

            return carry;
        }
    }
}
