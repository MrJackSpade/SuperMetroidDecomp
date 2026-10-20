using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>
/// Finds the members System.Text.Json reads or binds by reflection. Serializer access is not use:
/// a member reached only this way is reported as serialization-only, never counted reachable.
/// </summary>
internal sealed class JsonSerializationScan(SymbolIdentity identity)
{
    /// <summary>Repository payload types discovered from serializer calls and nested public properties.</summary>
    private readonly Dictionary<string, INamedTypeSymbol> payloadTypes = [];
    /// <summary>Repository helper type parameters whose concrete payloads depend on call sites.</summary>
    private readonly HashSet<(IMethodSymbol Helper, int Ordinal)> genericHelpers = new(HelperComparer.Instance);
    /// <summary>Observed generic calls used to resolve serializer payloads passed through helpers.</summary>
    private readonly List<IMethodSymbol> genericInvocations = [];

    /// <summary>Records JSON payload types from serializer arguments and generic repository helper calls.</summary>
    public void ObserveInvocation(SemanticModel model, InvocationExpressionSyntax invocation, IMethodSymbol method)
    {
        if (method.IsGenericMethod && IsRepositorySymbol(method.OriginalDefinition))
            genericInvocations.Add(method);
        if (method.ContainingType.Name != "JsonSerializer" || method.ContainingNamespace.ToDisplayString() != "System.Text.Json")
            return;
        foreach (var typeArgument in method.TypeArguments)
            AddPayload(typeArgument);
        if (method.Name.StartsWith("Serialize", StringComparison.Ordinal) && invocation.ArgumentList.Arguments.Count > 0)
            AddPayload(model.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression).Type);
        foreach (var argument in invocation.ArgumentList.Arguments)
            if (argument.Expression is TypeOfExpressionSyntax typeOf)
                AddPayload(model.GetTypeInfo(typeOf.Type).Type);
    }

    /// <summary>Keys of every property and binding constructor the serializer accesses.</summary>
    public HashSet<string> SerializerAccessedMembers()
    {
        ResolveGenericHelpers();
        var accessed = new HashSet<string>();
        var expanded = new HashSet<string>();
        var pending = new Queue<string>(payloadTypes.Keys);
        while (pending.Count > 0)
        {
            string key = pending.Dequeue();
            if (!expanded.Add(key))
                continue;
            var type = payloadTypes[key];
            foreach (var property in type.GetMembers().OfType<IPropertySymbol>()
                         .Where(p => p is { IsStatic: false, IsIndexer: false, DeclaredAccessibility: Accessibility.Public }))
            {
                if (identity.Key(property) is { } propertyKey)
                    accessed.Add(propertyKey);
                AddPayload(property.Type);
            }
            // Deserialization binds through the single public constructor when one is declared.
            var constructors = type.InstanceConstructors
                .Where(c => !c.IsImplicitlyDeclared && c.DeclaredAccessibility == Accessibility.Public).ToList();
            if (constructors is [var binding] && identity.Key(binding) is { } constructorKey)
                accessed.Add(constructorKey);
            foreach (string added in payloadTypes.Keys.Where(k => !expanded.Contains(k)))
                pending.Enqueue(added);
        }
        return accessed;
    }

    /// <summary>Unwraps arrays and generic arguments, tracking repository types and unresolved helper parameters.</summary>
    private void AddPayload(ITypeSymbol? type)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                AddPayload(array.ElementType);
                break;
            case ITypeParameterSymbol { ContainingSymbol: IMethodSymbol helper } parameter when IsRepositorySymbol(helper):
                // Serialized through a generic helper: resolved at each of its call sites.
                genericHelpers.Add((helper.OriginalDefinition, parameter.Ordinal));
                break;
            case ITypeParameterSymbol parameter:
                throw new InvalidOperationException(
                    $"Cannot resolve the JSON payload type parameter {parameter.Name} of {parameter.ContainingSymbol}.");
            case INamedTypeSymbol named:
                foreach (var argument in named.TypeArguments)
                    AddPayload(argument);
                if (identity.Key(named) is { } key)
                    payloadTypes.TryAdd(key, named.OriginalDefinition);
                break;
            default:
                break;
        }
    }

    /// <summary>Substitutes observed call-site type arguments into serializer helper parameters until stable.</summary>
    private void ResolveGenericHelpers()
    {
        var resolved = new HashSet<(IMethodSymbol, int)>(HelperComparer.Instance);
        while (genericHelpers.Where(h => !resolved.Contains(h)).ToList() is { Count: > 0 } pending)
            foreach (var (helper, ordinal) in pending)
            {
                resolved.Add((helper, ordinal));
                foreach (var call in genericInvocations.Where(c => SymbolEqualityComparer.Default.Equals(c.OriginalDefinition, helper)))
                    AddPayload(call.TypeArguments[ordinal]);
            }
    }

    /// <summary>Limits generic-helper tracing to symbols declared in repository source.</summary>
    private static bool IsRepositorySymbol(ISymbol symbol) => symbol.Locations.Any(l => l.IsInSource);

    /// <summary>Compares helper type-parameter slots by Roslyn symbol identity and ordinal.</summary>
    private sealed class HelperComparer : IEqualityComparer<(IMethodSymbol Helper, int Ordinal)>
    {
        /// <summary>Shared comparer for helper slots collected during payload inference.</summary>
        public static readonly HelperComparer Instance = new();

        /// <summary>Matches the same generic helper's parameter at the same ordinal.</summary>
        public bool Equals((IMethodSymbol Helper, int Ordinal) x, (IMethodSymbol Helper, int Ordinal) y) =>
            SymbolEqualityComparer.Default.Equals(x.Helper, y.Helper) && x.Ordinal == y.Ordinal;

        /// <summary>Hashes a helper slot consistently with Roslyn symbol equality.</summary>
        public int GetHashCode((IMethodSymbol Helper, int Ordinal) value) =>
            HashCode.Combine(SymbolEqualityComparer.Default.GetHashCode(value.Helper), value.Ordinal);
    }
}
