public abstract class LoggerFactory
{
    public abstract ILogger CreateLogger();
}

public class CachedLoggerFactory : LoggerFactory
{
    private static readonly ILogger _logger = new FileLogger();

    public override ILogger CreateLogger() => _logger; // no "new"
}

public interface ILogger { }
public class FileLogger : ILogger { }
