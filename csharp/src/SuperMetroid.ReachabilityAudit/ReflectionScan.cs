using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>A reflection lookup site, formatted as repository file, line and source text.</summary>
internal sealed record ReflectionSite(string File, int Line, string Text)
{
    /// <summary>Formats a lookup site as a tab-separated path, line, and compact source expression.</summary>
    public override string ToString() => $"{File}\t{Line}\t{Text}";
}

/// <summary>
/// Resolves reflection lookups by name (Type.GetMethod/GetProperty/GetField/GetConstructor/...,
/// Activator.CreateInstance) to repository members. A resolved lookup is a reference from the code
/// performing it. Lookups that cannot be resolved statically are listed: their targets need
/// [AccessedByReflection].
/// </summary>
internal sealed class ReflectionScan(SymbolIdentity identity, ReachabilityGraph graph)
{
    /// <summary>Single-member Type reflection APIs whose string argument can name a graph target.</summary>
    private static readonly HashSet<string> NamedLookups =
        ["GetMethod", "GetProperty", "GetField", "GetConstructor", "GetMember", "GetEvent", "GetNestedType"];
    /// <summary>Type reflection APIs that can reach all members of one declaration kind.</summary>
    private static readonly HashSet<string> Enumerations =
        ["GetMethods", "GetProperties", "GetFields", "GetConstructors", "GetMembers", "GetEvents", "GetNestedTypes"];

    /// <summary>Lookup calls whose target name or receiver cannot be fully proven from local constants.</summary>
    public HashSet<ReflectionSite> UnresolvedSites { get; } = [];
    /// <summary>Member enumeration calls for which the receiving Type cannot be identified.</summary>
    public HashSet<ReflectionSite> EnumerationSites { get; } = [];

    /// <summary>Identifier literals near lookups whose receiver type is unknown.</summary>
    public HashSet<(string Name, ReflectionSite Site)> CandidateNames { get; } = [];

    /// <summary>Adds reachability edges for statically resolved Type and Activator calls and records dynamic lookups for later resolution.</summary>
    public void ObserveInvocation(SemanticModel model, InvocationExpressionSyntax invocation, IMethodSymbol method, string owner)
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
                graph.Edge(owner, identity.Key(type));
            else
                UnresolvedSites.Add(site);
            return;
        }

        var receiver = ReceiverType(model, invocation);
        if (enumeration)
        {
            // Enumerating a known repository type reaches every member of the enumerated kind.
            if (receiver is not null && identity.Key(receiver) is not null)
                foreach (var member in EnumeratedMembers(receiver, method.Name))
                    graph.Edge(owner, identity.Key(member));
            else if (receiver is null)
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
                graph.Edge(owner, identity.Key(member));
            return;
        }

        // The name or receiver is computed: resolved after the walk from identifier literals in
        // the same method and, when the name is a parameter, the constants every caller passes.
        UnresolvedSites.Add(site);
        if (receiver is not null && identity.Key(receiver) is null)
            return;
        var nameParameter = invocation.ArgumentList.Arguments.FirstOrDefault() is { Expression: IdentifierNameSyntax identifier }
            ? model.GetSymbolInfo(identifier).Symbol as IParameterSymbol : null;
        computedLookups.Add(new ComputedLookup(receiver, EnclosingLiterals(invocation), nameParameter, site, owner));
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
                        graph.Edge(lookup.Owner, identity.Key(member));
            }
        }
    }

    /// <summary>Deferred reflection call with candidate strings from its containing body or callers.</summary>
    private sealed record ComputedLookup(INamedTypeSymbol? Receiver, HashSet<string> Literals, IParameterSymbol? NameParameter,
        ReflectionSite Site, string Owner);

    /// <summary>Dynamic lookups awaiting constants gathered from all repository call sites.</summary>
    private readonly List<ComputedLookup> computedLookups = [];
    /// <summary>Constant strings passed to repository helper parameters that may feed reflection names.</summary>
    private readonly HashSet<(string Parameter, string Value)> callerConstants = [];

    /// <summary>Identifies a helper parameter by original method signature and ordinal across call sites.</summary>
    private static string ParameterKey(IParameterSymbol parameter) =>
        $"{parameter.ContainingSymbol.OriginalDefinition.ToDisplayString()}#{parameter.Ordinal}";

    /// <summary>Walks a type and its bases, selecting the member kinds exposed by one Type enumeration API.</summary>
    private static IEnumerable<ISymbol> EnumeratedMembers(INamedTypeSymbol type, string enumeration)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
            foreach (var member in current.GetMembers().Where(m => !m.IsImplicitlyDeclared))
                if (enumeration switch
                    {
                        "GetMethods" => member is IMethodSymbol { MethodKind: MethodKind.Ordinary },
                        "GetProperties" => member is IPropertySymbol,
                        "GetFields" => member is IFieldSymbol,
                        "GetConstructors" => member is IMethodSymbol { MethodKind: MethodKind.Constructor },
                        "GetEvents" => member is IEventSymbol,
                        "GetNestedTypes" => member is INamedTypeSymbol,
                        _ => true,
                    })
                    yield return member;
    }

    /// <summary>Finds explicit members with a reflection name on the receiver and its base types.</summary>
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

    /// <summary>Collects identifier-shaped string literals from the lookup's nearest method, accessor, or property body.</summary>
    private static HashSet<string> EnclosingLiterals(SyntaxNode node)
    {
        var scope = node.Ancestors().FirstOrDefault(a => a is BaseMethodDeclarationSyntax or AccessorDeclarationSyntax
            or PropertyDeclarationSyntax or GlobalStatementSyntax) ?? node;
        return scope.DescendantNodes().OfType<LiteralExpressionSyntax>()
            .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression) && SyntaxFacts.IsValidIdentifier(l.Token.ValueText))
            .Select(l => l.Token.ValueText).ToHashSet();
    }

    /// <summary>Captures a repository-relative path, one-based line, and compact source text for a lookup.</summary>
    private ReflectionSite Site(SyntaxNode node) => new(identity.Relative(node.SyntaxTree.FilePath),
        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
        string.Join(' ', node.ToString().Split(['\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())));
}
