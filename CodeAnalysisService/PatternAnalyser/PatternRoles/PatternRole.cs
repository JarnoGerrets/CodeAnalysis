using CodeAnalysisService.GraphService.Nodes;

namespace CodeAnalysisService.PatternAnalyser.PatternRoles
{
    /// <summary>
    /// Represents a role played by a class in a detected design pattern.
    /// Example: "Subject", "Observer", "Singleton", etc.
    /// </summary>
    public record PatternRole(string Role, IAnalyzerNode Class);
}
