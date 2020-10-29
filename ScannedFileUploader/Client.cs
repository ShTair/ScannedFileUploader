using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ScannedFileUploader
{
    class Client
    {
        private const long _limit = 1024 * 1024 * 100;

        private DropboxClient _client;

        public Client(string accessToken)
        {
            _client = new DropboxClient(accessToken);
        }

        public async Task UploadAsync(string path, Stream stream, long length)
        {
            if (length > _limit)
            {
                var result = await _client.Files.UploadSessionStartAsync(body: new SubStream(stream, _limit));
                var current = _limit;
                UploadSessionCursor cursor;

                while (length - current > _limit)
                {
                    cursor = new UploadSessionCursor(result.SessionId, (ulong)current);
                    await _client.Files.UploadSessionAppendV2Async(cursor, body: new SubStream(stream, _limit));
                    current += _limit;
                }

                cursor = new UploadSessionCursor(result.SessionId, (ulong)current);
                var commit = new CommitInfo(path, mode: WriteMode.Add.Instance, autorename: true);
                await _client.Files.UploadSessionFinishAsync(cursor, commit, stream);
            }
            else
            {
                await _client.Files.UploadAsync(path, mode: WriteMode.Add.Instance, autorename: true, body: stream);
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        class SubStream : Stream
        {
            private Stream _stream;
            private long _start;
            private long _length;
            private long _pos;

            public SubStream(Stream stream, long length)
            {
                _stream = stream;
                _start = stream.Position;
                _length = length;
            }

            public override long Length { get { return _length; } }

            public override bool CanRead { get { return _stream.CanRead; } }

            public override bool CanWrite { get { return false; } }

            public override bool CanSeek { get { return true; } }

            public override void Flush() { }

            public override long Position
            {
                get { return _pos; }
                set { _stream.Position = _start + (_pos = value); }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_pos >= _length) return 0;
                count = (int)Math.Min(_length - _pos, count);
                int ret = _stream.Read(buffer, offset, count);
                _pos += ret;
                return ret;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin: Position = offset; break;
                    case SeekOrigin.Current: Position += offset; break;
                    case SeekOrigin.End: Position = _length + offset; break;
                }
                return _pos;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
