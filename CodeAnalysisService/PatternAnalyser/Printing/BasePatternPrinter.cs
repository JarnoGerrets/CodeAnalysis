using CodeAnalysisService.PatternAnalyser.RuleResult;
using System;

namespace CodeAnalysisService.PatternAnalyser.Printing
{
    public abstract class BasePatternPrinter : IPatternPrinter
    {
        public abstract string PatternName { get; }

        public void Print(PatternResult result)
        {
            PrintHeader(result);
            PrintRoles(result);
            PrintFooter();
        }

        protected abstract void PrintRoles(PatternResult result);

        protected void PrintHeader(PatternResult result)
        {
            Console.WriteLine(new string('-', 50));
            Console.WriteLine("Pattern Match Found:");
            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"Pattern: {result.Rule.Name}");
            Console.WriteLine($"Score: {result.Score}/{result.Rule.expectedTotalScore}");
            Console.WriteLine($"Passed Must-Pass Steps: {result.PassedMustPass}");
        }

        protected void PrintFooter()
        {
            Console.WriteLine(new string('-', 50));
            Console.WriteLine();
        }

        protected void PrintRoleGroup(IEnumerable<PatternRoles.PatternRole> roles, string label)
        {
            if (!roles.Any()) return;
            Console.WriteLine($"{label}:");
            foreach (var r in roles)
                Console.WriteLine($" - {r.Class.Symbol.Name}");
        }
    }
}
