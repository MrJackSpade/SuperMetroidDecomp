using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// The rules a staying member breaks when <see cref="SymbolRelocator"/> makes it internal for moved code, each of which
/// fails the run rather than being written. A promotion changes only a declaration's accessibility (and, for an
/// underscore-prefixed field or property, its name: <see cref="PromotedNames"/>), so these are the rules the build enforces
/// that look at accessibility:
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

    /// <summary>Fails when making each promoted member internal, under the name it takes, would break one of the rules.</summary>
    /// <param name="promoted">The staying members that become internal, every field sharing a declaration with one included.</param>
    /// <param name="setters">The staying properties whose private or protected setter becomes internal.</param>
    /// <param name="nameOf">The name each promoted member has once promoted.</param>
    /// <param name="types">The repository types and their derived types.</param>
    /// <param name="modelOf">The semantic model of a repository tree.</param>
    /// <param name="identity">Says which trees are repository source.</param>
    /// <exception cref="InvalidOperationException">A promotion would break one of the rules.</exception>
    public static void Reject(IReadOnlyCollection<ISymbol> promoted, IReadOnlyCollection<IPropertySymbol> setters, Func<ISymbol, string> nameOf,
        SourceTypes types, Func<SyntaxTree, SemanticModel> modelOf, SymbolIdentity identity)
    {
        var promotedSet = promoted.ToHashSet(SymbolEqualityComparer.Default);
        foreach (var symbol in promoted.OrderBy(symbol => symbol.ToDisplayString(), StringComparer.Ordinal))
        {
            RejectExposedField(symbol, promotedSet);
            if (symbol.DeclaredAccessibility == Accessibility.Private)
                foreach (var declaration in RelocationSymbols.DeclarationsOf(identity, symbol))
                    RejectArgumentGuard(symbol, declaration, modelOf);
            RejectHiding(symbol, nameOf(symbol), types);
        }
        foreach (var property in setters.OrderBy(property => property.ToDisplayString(), StringComparer.Ordinal))
            if (property.SetMethod is { DeclaredAccessibility: Accessibility.Private } setter)
                foreach (var declaration in RelocationSymbols.DeclarationsOf(identity, setter))
                    RejectArgumentGuard(setter, declaration, modelOf);
    }

    /// <summary>Fails when the member is a private mutable instance field of a class that only its own class could reach (SME6436).</summary>
    /// <exception cref="InvalidOperationException">The field would be exposed for other types to write.</exception>
    private static void RejectExposedField(ISymbol symbol, HashSet<ISymbol> promoted)
    {
        if (symbol is not IFieldSymbol { IsStatic: false, IsConst: false, IsReadOnly: false, IsImplicitlyDeclared: false,
                DeclaredAccessibility: Accessibility.Private, ContainingType.TypeKind: TypeKind.Class } field)
            return;
        // A field of a type nested privately is reached only by the type declaring it, unless that type becomes internal too.
        for (var type = field.ContainingType; type is not null; type = type.ContainingType)
            if (type.DeclaredAccessibility == Accessibility.Private && !promoted.Contains(type))
                return;
        throw new InvalidOperationException($"Moved code uses {field.ToDisplayString()}, a mutable field of the class " +
            $"{field.ContainingType.ToDisplayString()} ({Where(field)}). Made internal, other types could write it directly " +
            "(SME6436), so it stays private: give the class a named operation or a property for what the moved code needs, " +
            "and the next run moves the code.");
    }

    /// <summary>Fails when a private method or accessor that becomes internal checks an argument by hand (SME6201).</summary>
    /// <exception cref="InvalidOperationException">The declaration's body holds a hand-written argument check.</exception>
    private static void RejectArgumentGuard(ISymbol symbol, SyntaxNode declaration, Func<SyntaxTree, SemanticModel> modelOf)
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
            throw new InvalidOperationException($"Moved code uses {symbol.ToDisplayString()}, which checks an argument by hand " +
                $"({check.Condition}, {Where(check)}). Made internal, that check must use Ensure (SME6201), so it stays private: " +
                "write the check with Ensure, and the next run moves the code.");
        }
    }

    /// <summary>
    /// Fails when a type deriving from the member's owner declares a member of the name the promoted member takes without
    /// <c>new</c>: once the promoted member is internal, the derived member hides it (CS0108). A method of other parameters
    /// overloads it instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">A derived type's member would hide the promoted member.</exception>
    private static void RejectHiding(ISymbol symbol, string name, SourceTypes types)
    {
        // A constructor is not inherited, so a derived type's constructors hide nothing.
        if (symbol.ContainingType is not { } owner || symbol is IMethodSymbol { MethodKind: not MethodKind.Ordinary })
            return;
        foreach (var derived in types.DerivedFrom(owner))
            foreach (var member in derived.GetMembers(name).Where(member => !member.IsImplicitlyDeclared))
            {
                if (symbol is IMethodSymbol method && member is IMethodSymbol other && !SameParameters(method, other) || Hides(member))
                    continue;
                throw new InvalidOperationException($"Moved code uses {symbol.ToDisplayString()}, and {member.ToDisplayString()} " +
                    $"({Where(member)}), in a type deriving from {owner.ToDisplayString()}, takes its name '{name}': made internal, " +
                    "it would be hidden (CS0108), so it stays private. Rename one of them, and the next run moves the code.");
            }
    }

    /// <summary>Whether two methods take the same parameter types and kinds.</summary>
    private static bool SameParameters(IMethodSymbol first, IMethodSymbol second) =>
        first.Parameters.Length == second.Parameters.Length && first.Parameters.Zip(second.Parameters).All(pair =>
            pair.First.RefKind == pair.Second.RefKind && SymbolEqualityComparer.Default.Equals(pair.First.Type, pair.Second.Type));

    /// <summary>Whether a member is declared <c>new</c>, so that hiding is intended.</summary>
    private static bool Hides(ISymbol member) => member.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax())
        .Select(syntax => syntax is VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax field } ? field : syntax)
        .OfType<MemberDeclarationSyntax>().Any(declaration => declaration.Modifiers.Any(SyntaxKind.NewKeyword));

    /// <summary>Where a symbol is declared, for a failure message.</summary>
    private static string Where(ISymbol symbol) =>
        symbol.Locations.FirstOrDefault(location => location.IsInSource) is { } location ? location.GetLineSpan().ToString() : symbol.ToDisplayString();

    /// <summary>Where a node is, for a failure message.</summary>
    private static string Where(SyntaxNode node) => node.GetLocation().GetLineSpan().ToString();
}
