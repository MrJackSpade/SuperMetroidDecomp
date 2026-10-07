using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>A reflection lookup site, formatted as repository file, line and source text.</summary>
internal sealed record ReflectionSite(string File, int Line, string Text)
{
    public override string ToString() => $"{File}\t{Line}\t{Text}";
}

/// <summary>
/// Resolves reflection lookups by name (Type.GetMethod/GetProperty/GetField/GetConstructor/...,
/// Activator.CreateInstance) to repository members, so members reached only this way can be
/// required to carry [AccessedByReflection]. Lookups that cannot be resolved statically are listed.
/// </summary>
internal sealed class ReflectionScan(SymbolIdentity identity)
{
    private static readonly HashSet<string> NamedLookups =
        ["GetMethod", "GetProperty", "GetField", "GetConstructor", "GetMember", "GetEvent", "GetNestedType"];
    private static readonly HashSet<string> Enumerations =
        ["GetMethods", "GetProperties", "GetFields", "GetConstructors", "GetMembers", "GetEvents", "GetNestedTypes"];

    /// <summary>Unmarked repository members resolved as reflection targets, with one lookup site each.</summary>
    public Dictionary<string, ReflectionSite> UnmarkedTargets { get; } = [];
    public HashSet<ReflectionSite> UnresolvedSites { get; } = [];
    public HashSet<ReflectionSite> EnumerationSites { get; } = [];

    /// <summary>Identifier literals near lookups whose receiver type is unknown.</summary>
    public HashSet<(string Name, ReflectionSite Site)> CandidateNames { get; } = [];

    public void ObserveInvocation(SemanticModel model, InvocationExpressionSyntax invocation, IMethodSymbol method)
    {
        string container = method.ContainingType.ToDisplayString();
        bool activator = container == "System.Activator" && method.Name == "CreateInstance";
        bool lookup = container == "System.Type" && NamedLookups.Contains(method.Name);
        bool enumeration = container == "System.Type" && Enumerations.Contains(method.Name);
        if (!activator && !lookup && !enumeration)
            return;
        var site = Site(invocation);

        if (activator)
        {
            ITypeSymbol? created = method.TypeArguments is [var generic] ? generic
                : invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression is TypeOfExpressionSyntax typeOf
                    ? model.GetTypeInfo(typeOf.Type).Type : null;
            if (created is INamedTypeSymbol type && identity.Key(type) is not null)
                AddTarget(type, site);
            else
                UnresolvedSites.Add(site);
            return;
        }

        var receiver = ReceiverType(model, invocation);
        if (enumeration)
        {
            if (receiver is null || identity.Key(receiver) is not null)
                EnumerationSites.Add(site);
            return;
        }

        string? name = method.Name == "GetConstructor" ? ".ctor"
            : invocation.ArgumentList.Arguments.FirstOrDefault() is { } first
              && model.GetConstantValue(first.Expression) is { HasValue: true, Value: string constant } ? constant : null;
        if (receiver is not null && name is not null)
        {
            if (identity.Key(receiver) is null)
                return;
            var members = MembersNamed(receiver, name).ToList();
            if (members.Count == 0)
                UnresolvedSites.Add(site);
            foreach (var member in members)
                AddTarget(member, site);
            return;
        }

        // The name or receiver is computed: resolved after the walk from identifier literals in
        // the same method and, when the name is a parameter, the constants every caller passes.
        UnresolvedSites.Add(site);
        if (receiver is not null && identity.Key(receiver) is null)
            return;
        var nameParameter = invocation.ArgumentList.Arguments.FirstOrDefault() is { Expression: IdentifierNameSyntax identifier }
            ? model.GetSymbolInfo(identifier).Symbol as IParameterSymbol : null;
        computedLookups.Add(new ComputedLookup(receiver, EnclosingLiterals(invocation), nameParameter, site));
    }

    /// <summary>Records constant string arguments of repository calls, for name parameters of lookup helpers.</summary>
    public void ObserveCall(SemanticModel model, InvocationExpressionSyntax invocation, IMethodSymbol method)
    {
        if (!method.Locations.Any(l => l.IsInSource))
            return;
        var definition = method.ReducedFrom ?? method;
        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (model.GetConstantValue(argument.Expression) is not { HasValue: true, Value: string value })
                continue;
            var parameter = argument.NameColon is { } named
                ? definition.Parameters.FirstOrDefault(p => p.Name == named.Name.Identifier.ValueText)
                : definition.Parameters.ElementAtOrDefault(invocation.ArgumentList.Arguments.IndexOf(argument)
                    + (method.ReducedFrom is null ? 0 : 1));
            if (parameter is not null)
                callerConstants.Add((ParameterKey(parameter), value));
        }
    }

    /// <summary>Resolves every computed lookup once all call sites have been observed.</summary>
    public void Resolve()
    {
        foreach (var lookup in computedLookups)
        {
            var names = new HashSet<string>(lookup.Literals);
            if (lookup.NameParameter is { } parameter)
                names.UnionWith(callerConstants.Where(c => c.Parameter == ParameterKey(parameter)).Select(c => c.Value));
            foreach (string name in names)
            {
                if (lookup.Receiver is null)
                    CandidateNames.Add((name, lookup.Site));
                else
                    foreach (var member in MembersNamed(lookup.Receiver, name))
                        AddTarget(member, lookup.Site);
            }
        }
    }

    private sealed record ComputedLookup(INamedTypeSymbol? Receiver, HashSet<string> Literals, IParameterSymbol? NameParameter,
        ReflectionSite Site);

    private readonly List<ComputedLookup> computedLookups = [];
    private readonly HashSet<(string Parameter, string Value)> callerConstants = [];

    private static string ParameterKey(IParameterSymbol parameter) =>
        $"{parameter.ContainingSymbol.OriginalDefinition.ToDisplayString()}#{parameter.Ordinal}";

    private void AddTarget(ISymbol member, ReflectionSite site)
    {
        if (!RootCollector.HasReflectionMarker(member) && identity.Key(member) is { } key)
            UnmarkedTargets.TryAdd(key, site);
    }

    private static IEnumerable<ISymbol> MembersNamed(INamedTypeSymbol type, string name)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
            foreach (var member in current.GetMembers(name).Where(m => !m.IsImplicitlyDeclared))
                yield return member;
    }

    /// <summary>typeof(T).Lookup(...) or value.GetType().Lookup(...); anything else is unknown.</summary>
    private static INamedTypeSymbol? ReceiverType(SemanticModel model, InvocationExpressionSyntax invocation) =>
        invocation.Expression is not MemberAccessExpressionSyntax access ? null
        : access.Expression is TypeOfExpressionSyntax typeOf ? model.GetTypeInfo(typeOf.Type).Type as INamedTypeSymbol
        : access.Expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "GetType" } getType }
            ? model.GetTypeInfo(getType.Expression).Type as INamedTypeSymbol
        : null;

    private static HashSet<string> EnclosingLiterals(SyntaxNode node)
    {
        var scope = node.Ancestors().FirstOrDefault(a => a is BaseMethodDeclarationSyntax or AccessorDeclarationSyntax
            or PropertyDeclarationSyntax or GlobalStatementSyntax) ?? node;
        return scope.DescendantNodes().OfType<LiteralExpressionSyntax>()
            .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression) && SyntaxFacts.IsValidIdentifier(l.Token.ValueText))
            .Select(l => l.Token.ValueText).ToHashSet();
    }

    private ReflectionSite Site(SyntaxNode node) => new(identity.Relative(node.SyntaxTree.FilePath),
        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
        string.Join(' ', node.ToString().Split(['\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())));
}
