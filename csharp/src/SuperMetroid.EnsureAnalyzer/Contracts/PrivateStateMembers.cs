using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// The member a check's private-state call names (SME6477, #2570), looked up as the reflection behind the call finds it.
/// The SME6477 analyzer judges a build's calls with it, and the architecture audit, which binds checks
/// against the product's source, every call.
/// </summary>
public static class PrivateStateMembers
{
    /// <summary>The calls that name a field, by the parameter that takes its name.</summary>
    private static readonly ImmutableHashSet<string> FieldCalls = ImmutableHashSet.Create(StringComparer.Ordinal,
        "Field", "SetField", "ValueField", "SetValueField", "StaticField", "SetStaticField");

    /// <summary>The calls that name a property.</summary>
    private static readonly ImmutableHashSet<string> PropertyCalls = ImmutableHashSet.Create(StringComparer.Ordinal,
        "Get", "Set", "ValueGet", "ValueSet", "With", "StaticGet", "StaticSet", "GetterDelegate");

    /// <summary>The calls that name a method.</summary>
    private static readonly ImmutableHashSet<string> MethodCalls = ImmutableHashSet.Create(StringComparer.Ordinal,
        "Call", "Invoke", "InvokeValue", "InvokeStatic", "InvokeGeneric", "InvokeStaticGeneric", "MethodDelegate");

    /// <summary>
    /// The member a private-state call names that its owner, as <paramref name="invocation"/>'s compilation sees it, does
    /// not have: the name's site, the call, the kind and name of the member, and the owner; null for any other invocation.
    /// </summary>
    public static (SyntaxNode Site, string Call, string Kind, string Name, INamedTypeSymbol Owner)? MissingMember(IInvocationOperation invocation)
    {
        IMethodSymbol method = Required(invocation).TargetMethod;
        if (method.ContainingType is not { Name: "PrivateState", ContainingType: null } state ||
            state.ContainingNamespace?.ToDisplayString() != "SuperMetroid.Core")
            return null;
        string kind = FieldCalls.Contains(method.Name) ? "field"
            : PropertyCalls.Contains(method.Name) ? "property"
            : MethodCalls.Contains(method.Name) ? "method"
            : "";
        if (kind.Length == 0 || Owner(invocation) is not { TypeKind: not TypeKind.Error } owner ||
            invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name is "field" or "property" or "method") is not { } named ||
            named.Value.ConstantValue is not { HasValue: true, Value: string name } ||
            Has(owner, kind, name))
            return null;
        return (named.Value.Syntax, invocation.Syntax.ToString(), kind, name, owner);
    }

    /// <summary>The invocation, which must be given.</summary>
    private static IInvocationOperation Required(IInvocationOperation? invocation) =>
        invocation ?? throw new ArgumentNullException(nameof(invocation));

    /// <summary>The owner a call states: the <c>typeof</c> its <c>owner</c> argument passes, or its first type argument.</summary>
    private static INamedTypeSymbol? Owner(IInvocationOperation invocation)
    {
        IArgumentOperation? owner = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "owner");
        if (owner?.Parameter?.Type is { Name: "Type", ContainingNamespace.Name: "System" })
            return owner.Value is ITypeOfOperation { TypeOperand: INamedTypeSymbol stated } ? stated : null;
        return invocation.TargetMethod.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
    }

    /// <summary>
    /// Whether the owner has a member of the kind and name, as reflection over its declared and inherited members finds
    /// it: any member the owner declares, or a non-private instance member of a base type. A method is an ordinary method
    /// or a property's or an event's accessor; a static class's extension blocks compile to methods of the class itself, a method of a
    /// block taking its own name and a property's accessors theirs.
    /// </summary>
    private static bool Has(INamedTypeSymbol owner, string kind, string name)
    {
        for (INamedTypeSymbol? type = owner; type is not null; type = type.BaseType)
        {
            bool own = SymbolEqualityComparer.Default.Equals(type, owner);
            var members = type.GetMembers(name).AsEnumerable();
            if (own && kind == "method")
                members = members.Concat(type.GetTypeMembers().Where(block => block.TypeKind == TypeKind.Extension)
                    .SelectMany(block => block.GetMembers(name)));
            foreach (ISymbol member in members)
            {
                if (!own && (member.IsStatic || member.DeclaredAccessibility == Accessibility.Private))
                    continue;
                bool matches = kind switch
                {
                    "field" => member is IFieldSymbol { IsImplicitlyDeclared: false },
                    "property" => member is IPropertySymbol { IsIndexer: false },
                    _ => member is IMethodSymbol
                    {
                        MethodKind: MethodKind.Ordinary or MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove,
                    },
                };
                if (matches)
                    return true;
            }
        }
        return false;
    }
}
