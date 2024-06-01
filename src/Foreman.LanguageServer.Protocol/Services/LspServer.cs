using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Foreman.LanguageServer.Protocol.Services;

public class LspServer : IHostedService {
    private readonly IDebugLogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Lazy<IStreamingInput> _inputStream;
    private readonly Lazy<IStreamingOutput> _outputStream;
    private readonly Lazy<ContentLengthParser> _contentLengthParser;
    private readonly Lazy<MessageHandler> _messageHandler;
    private Task? _task;

    public LspServer(
        IDebugLogger logger,
        IServiceProvider serviceProvider
    ) {
        _logger = logger;
        _serviceProvider = serviceProvider;

        _inputStream = Build<IStreamingInput>();
        _outputStream = Build<IStreamingOutput>();
        _contentLengthParser = Build<ContentLengthParser>();
        _messageHandler = Build<MessageHandler>();
    }

    private Lazy<T> Build<T>() where T : notnull
        => new(_serviceProvider.GetRequiredService<T>);

    public Task StartAsync(CancellationToken cancellationToken) {
        _logger.Write("Started language server");
        _task = Scan(cancellationToken);
        return Task.CompletedTask;
    }

    private Task Scan(CancellationToken cancellationToken) {
        bool doContinue = true;
        while (doContinue) {
            try {
                long contentLength = _contentLengthParser.Value.GetContentLength(_inputStream.Value, cancellationToken);
                Span<byte> contentBytes = new byte[contentLength];
                _inputStream.Value.ReadExactly(_logger, contentBytes);
                _messageHandler.Value.HandleMessageAsync(contentBytes, _outputStream.Value);
            } catch (TaskCanceledException) {
                doContinue = !cancellationToken.IsCancellationRequested;
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => _task ?? Task.CompletedTask;
}