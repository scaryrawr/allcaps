using NAudio.Wave;

namespace AllCaps
{
    public class Wave32To16CaptureStream : Wave32To16Stream
    {
        public Wave32To16CaptureStream(WaveCaptureStream stream) : base(stream)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override WaveFormat WaveFormat => base.WaveFormat;

        public override long Length => -1;

        public override long Position { get => 0; set { } }

        public override bool CanSeek => false;

        public override bool CanRead => true;

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            return read;
        }
    }
}
