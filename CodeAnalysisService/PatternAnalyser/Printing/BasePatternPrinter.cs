using CodeAnalysisService.PatternAnalyser.RuleResult;
using CodeAnalysisService.PatternAnalyser.PatternRoles;

namespace CodeAnalysisService.PatternAnalyser.Printing
{
    /// <summary>
    /// Printing the results of the analyser. purely for debug purposes.
    /// </summary>
    public abstract class BasePatternPrinter : IPatternPrinter
    {
        public abstract string PatternName { get; }

        public void Print(PatternResult result)
        {
            PrintHeader(result);
            PrintChecks(result);
            PrintRoles(result); 
            PrintFooter();
        }

        private void PrintChecks(PatternResult result)
        {
            Console.WriteLine("Checks:");
            foreach (var check in result.Checks)
            {
                var status = check.Passed ? "✔" : "✖";
                var weight = check.Weight > 0 ? $"(+{check.Weight})" : string.Empty;
                Console.WriteLine($" {status} {check.Description} {weight}");
            }
            Console.WriteLine();
        }

        protected abstract void PrintRoles(PatternResult result);

        protected void PrintHeader(PatternResult result)
        {
            Console.WriteLine(new string('-', 50));
            Console.WriteLine("Pattern Match Found:");
            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"Pattern: {result.PatternName}");
            Console.WriteLine($"Confidence: {result.Score}% ({GetConfidenceLabel(result.Score)})");
        }

        protected void PrintFooter()
        {
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();
        }

        protected void PrintRoleGroup(IEnumerable<PatternRole> roles, string label)
        {
            if (!roles.Any()) return;
            Console.WriteLine($"{label}:");
            foreach (var r in roles)
                Console.WriteLine($" - {r.Class.Symbol.Name}");
            Console.WriteLine();
        }

        private string GetConfidenceLabel(int score) =>
            score < 50 ? "Fail (not recognized)" :
            score < 70 ? "Attempted but weak" :
            score < 80 ? "Almost there" :
            "Strong match";
    }
}
