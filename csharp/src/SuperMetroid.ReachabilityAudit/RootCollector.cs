using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>
/// Entry points and the reachability the runtime adds without source references: framework
/// instantiation, virtual and interface dispatch, interop layout and reflection by name.
/// </summary>
internal sealed class RootCollector(SymbolIdentity identity, ReachabilityGraph graph)
{
    /// <summary>Marks a member or type that only reflection reaches (SuperMetroid.Core.AccessedByReflectionAttribute).</summary>
    public const string ReflectionMarker = "AccessedByReflectionAttribute";

    private static readonly HashSet<string> FrameworkCreatedAttributes =
        ["ActivityAttribute", "ApplicationAttribute", "ServiceAttribute", "BroadcastReceiverAttribute",
         "ContentProviderAttribute", "DiagnosticAnalyzerAttribute"];

    /// <summary>Roots of every repository executable, tools and verifiers included.</summary>
    public HashSet<string> Roots { get; } = [];

    /// <summary>Roots of the shipped player hosts and the build-time analyzer only.</summary>
    public HashSet<string> ProductionRoots { get; } = [];

    public static bool HasReflectionMarker(ISymbol symbol) =>
        symbol.GetAttributes().Any(a => a.AttributeClass?.Name == ReflectionMarker);

    public void Collect(Project project, Compilation compilation, bool production)
    {
        if (compilation.GetEntryPoint(CancellationToken.None) is { } entry)
            AddRoot(EntryPointKey(entry, project.Name), production);
        foreach (var type in AllTypes(compilation.Assembly.GlobalNamespace))
        {
            if (identity.Key(type) is not { } typeKey)
                continue;
            if (type.GetAttributes().Any(a => a.AttributeClass is { } attribute && FrameworkCreatedAttributes.Contains(attribute.Name)))
            {
                AddRoot(typeKey, production);
                foreach (var constructor in type.InstanceConstructors)
                    graph.ImpliedByType(typeKey, identity.Key(constructor));
            }
            foreach (var staticConstructor in type.StaticConstructors)
                graph.ImpliedByType(typeKey, identity.Key(staticConstructor));
            // An explicit layout, or an inline array's element field, is storage the type's
            // users read through memory rather than by name.
            if (type.GetAttributes().Any(a => a.AttributeClass?.Name is "StructLayoutAttribute" or "InlineArrayAttribute"))
                foreach (var field in type.GetMembers().OfType<IFieldSymbol>())
                    graph.ImpliedByType(typeKey, identity.Key(field));
            if (HasReflectionMarker(type))
                Roots.Add(typeKey);
            foreach (var member in type.GetMembers().Where(HasReflectionMarker))
                if (identity.Key(member) is { } memberKey)
                    Roots.Add(memberKey);
            CollectDispatch(type, typeKey);
        }
    }

    /// <summary>Top-level statements compile to a synthesized entry point that has no repository key.</summary>
    public string EntryPointKey(IMethodSymbol entry, string projectName) => identity.Key(entry) ?? $"ENTRY:{projectName}";

    private void AddRoot(string key, bool production)
    {
        Roots.Add(key);
        if (production)
            ProductionRoots.Add(key);
    }

    private void CollectDispatch(INamedTypeSymbol type, string typeKey)
    {
        foreach (var member in type.GetMembers())
        {
            ISymbol? overridden = member switch
            {
                IMethodSymbol method => method.OverriddenMethod,
                IPropertySymbol property => property.OverriddenProperty,
                IEventSymbol @event => @event.OverriddenEvent,
                _ => null,
            };
            if (overridden is null)
                continue;
            // A repository base member dispatches to its overrides; a framework member is
            // called by the framework whenever the overriding type exists.
            if (InSource(overridden))
                graph.Edge(identity.Key(overridden), identity.Key(member));
            else
                graph.ImpliedByType(typeKey, identity.Key(member));
        }
        foreach (var contract in type.AllInterfaces)
            foreach (var contractMember in contract.GetMembers())
            {
                if (contractMember is not (IMethodSymbol or IPropertySymbol or IEventSymbol)
                    || type.FindImplementationForInterfaceMember(contractMember) is not { } implementation
                    || !InSource(implementation))
                    continue;
                if (InSource(contractMember))
                    graph.Edge(identity.Key(contractMember), identity.Key(implementation));
                else
                    graph.ImpliedByType(typeKey, identity.Key(implementation));
            }
    }

    private static bool InSource(ISymbol symbol) => symbol.Locations.Any(l => l.IsInSource);

    public static IEnumerable<INamedTypeSymbol> AllTypes(INamespaceSymbol ns)
    {
        foreach (var member in ns.GetMembers())
        {
            var types = member is INamespaceSymbol child ? AllTypes(child) : WithNested((INamedTypeSymbol)member);
            foreach (var type in types)
                yield return type;
        }
    }

    private static IEnumerable<INamedTypeSymbol> WithNested(INamedTypeSymbol type)
    {
        yield return type;
        foreach (var nested in type.GetTypeMembers())
            foreach (var descendant in WithNested(nested))
                yield return descendant;
    }
}
