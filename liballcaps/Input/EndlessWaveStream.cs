using NAudio.Wave;

namespace AllCaps.Input
{
    public abstract class EndlessWaveStream : WaveStream
    {
        public override long Length => -1;

        public override long Position { get => 0; set { } }

        public override bool CanSeek => false;

        public override bool CanRead => true;
    }
}
