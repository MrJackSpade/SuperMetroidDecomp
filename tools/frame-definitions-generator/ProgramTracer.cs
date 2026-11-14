using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// The programs a projectile program family's construction can start it in when its program argument is not a constant
/// (#1784), as a spawner starting the program its callers hand it does: a program member named outright; for a parameter of
/// the method holding it, or a positional record's property, what the calls or constructions passing that parameter pass,
/// traced on in turn. Since the projectile behaviours took over from the kind-keyed start tables (#1760), this is how a kind's
/// spawner names the program it starts.
/// </summary>
/// <param name="model">The Core facts.</param>
/// <param name="reaches">
/// Whether a walk reaches a unit: the calls a tracer follows are those whose code the walk reaches, or every one in Core when
/// the walk reaches none; null to follow every call.
/// </param>
internal sealed class ProgramTracer(CoreModel model, Func<Unit, bool>? reaches)
{
    /// <summary>The programs of <paramref name="programType"/> <paramref name="expression"/> can be; null when one of them may be any.</summary>
    /// <param name="programType">The family's program type.</param>
    /// <param name="expression">The program expression.</param>
    /// <param name="semantic">The semantic model it binds in.</param>
    public HashSet<string>? Programs(INamedTypeSymbol programType, ExpressionSyntax expression, SemanticModel semantic) =>
        Traced(programType, expression, semantic, new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default));

    /// <summary>The programs <paramref name="expression"/> can be, with <paramref name="visiting"/> the parameters being traced.</summary>
    private HashSet<string>? Traced(INamedTypeSymbol programType, ExpressionSyntax expression, SemanticModel semantic, HashSet<IParameterSymbol> visiting)
    {
        var programs = new HashSet<string>(StringComparer.Ordinal);
        foreach (ExpressionSyntax choice in CoreModel.Choices(expression))
        {
            ISymbol? symbol = semantic.GetSymbolInfo(choice).Symbol;
            if (symbol is IFieldSymbol member && SymbolEqualityComparer.Default.Equals(member.ContainingType, programType))
            {
                programs.Add(member.Name);
                continue;
            }
            if (model.HandedParameter(symbol) is not { ContainingSymbol: IMethodSymbol callee } handed)
                return null;
            // A cycle of calls handing the program round adds nothing of its own.
            if (!visiting.Add(handed))
                continue;
            HashSet<string>? passed = Passed(programType, callee, handed, visiting);
            visiting.Remove(handed);
            if (passed is null)
                return null;
            programs.UnionWith(passed);
        }
        return programs;
    }

    /// <summary>
    /// The programs the calls of <paramref name="callee"/> pass for <paramref name="parameter"/>: those whose code the walk
    /// reaches, or every one when it reaches none; null when none calls it, or one leaves it to its default or passes what
    /// cannot be traced.
    /// </summary>
    private HashSet<string>? Passed(INamedTypeSymbol programType, IMethodSymbol callee, IParameterSymbol parameter, HashSet<IParameterSymbol> visiting)
    {
        IReadOnlyList<CallSite> sites = model.CallSites(callee);
        List<CallSite> reached = reaches is null ? [] : [.. sites.Where(site => site.Caller is { } caller && reaches(caller))];
        if (sites.Count == 0)
            return null;
        var programs = new HashSet<string>(StringComparer.Ordinal);
        foreach (CallSite site in reached.Count > 0 ? reached : sites)
        {
            if (site.ArgumentFor(parameter) is not { } argument || Traced(programType, argument, site.Semantic, visiting) is not { } traced)
                return null;
            programs.UnionWith(traced);
        }
        return programs;
    }
}
