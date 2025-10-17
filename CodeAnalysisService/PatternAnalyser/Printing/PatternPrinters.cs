using CodeAnalysisService.PatternAnalyser.RuleResult;
using CodeAnalysisService.PatternAnalyser.Names;

namespace CodeAnalysisService.PatternAnalyser.Printing
{
    /// <summary>
    /// Pattern specific printers.
    /// </summary>
    public class ObserverPatternPrinter : BasePatternPrinter
    {
        public override string PatternName => PatternNames.Observer;

        protected override void PrintRoles(PatternResult result)
        {
            PrintRoleGroup(result.Roles.Where(r => r.Role == "Subject"), "Subject(s)");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "Observer"), "Observer(s)");
        }
    }

    public class SingletonPatternPrinter : BasePatternPrinter
    {
        public override string PatternName => PatternNames.Singleton;

        protected override void PrintRoles(PatternResult result)
        {
            PrintRoleGroup(result.Roles.Where(r => r.Role == "Singleton"), "Singleton(s)");
        }
    }

    public class FactoryMethodPatternPrinter : BasePatternPrinter
    {
        public override string PatternName => PatternNames.FactoryMethod;

        protected override void PrintRoles(PatternResult result)
        {
            PrintRoleGroup(result.Roles.Where(r => r.Role == "AbstractFactory"), "Abstract Factory");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "ConcreteFactory"), "Concrete Factories");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "Product"), "Products");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "InvalidProduct"), "Failed Creations");
        }
    }

    public class StrategyPatternPrinter : BasePatternPrinter
    {
        public override string PatternName => PatternNames.Strategy;

        protected override void PrintRoles(PatternResult result)
        {
            PrintRoleGroup(result.Roles.Where(r => r.Role == "Strategy"), "Strategy Interface/Abstract");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "ConcreteStrategy"), "Concrete Strategies");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "Context"), "Context(s)");
        }
    }

    public class AdapterPatternPrinter : BasePatternPrinter
    {
        public override string PatternName => PatternNames.Adapter;

        protected override void PrintRoles(PatternResult result)
        {
            PrintRoleGroup(result.Roles.Where(r => r.Role == "Target"), "Target(s)");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "Adapter"), "Adapter(s)");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "Adaptee"), "Adaptee(s)");
        }
    }

    public class StatePatternPrinter : BasePatternPrinter
    {
        public override string PatternName => PatternNames.State;

        protected override void PrintRoles(PatternResult result)
        {
            PrintRoleGroup(result.Roles.Where(r => r.Role == "StateInterface"), "State Interface/Abstract");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "ConcreteState"), "Concrete States");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "StateContextCandidate"), "Context Candidates");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "StateContext"), "State Context(s)");
            PrintRoleGroup(result.Roles.Where(r => r.Role == "StateNameHint"), "Naming Hints");
        }
    }
}
