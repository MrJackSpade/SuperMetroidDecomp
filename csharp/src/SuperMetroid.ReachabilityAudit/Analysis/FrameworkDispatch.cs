using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>
/// Keys that pair a call through a framework interface member (an <c>IReadOnlyList&lt;T&gt;</c> indexer, an
/// <c>IEnumerable&lt;T&gt;.GetEnumerator</c>, an <c>IEquatable&lt;T&gt;.Equals</c>) with the repository members implementing
/// it. A call through a closed contract (<c>IReadOnlyList&lt;RgbColor&gt;</c>) pairs with implementations of that same
/// constructed contract and with implementations whose contract is still open (<c>IReadOnlyList&lt;T&gt;</c> in a generic
/// type); a call through an open contract, from generic code, pairs with every implementation of the contract.
/// </summary>
internal static class FrameworkDispatch
{
    /// <summary>Implementations of exactly this constructed contract member.</summary>
    private const string ClosedPrefix = "DISPATCH:";
    /// <summary>Implementations whose contract still names a type parameter of their generic type.</summary>
    private const string OpenPrefix = "DISPATCH-OPEN:";
    /// <summary>Every implementation of the contract member, whatever its type arguments.</summary>
    private const string AnyPrefix = "DISPATCH-ANY:";

    /// <summary>A member written with its parameter types, so overloads stay distinct.</summary>
    private static readonly SymbolDisplayFormat MemberFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    /// <summary>
    /// The dispatch keys a reference to <paramref name="target"/> calls through, or none when it is not a member of a
    /// framework interface.
    /// </summary>
    public static IEnumerable<string> CallKeys(ISymbol? target)
    {
        if (Contract(target) is not { } member)
            return [];
        return ContainsTypeParameter(member.ContainingType)
            ? [AnyPrefix + Display(member.OriginalDefinition)]
            : [ClosedPrefix + Display(member), OpenPrefix + Display(member.OriginalDefinition)];
    }

    /// <summary>The dispatch keys under which an implementation of the framework contract member is found.</summary>
    public static IEnumerable<string> ImplementationKeys(ISymbol contractMember) =>
        ContainsTypeParameter(contractMember.ContainingType)
            ? [OpenPrefix + Display(contractMember.OriginalDefinition), AnyPrefix + Display(contractMember.OriginalDefinition)]
            : [ClosedPrefix + Display(contractMember), AnyPrefix + Display(contractMember.OriginalDefinition)];

    /// <summary>
    /// The framework interface member a reference calls, with accessors standing for their property or event and a generic
    /// method standing for its unconstructed form, or null when the reference is to anything else.
    /// </summary>
    private static ISymbol? Contract(ISymbol? target)
    {
        ISymbol? member = target switch
        {
            IMethodSymbol { AssociatedSymbol: { } associated } => associated,
            IMethodSymbol { IsGenericMethod: true } method => method.ConstructedFrom,
            IMethodSymbol or IPropertySymbol or IEventSymbol => target,
            _ => null,
        };
        return member is { ContainingType: { TypeKind: TypeKind.Interface } contract } && !contract.Locations.Any(l => l.IsInSource)
            ? member
            : null;
    }

    /// <summary>The member qualified by its (possibly constructed) interface.</summary>
    private static string Display(ISymbol member) =>
        $"{member.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}::{member.ToDisplayString(MemberFormat)}";

    /// <summary>Whether a type, or any of its type arguments or element types, is a type parameter.</summary>
    private static bool ContainsTypeParameter(ITypeSymbol type) => type switch
    {
        ITypeParameterSymbol => true,
        IArrayTypeSymbol array => ContainsTypeParameter(array.ElementType),
        IPointerTypeSymbol pointer => ContainsTypeParameter(pointer.PointedAtType),
        INamedTypeSymbol named => named.TypeArguments.Any(ContainsTypeParameter),
        _ => false,
    };
}
