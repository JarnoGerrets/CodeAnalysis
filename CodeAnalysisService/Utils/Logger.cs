namespace CodeAnalysisService.Utils
{
    public static class Logger
    {
        private static readonly string logFile = "debug.log";

        public static void Log(string message)
        {
            // Add timestamp for clarity
            string line = $"[{DateTime.Now:HH:mm:ss}] {message}";

            // Append to file
            File.AppendAllText(logFile, line + Environment.NewLine);
        }
    }
}
