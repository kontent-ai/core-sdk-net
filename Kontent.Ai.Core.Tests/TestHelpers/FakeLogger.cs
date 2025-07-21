using System.Collections.Concurrent;

namespace Kontent.Ai.Core.Tests.TestHelpers;

public class FakeLogger<T> : ILogger<T>
{
    public FakeLogCollector Collector { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => new NoOpDisposable();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        Collector.AddLog(new LogEntry(logLevel, eventId, message, exception));
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}

public class FakeLogCollector
{
    private readonly ConcurrentQueue<LogEntry> _logs = new();

    public void AddLog(LogEntry log) => _logs.Enqueue(log);

    public IReadOnlyList<LogEntry> GetSnapshot() => _logs.ToList();

    public void Clear()
    {
        while (_logs.TryDequeue(out _)) { }
    }
}

public record LogEntry(LogLevel Level, EventId EventId, string Message, Exception? Exception); 