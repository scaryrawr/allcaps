using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Collections.Concurrent;
using System.Threading;

namespace AllCaps.Input
{
    public class WaveCaptureStream : WaveStream
    {
        private readonly WasapiCapture capture;
        private readonly ConcurrentQueue<byte> buffer = new ConcurrentQueue<byte>();

        public WaveCaptureStream(WasapiCapture capture)
        {
            this.capture = capture;
            this.capture.DataAvailable += (snd, evt) =>
            {
                // HACK: CircularBuffer seems to let us read past the writer and get
                // noise. need to implement an efficient buffer.
                for (int x = 0; x < evt.BytesRecorded; ++x)
                {
                    this.buffer.Enqueue(evt.Buffer[x]);
                }
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
            int read = 0;
            // HACK: if we ever return less than count the speech recognition
            // engine will assume we've reached the end of the stream, so we
            // need a way to efficiently wait for the required amount of data
            while (read < count)
            {
                if (this.buffer.TryDequeue(out byte b))
                {
                    buffer[offset++] = b;
                    ++read;
                }
                else
                {
                    Thread.Sleep(0);
                }
            }

            return read;
        }
    }
}
