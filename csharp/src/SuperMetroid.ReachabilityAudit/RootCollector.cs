using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>
/// Entry points and the reachability the runtime adds without source references: framework
/// instantiation, virtual and interface dispatch, interop layout and reflection by name.
/// </summary>
/// <param name="identity">Repository symbol identity service used to assign stable root keys.</param>
/// <param name="graph">Reachability graph that receives root and dispatch edges.</param>
internal sealed class RootCollector(SymbolIdentity identity, ReachabilityGraph graph)
{
    /// <summary>Marks a member or type that only reflection reaches (SuperMetroid.Core.AccessedByReflectionAttribute).</summary>
    public const string ReflectionMarker = "AccessedByReflectionAttribute";

    /// <summary>Attribute names whose framework or host machinery creates annotated types.</summary>
    private static readonly HashSet<string> FrameworkCreatedAttributes =
        ["ActivityAttribute", "ApplicationAttribute", "ServiceAttribute", "BroadcastReceiverAttribute",
         "ContentProviderAttribute", "DiagnosticAnalyzerAttribute"];

    /// <summary>Roots of every repository executable, tools and verifiers included.</summary>
    public HashSet<string> Roots { get; } = [];

    /// <summary>Roots of the shipped player hosts and the build-time analyzer only.</summary>
    public HashSet<string> ProductionRoots { get; } = [];

    /// <summary>Whether the symbol carries the repository attribute that declares name-based reflection access.</summary>
    public static bool HasReflectionMarker(ISymbol symbol) =>
        symbol.GetAttributes().Any(a => a.AttributeClass?.Name == ReflectionMarker);

    /// <summary>Adds executable roots and members reached through framework dispatch for one compilation.</summary>
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
            if (type.GetAttributes().Any(a => a.AttributeClass?.Name == "StructLayoutAttribute"))
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

    /// <summary>Records a root for all-tool reachability and, when applicable, production reachability.</summary>
    private void AddRoot(string key, bool production)
    {
        Roots.Add(key);
        if (production)
            ProductionRoots.Add(key);
    }

    /// <summary>Adds override and interface implementation edges, including framework-triggered dispatch.</summary>
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

    /// <summary>Checks whether a symbol has at least one source declaration.</summary>
    private static bool InSource(ISymbol symbol) => symbol.Locations.Any(l => l.IsInSource);

    /// <summary>Enumerates namespace types recursively, including nested types.</summary>
    public static IEnumerable<INamedTypeSymbol> AllTypes(INamespaceSymbol ns)
    {
        foreach (var member in ns.GetMembers())
        {
            var types = member is INamespaceSymbol child ? AllTypes(child) : WithNested((INamedTypeSymbol)member);
            foreach (var type in types)
                yield return type;
        }
    }

    /// <summary>Enumerates a type followed by all of its nested types.</summary>
    private static IEnumerable<INamedTypeSymbol> WithNested(INamedTypeSymbol type)
    {
        yield return type;
        foreach (var nested in type.GetTypeMembers())
            foreach (var descendant in WithNested(nested))
                yield return descendant;
    }
}
