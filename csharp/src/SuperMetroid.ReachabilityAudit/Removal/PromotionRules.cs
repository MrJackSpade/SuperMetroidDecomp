using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// The rules a staying member breaks when <see cref="SymbolRelocator"/> makes it internal for moved code. Such a promotion
/// is refused, and the moved code reaching the member stays for manual relocation with a <c>MANUAL</c> line saying why,
/// before any edit is made. A promotion changes only a declaration's accessibility (and, for an underscore-prefixed field
/// or property, its name: <see cref="PromotedNames"/>), so these are the rules the build enforces that look at
/// accessibility:
/// <list type="bullet">
/// <item>SME6436 reports a non-private mutable instance field of a class: other types would write it directly. A private
/// one moved code uses cannot be made internal; the class needs a named operation or a property for it.</item>
/// <item>SME6201 asks a non-private method or accessor to check its arguments with Ensure. A private one whose body checks
/// an argument by hand (an <c>if</c> that throws an argument exception naming the parameter) cannot be made internal
/// until the check uses Ensure.</item>
/// <item>CS0108, a build error here, reports a member hiding an inherited one it can see. A member of a type deriving from
/// the owner that takes the promoted member's name, without <c>new</c>, would hide it once it is internal.</item>
/// </list>
/// SME6354 (an underscore-prefixed field or property is private) is met by renaming, not refused. Every other rule the
/// build enforces reads what a declaration is, says or does, which a promotion does not change.
/// </summary>
internal static class PromotionRules
{
    /// <summary>The argument exceptions SME6201 asks Ensure to throw.</summary>
    private static readonly HashSet<string> ArgumentExceptions =
        ["System.ArgumentException", "System.ArgumentNullException", "System.ArgumentOutOfRangeException"];

