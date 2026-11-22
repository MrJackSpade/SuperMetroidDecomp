using Microsoft.CodeAnalysis;
using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>
/// Entry points and the reachability the runtime adds without source references: framework
/// instantiation, virtual and interface dispatch, interop layout and reflection by name.
/// </summary>
internal sealed class RootCollector(SymbolIdentity identity, ReachabilityGraph graph)
{
    /// <summary>Marks a member or type that only reflection reaches (SuperMetroid.Core.AccessedByReflectionAttribute).</summary>
    public const string ReflectionMarker = "AccessedByReflectionAttribute";

    /// <summary>
    /// Marker attributes whose framework-constructed types require implicit roots: the compiler constructs every analyzer,
    /// code fix provider and source generator (<c>[Generator]</c>) it is handed, which no source names (#2614).
    /// </summary>
    private static readonly HashSet<string> FrameworkCreatedAttributes =
        ["ActivityAttribute", "ApplicationAttribute", "ServiceAttribute", "BroadcastReceiverAttribute",
         "ContentProviderAttribute", "DiagnosticAnalyzerAttribute", "ExportCodeFixProviderAttribute", "GeneratorAttribute"];

    /// <summary>Roots of every repository executable, tools and verifiers included.</summary>
    public HashSet<string> Roots { get; } = [];

    /// <summary>Roots of the shipped player hosts and the build-time analyzer only.</summary>
    public HashSet<string> ProductionRoots { get; } = [];

    /// <summary>Recognizes explicit repository annotations for symbol-name reflection.</summary>
    public static bool HasReflectionMarker(ISymbol symbol) =>
        symbol.GetAttributes().Any(a => a.AttributeClass?.Name == ReflectionMarker);

    /// <summary>Roots entry points, framework types, layout fields, reflection markers, and dispatch edges.</summary>
    public void Collect(Project project, Compilation compilation, bool production)
    {
        if (compilation.GetEntryPoint(CancellationToken.None) is { } entry)
            AddRoot(EntryPointKey(entry, project.Name), production);
        // The compiler references an owned IsExternalInit polyfill through every init accessor's modreq; no source names it.
        if (compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.IsExternalInit") is { } isExternalInit &&
            SymbolEqualityComparer.Default.Equals(isExternalInit.ContainingAssembly, compilation.Assembly) &&
            identity.Key(isExternalInit) is { } isExternalInitKey &&
            AllTypes(compilation.Assembly.GlobalNamespace).Any(type => type.GetMembers().OfType<IMethodSymbol>().Any(method => method.IsInitOnly)))
            AddRoot(isExternalInitKey, production);
        CollectAssemblyAttributes(project, compilation, production);
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
            graph.Constructors(typeKey, type.InstanceConstructors.Select(identity.Key).OfType<string>());
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

    /// <summary>
    /// An attribute applied to the assembly or its module is part of the assembly whenever it is built, which no
    /// declaration reaches: its class, constructor and the members its named arguments set are roots (#2614: the
    /// <c>[assembly: StateGraphAssembly]</c> each marked assembly carries), production ones in a shipped assembly.
    /// </summary>
    private void CollectAssemblyAttributes(Project project, Compilation compilation, bool production)
    {
        bool shipped = production || Reporting.ReachabilityFindings.ProductionAssemblies.Contains(project.Name);
        foreach (AttributeData attribute in compilation.Assembly.GetAttributes().Concat(compilation.SourceModule.GetAttributes()))
        {
            if (attribute.ApplicationSyntaxReference?.SyntaxTree is not { } tree || !(identity.IsRepositorySource(tree) || identity.IsGenerated(tree)))
                continue;
            IEnumerable<ISymbol?> used = [attribute.AttributeClass, attribute.AttributeConstructor,
                .. attribute.NamedArguments.Select(argument => attribute.AttributeClass?.GetMembers(argument.Key).FirstOrDefault())];
            foreach (ISymbol? symbol in used)
                if (identity.Key(symbol) is { } key)
                    AddRoot(key, shipped);
        }
    }

    /// <summary>Top-level statements compile to a synthesized entry point that has no repository key.</summary>
    public string EntryPointKey(IMethodSymbol entry, string projectName) => identity.Key(entry) ?? $"ENTRY:{projectName}";

    /// <summary>Adds a general root and, for shipped code, a production root.</summary>
    private void AddRoot(string key, bool production)
    {
        Roots.Add(key);
        if (production)
            ProductionRoots.Add(key);
    }

    /// <summary>Connects base and interface calls to implementations, including framework callbacks.</summary>
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
        IReadOnlyList<string>? instantiation = null;
        foreach (var contract in type.AllInterfaces)
            foreach (var contractMember in contract.GetMembers())
            {
                if (contractMember is not (IMethodSymbol or IPropertySymbol or IEventSymbol)
                    || type.FindImplementationForInterfaceMember(contractMember) is not { } implementation
                    || !InSource(implementation))
                    continue;
                if (InSource(contractMember))
                {
                    graph.Edge(identity.Key(contractMember), identity.Key(implementation));
                    continue;
                }
                graph.ImpliedByType(typeKey, identity.Key(implementation));
                instantiation ??= InstantiationKeys(type, typeKey);
                foreach (string dispatchKey in FrameworkDispatch.ImplementationKeys(contractMember))
                    graph.DispatchImplementation(dispatchKey, identity.Key(implementation), instantiation);
            }
    }

    /// <summary>
    /// The keys whose reachability shows <paramref name="type"/> constructed: each instance constructor, compiler-declared
    /// ones included, and for a value type the type itself, since its default value exists without a constructor call.
    /// </summary>
    private List<string> InstantiationKeys(INamedTypeSymbol type, string typeKey)
    {
        var keys = type.InstanceConstructors.Select(identity.Key).OfType<string>().ToList();
        if (type.IsValueType)
            keys.Add(typeKey);
        return keys;
    }

    /// <summary>Distinguishes repository declarations from external framework contracts.</summary>
    private static bool InSource(ISymbol symbol) => symbol.Locations.Any(l => l.IsInSource);

    /// <summary>Visits every namespace type, including nested declarations.</summary>
    public static IEnumerable<INamedTypeSymbol> AllTypes(INamespaceSymbol ns)
    {
        foreach (var member in ns.GetMembers())
        {
            var types = member is INamespaceSymbol child ? AllTypes(child) : WithNested((INamedTypeSymbol)member);
            foreach (var type in types)
                yield return type;
        }
    }

    /// <summary>Yields a type before recursively visiting its nested types.</summary>
    private static IEnumerable<INamedTypeSymbol> WithNested(INamedTypeSymbol type)
    {
        yield return type;
        foreach (var nested in type.GetTypeMembers())
            foreach (var descendant in WithNested(nested))
                yield return descendant;
    }
}
