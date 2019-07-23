using NAudio.Wave;

namespace AllCaps.Input
{
    public class Wave32To16CaptureStream : Wave32To16Stream
    {
        public Wave32To16CaptureStream(WaveStream stream) : base(stream)
        {
        }

        public override WaveFormat WaveFormat => base.WaveFormat;

        public override long Length => -1;

        public override long Position { get => 0; set { } }

        public override bool CanSeek => false;

        public override bool CanRead => true;

        public override int Read(byte[] buffer, int offset, int count)
        {
            return base.Read(buffer, offset, count);
        }
    }
}