    /// <summary>Why making each refused promoted member internal, under the name it takes, would break one of the rules.</summary>
    /// <param name="promoted">The staying members that become internal, every field sharing a declaration with one included.</param>
    /// <param name="setters">The staying properties whose private or protected setter becomes internal.</param>
    /// <param name="nameOf">The name each promoted member has once promoted.</param>
    /// <param name="types">The repository types and their derived types.</param>
    /// <param name="modelOf">The semantic model of a repository tree.</param>
    /// <param name="identity">Says which trees are repository source.</param>
    /// <returns>The reason for each refused member; a refused setter is the property's.</returns>
    public static Dictionary<ISymbol, string> Refusals(IReadOnlyCollection<ISymbol> promoted, IReadOnlyCollection<IPropertySymbol> setters,
        Func<ISymbol, string> nameOf, SourceTypes types, Func<SyntaxTree, SemanticModel> modelOf, SymbolIdentity identity)
    {
        var promotedSet = promoted.ToHashSet(SymbolEqualityComparer.Default);
        var refusals = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);
        foreach (var symbol in promoted.OrderBy(symbol => symbol.ToDisplayString(), StringComparer.Ordinal))
        {
            string? refusal = ExposedField(symbol, promotedSet);
            if (symbol.DeclaredAccessibility == Accessibility.Private)
                foreach (var declaration in RelocationSymbols.DeclarationsOf(identity, symbol))
                    refusal ??= ArgumentGuard(symbol, declaration, modelOf);
            refusal ??= Hiding(symbol, nameOf(symbol), types);
            if (refusal is not null)
                refusals[symbol] = refusal;
        }
        foreach (var property in setters.OrderBy(property => property.ToDisplayString(), StringComparer.Ordinal))
            if (!refusals.ContainsKey(property) && property.SetMethod is { DeclaredAccessibility: Accessibility.Private } setter)
                foreach (var declaration in RelocationSymbols.DeclarationsOf(identity, setter))
                    if (ArgumentGuard(setter, declaration, modelOf) is { } refusal)
                        refusals.TryAdd(property, refusal);
        return refusals;
    }

    /// <summary>Why the member stays private: it is a private mutable instance field of a class (SME6436); or null.</summary>
    private static string? ExposedField(ISymbol symbol, HashSet<ISymbol> promoted)
    {
        if (symbol is not IFieldSymbol { IsStatic: false, IsConst: false, IsReadOnly: false, IsImplicitlyDeclared: false,
                DeclaredAccessibility: Accessibility.Private, ContainingType.TypeKind: TypeKind.Class } field)
            return null;
        // A field of a type nested privately is reached only by the type declaring it, unless that type becomes internal too.
        for (var type = field.ContainingType; type is not null; type = type.ContainingType)
            if (type.DeclaredAccessibility == Accessibility.Private && !promoted.Contains(type))
                return null;
        return $"{field.ToDisplayString()}, a mutable field of the class {field.ContainingType.ToDisplayString()}, " +
            "which made internal other types could write directly (SME6436): give the class a named operation or a property " +
            "for what this code needs";
    }

    /// <summary>Why a private method or accessor stays private: its body checks an argument by hand (SME6201); or null.</summary>
    private static string? ArgumentGuard(ISymbol symbol, SyntaxNode declaration, Func<SyntaxTree, SemanticModel> modelOf)
    {
        // A check inside a lambda or local function belongs to that function, which SME6201 judges on its own.
        foreach (var check in declaration.DescendantNodes(node => node is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax))
                     .OfType<IfStatementSyntax>())
        {
            if (check.Else is not null)
                continue;
            var statement = check.Statement is BlockSyntax { Statements: [var only] } ? only : check.Statement;
            if (statement is not ThrowStatementSyntax { Expression: ObjectCreationExpressionSyntax { ArgumentList: { } arguments } creation } ||
                !arguments.Arguments.Any(argument => argument.Expression is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } }) ||
                modelOf(check.SyntaxTree).GetTypeInfo(creation).Type?.ToDisplayString() is not { } thrown || !ArgumentExceptions.Contains(thrown))
                continue;
            return $"{symbol.ToDisplayString()}, which checks an argument by hand ({check.Condition}), a check " +
                "that made internal must use Ensure (SME6201): write it with Ensure";
        }
        return null;
    }

    /// <summary>
    /// Why the member stays private: a type deriving from its owner declares a member of the name it takes without
    /// <c>new</c>, which would hide it once it is internal (CS0108); or null. A method of other parameters overloads it instead.
    /// </summary>
    private static string? Hiding(ISymbol symbol, string name, SourceTypes types)
    {
        // A constructor is not inherited, so a derived type's constructors hide nothing.
        if (symbol.ContainingType is not { } owner || symbol is IMethodSymbol { MethodKind: not MethodKind.Ordinary })
            return null;
        foreach (var derived in types.DerivedFrom(owner))
            foreach (var member in derived.GetMembers(name).Where(member => !member.IsImplicitlyDeclared))
            {
                if (symbol is IMethodSymbol method && member is IMethodSymbol other && !SameParameters(method, other) || Hides(member))
                    continue;
                return $"{symbol.ToDisplayString()}, whose name '{name}' {member.ToDisplayString()} takes in a type " +
                    $"deriving from {owner.ToDisplayString()}, which made internal it would hide (CS0108): rename one of them";
            }
        return null;
    }

    /// <summary>Whether two methods take the same parameter types and kinds.</summary>
    private static bool SameParameters(IMethodSymbol first, IMethodSymbol second) =>
        first.Parameters.Length == second.Parameters.Length && first.Parameters.Zip(second.Parameters).All(pair =>
            pair.First.RefKind == pair.Second.RefKind && SymbolEqualityComparer.Default.Equals(pair.First.Type, pair.Second.Type));

    /// <summary>Whether a member is declared <c>new</c>, so that hiding is intended.</summary>
    private static bool Hides(ISymbol member) => member.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax())
        .Select(syntax => syntax is VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax field } ? field : syntax)
        .OfType<MemberDeclarationSyntax>().Any(declaration => declaration.Modifiers.Any(SyntaxKind.NewKeyword));
}
