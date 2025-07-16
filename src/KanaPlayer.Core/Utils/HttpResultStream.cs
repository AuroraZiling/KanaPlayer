namespace KanaPlayer.Core.Utils;

public class HttpResultStream(Stream inner, long contentLength): Stream
{
    public override void Flush() { }
    
    public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
    
    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                if (offset < 0 || offset > contentLength)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                Position = offset;
                return Position;
            case SeekOrigin.Current:
                if (Position + offset < 0 || Position + offset > contentLength)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                Position += offset;
                return Position;
            case SeekOrigin.End:
                if (contentLength + offset < 0 || contentLength + offset > contentLength)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                Position = contentLength + offset;
                return Position;
            default:
                throw new ArgumentException("Invalid SeekOrigin", nameof(origin));
        }
    }
    
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }
    
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
    
    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => contentLength;

    public override long Position { get; set; }
}