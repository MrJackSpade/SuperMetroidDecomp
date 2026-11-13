using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// Every modifier <see cref="SymbolRelocator"/> writes. A declaration that moves out of its owner into a holder class, and
/// a staying declaration that moved code uses, become internal when they were private or protected or carried no
/// accessibility: outside the owner they are reached only through the assembly and the projects it admits. Each
/// declaration kind gets only the modifiers C# allows it:
/// <list type="bullet">
/// <item>An extension block takes no modifier: a moved block keeps its header as written, and its members become internal
/// as any moved member does. The 2026-11-13 cleanup at 2390b2f3af wrote a moved block as <c>internal extension(...)</c>
/// (CS0106).</item>
/// <item>An explicit interface implementation takes no accessibility and keeps its declaration as written; an operator or
/// conversion is public, as C# requires, and keeps it too.</item>
/// <item>Every other declaration's accessibility, one keyword or two (<c>protected internal</c>, <c>private protected</c>),
/// becomes the one keyword <c>internal</c>.</item>
/// <item>A moved declaration drops <c>new</c>: its holder has no base member to hide. An instance member that becomes an
/// extension member drops <c>readonly</c>, which an extension member cannot take (CS0106) and which its by-value receiver
/// makes moot.</item>
/// <item>A staying property's private or protected setter that moved code writes becomes internal, or loses its modifier
/// when the property is internal, since an accessor's accessibility must be narrower than its property's (CS0273).</item>
/// </list>
/// A declaration of a kind a holder cannot take, and a staying declaration whose accessibility cannot change without
/// changing what it overrides or what overrides it, fail the run rather than being written wrongly.
/// </summary>
internal static class RelocationModifiers
{
    /// <summary>The text of a moved declaration as a member of its holder class.</summary>
    /// <param name="text">The declaration's moved text, with its leading and trailing trivia.</param>
    /// <param name="options">The parse options of the tree the declaration leaves.</param>
    /// <param name="becomesExtension">Whether the declaration is an instance member of a type that becomes an extension member.</param>
    /// <exception cref="InvalidOperationException">The text is not one declaration, or is a kind a holder cannot take.</exception>
    public static string AsHolderMember(string text, CSharpParseOptions options, bool becomesExtension)
    {
        var member = SyntaxFactory.ParseMemberDeclaration(text, options: options);
        if (member is null || member.FullSpan.Length != text.Length || member.ContainsDiagnostics)
            throw new InvalidOperationException($"The moved text is not one declaration the relocator can write:\n{text}");
        var edits = new List<TextEdit>();
        if (member is ExtensionBlockDeclarationSyntax block)
        {
            foreach (var blockMember in block.Members)
                edits.AddRange(HolderAccessibility(blockMember));
        }
        else
        {
            edits.AddRange(HolderAccessibility(member));
            edits.AddRange(member.Modifiers.Where(m => m.IsKind(SyntaxKind.NewKeyword)).Select(Removal));
            if (becomesExtension)
                edits.AddRange(member.Modifiers.Where(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)).Select(Removal));
        }
        return Applied(text, edits);
    }

    /// <summary>The edits that make a staying declaration internal, so moved code can use it from outside its owner.</summary>
    /// <param name="declaration">The declaration; a field's whole declaration.</param>
    /// <exception cref="InvalidOperationException">
    /// The declaration takes no accessibility, or is virtual, abstract or an override, whose accessibility is bound to what
    /// it overrides or what overrides it.
    /// </exception>
    public static IReadOnlyList<TextEdit> Promoted(MemberDeclarationSyntax declaration)
    {
        if (!TakesAccessibility(declaration))
            throw new InvalidOperationException($"{Described(declaration)} takes no accessibility modifier, so moved code cannot reach it from outside its owner.");
        if (declaration.Modifiers.Any(m => m.Kind() is SyntaxKind.VirtualKeyword or SyntaxKind.AbstractKeyword or SyntaxKind.OverrideKeyword))
            throw new InvalidOperationException($"{Described(declaration)} cannot become internal without changing what it overrides or " +
                "what overrides it, so moved code cannot reach it from outside its owner.");
        return Internal(declaration.Modifiers, InsertionPoint(declaration));
    }

    /// <summary>
    /// The edits that let moved code write a staying property through its private or protected setter: the setter becomes
    /// internal, or, when the property is internal, loses its modifier and takes the property's.
    /// </summary>
    /// <param name="setter">The set or init accessor.</param>
    /// <param name="propertyInternal">Whether the property is internal once moved code can use it.</param>
    /// <exception cref="InvalidOperationException">The setter carries no accessibility modifier.</exception>
    public static IReadOnlyList<TextEdit> PromotedSetter(AccessorDeclarationSyntax setter, bool propertyInternal)
    {
        var accessibility = setter.Modifiers.Where(IsAccessibility).ToList();
        if (accessibility.Count == 0)
            throw new InvalidOperationException($"The {setter.Keyword.Text} accessor at {setter.GetLocation().GetLineSpan()} carries no accessibility to widen.");
        return propertyInternal ? [.. accessibility.Select(Removal)] : Internal(setter.Modifiers, setter.Keyword.SpanStart);
    }

    /// <summary>The edits that write a moved declaration's accessibility as a member of its holder.</summary>
    /// <exception cref="InvalidOperationException">The declaration is a kind a holder cannot take as written.</exception>
    private static IReadOnlyList<TextEdit> HolderAccessibility(MemberDeclarationSyntax member)
    {
        if (ExplicitlyImplements(member))
            return [];
        if (member is OperatorDeclarationSyntax or ConversionOperatorDeclarationSyntax)
            return member.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)) ? []
                : throw new InvalidOperationException($"{Described(member)} is an operator that is not public.");
        if (!TakesAccessibility(member) || member is ConstructorDeclarationSyntax)
            throw new InvalidOperationException($"{Described(member)} cannot be written into a holder class.");
        if (member is not (BaseTypeDeclarationSyntax or DelegateDeclarationSyntax) &&
            member.Modifiers.Any(m => m.Kind() is SyntaxKind.VirtualKeyword or SyntaxKind.AbstractKeyword or SyntaxKind.OverrideKeyword or SyntaxKind.SealedKeyword))
            throw new InvalidOperationException($"{Described(member)} is virtual, abstract or an override, which a holder class cannot declare.");
        return Internal(member.Modifiers, InsertionPoint(member));
    }

    /// <summary>
    /// Whether a declaration of this kind takes an accessibility modifier: not an extension block, an explicit interface
    /// implementation, an operator, a static constructor, a destructor, or anything that is not a type or member.
    /// </summary>
    private static bool TakesAccessibility(MemberDeclarationSyntax member) => !ExplicitlyImplements(member) && member switch
    {
        FieldDeclarationSyntax or EventFieldDeclarationSyntax or MethodDeclarationSyntax or PropertyDeclarationSyntax
            or EventDeclarationSyntax or IndexerDeclarationSyntax or DelegateDeclarationSyntax => true,
        ConstructorDeclarationSyntax constructor => !constructor.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
        BaseTypeDeclarationSyntax type => type is not ExtensionBlockDeclarationSyntax,
        _ => false,
    };

    /// <summary>Whether the declaration implements an interface member explicitly.</summary>
    private static bool ExplicitlyImplements(MemberDeclarationSyntax member) => member switch
    {
        MethodDeclarationSyntax method => method.ExplicitInterfaceSpecifier is not null,
        BasePropertyDeclarationSyntax property => property.ExplicitInterfaceSpecifier is not null,
        OperatorDeclarationSyntax @operator => @operator.ExplicitInterfaceSpecifier is not null,
        ConversionOperatorDeclarationSyntax conversion => conversion.ExplicitInterfaceSpecifier is not null,
        _ => false,
    };

    /// <summary>
    /// The edits that make a modifier list internal: none when it is public or internal already, else one
    /// <c>internal</c> in place of the first accessibility keyword with any other removed, or inserted at
    /// <paramref name="insertAt"/> when there is none.
    /// </summary>
    private static IReadOnlyList<TextEdit> Internal(SyntaxTokenList modifiers, int insertAt)
    {
        var accessibility = modifiers.Where(IsAccessibility).ToList();
        if (accessibility is [var only] && only.Kind() is SyntaxKind.PublicKeyword or SyntaxKind.InternalKeyword)
            return [];
        if (accessibility.Count == 0)
            return [new TextEdit(new TextSpan(insertAt, 0), "internal ")];
        return [new TextEdit(accessibility[0].Span, "internal"), .. accessibility.Skip(1).Select(Removal)];
    }

    /// <summary>Whether a modifier states accessibility.</summary>
    private static bool IsAccessibility(SyntaxToken modifier) =>
        modifier.Kind() is SyntaxKind.PublicKeyword or SyntaxKind.InternalKeyword or SyntaxKind.ProtectedKeyword or SyntaxKind.PrivateKeyword;

    /// <summary>Where an accessibility modifier goes: before the first modifier, else after the attributes, else at the start.</summary>
    private static int InsertionPoint(MemberDeclarationSyntax member) =>
        member.Modifiers.Count > 0 ? member.Modifiers[0].SpanStart
        : member.AttributeLists.Count > 0 ? member.AttributeLists[^1].GetLastToken().GetNextToken().SpanStart
        : member.GetFirstToken().SpanStart;

    /// <summary>The edit that removes a modifier and the space after it.</summary>
    private static TextEdit Removal(SyntaxToken modifier) => new(TextSpan.FromBounds(modifier.SpanStart, modifier.FullSpan.End), "");

    /// <summary>The text with the edits applied, from the end so earlier spans hold.</summary>
    /// <exception cref="InvalidOperationException">Two edits overlap.</exception>
    private static string Applied(string text, IEnumerable<TextEdit> edits)
    {
        var output = new StringBuilder(text);
        int floor = int.MaxValue;
        foreach (var (span, replacement) in edits.OrderByDescending(e => e.Span.Start).ThenByDescending(e => e.Span.End))
        {
            if (span.End > floor)
                throw new InvalidOperationException($"Overlapping modifier edits at {span} in:\n{text}");
            output.Remove(span.Start, span.Length).Insert(span.Start, replacement);
            floor = span.Start;
        }
        return output.ToString();
    }

    /// <summary>A declaration's kind and first line, for a failure message.</summary>
    private static string Described(MemberDeclarationSyntax member)
    {
        string where = member.SyntaxTree.FilePath.Length > 0 ? $" at {member.GetLocation().GetLineSpan()}" : "";
        string first = member.WithoutTrivia().ToString().Split('\n')[0].TrimEnd('\r');
        return $"The {member.Kind()} '{first}'{where}";
    }
}
