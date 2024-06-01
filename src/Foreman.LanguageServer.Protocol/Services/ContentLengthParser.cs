using System.Text;

namespace Foreman.LanguageServer.Protocol.Services;

internal class ContentLengthParser {
    private readonly byte[] HEADING_BYTES = Encoding.UTF8.GetBytes("Content-Length: ");
    private const byte SEPARATOR_0 = (byte)'\r';
    private const byte SEPARATOR_1 = (byte)'\n';

    private readonly IDebugLogger _logger;


    public ContentLengthParser(IDebugLogger logger)
    {
        _logger = logger;
    }

    private void AssertDelimiter(IStreamingInput inputStream, Span<byte> bytes) {
        bytes.Clear();
        inputStream.ReadExactly(_logger, bytes);
        if (!bytes.SequenceEqual([SEPARATOR_1,SEPARATOR_0,SEPARATOR_1])) {
            _logger.Write("INVALID DELIMITER: " + bytes.AsString());
            throw LanguageServerException.FromCode(ErrorCode.MalformedMessage);
        }
    }

    private int GetRequiredInt(Span<byte> numericBytes) {
        if (!int.TryParse(numericBytes.AsString(), out int value)) {
            _logger.Write("Could not parse content length numeric: " + numericBytes.AsString());
            throw LanguageServerException.FromCode(ErrorCode.InvalidContentLength);
        }

        return value;
    }

    public long GetContentLength(IStreamingInput inputStream, CancellationToken cancellationToken) {
        Span<byte> headingBuffer = new byte[HEADING_BYTES.Length];

        inputStream.ReadExactly(_logger, headingBuffer);
        if (!headingBuffer.SequenceEqual(HEADING_BYTES)) {
            _logger.Write("INVALID HEADING: " + headingBuffer.AsString());
            throw LanguageServerException.FromCode(ErrorCode.MissingHeader); 
        }

        Span<byte> oneSpan = new byte[1];
        Span<byte> threeSpan = new byte[3];

        long contentLength = 0;
        bool isParsingContentLength = true;
        while (isParsingContentLength) {
            oneSpan.Clear();
            inputStream.ReadExactly(_logger, oneSpan);
            if (oneSpan[0] == SEPARATOR_0) {
                AssertDelimiter(inputStream, threeSpan);
                isParsingContentLength = false;
            } else {
                contentLength *= 10;
                contentLength += GetRequiredInt(oneSpan);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        return contentLength;
    }
}
