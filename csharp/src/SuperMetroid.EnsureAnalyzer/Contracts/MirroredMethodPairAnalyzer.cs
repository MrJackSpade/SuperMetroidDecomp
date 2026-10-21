using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #1370: behaviour written twice, once per direction, drifts apart, and a fix to one side never reaches the other. Two
/// methods of one type whose names differ only by opposing direction tokens (<c>Left</c>/<c>Right</c>, <c>Up</c>/<c>Down</c>,
/// <c>Clockwise</c>/<c>Counterclockwise</c>, <c>Add</c>/<c>Subtract</c>, <c>First</c>/<c>Second</c>) are reported when
/// their bodies are the same syntax once mirrored: identifiers may swap direction tokens, numeric literals may differ in
/// sign, and <c>+</c>/<c>-</c>, <c>+=</c>/<c>-=</c>, <c>++</c>/<c>--</c> and the ordering comparisons stand for each other.
/// A name with a direction that both bodies share unflipped is not mirrored.
/// A body that only forwards to one call is already the parameterised shape and is not reported. A pair that is a
/// deliberate, behaviourally distinct asymmetry says so with <c>[MirroredAsymmetry(reason)]</c> on either method.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MirroredMethodPairAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Two methods are one behaviour written once per direction.</summary>
    public const string MirroredPairId = "SME6418";

    /// <summary>The attribute that justifies a deliberate asymmetry at its site.</summary>
    public const string MirroredAsymmetryAttributeName = "MirroredAsymmetryAttribute";

    /// <summary>Reports a mirrored method pair.</summary>
    private static readonly DiagnosticDescriptor MirroredPairRule = new(
        MirroredPairId,
        "Behaviour written once per direction",
        "'{0}' and '{1}' are the same behaviour mirrored; write it once, parameterised by a direction descriptor",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Two methods whose names differ only by opposing direction tokens and whose bodies are identical once mirrored " +
            "are one method taking a typed direction; a deliberate asymmetry declares [MirroredAsymmetry] with its reason (#1370).");

    /// <summary>
    /// One direction token at a word boundary: a capitalised token starts a PascalCase word wherever it appears, a
    /// lower-case token only at the start of an identifier, and neither may run on into a lower-case letter.
    /// </summary>
    private static readonly Regex DirectionToken = new(
        "(?:(?<![A-Za-z])(left|right|up|down|add|subtract|first|second|counterclockwise|anticlockwise|clockwise)" +
        "|(Left|Right|Up|Down|Add|Subtract|First|Second|CounterClockwise|Counterclockwise|Anticlockwise|Clockwise))(?![a-z])",
        RegexOptions.CultureInvariant);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MirroredPairRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    /// <summary>Reports each mirrored pair among one type's source methods.</summary>
    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct))
            return;
        var groups = new Dictionary<string, List<MirroredMethod>>();
        foreach (IMethodSymbol method in type.GetMembers().OfType<IMethodSymbol>())
        {
            if (method.MethodKind != MethodKind.Ordinary || method.IsImplicitlyDeclared ||
                method.DeclaringSyntaxReferences.Length != 1 ||
                method.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken) is not MethodDeclarationSyntax declaration ||
                ((SyntaxNode?)declaration.Body ?? declaration.ExpressionBody?.Expression) is not { } body)
                continue;
            (string canonical, string sides) = Canonical(method.Name);
            if (sides.Length == 0 || IsForwardingWrapper(body))
                continue;
            if (!groups.TryGetValue(canonical, out List<MirroredMethod>? group))
                groups[canonical] = group = [];
            group.Add(new MirroredMethod(method, sides, body));
        }
        foreach (List<MirroredMethod> group in groups.Values)
        {
            for (int first = 0; first < group.Count; first++)
            for (int second = first + 1; second < group.Count; second++)
            {
                (IMethodSymbol a, string aSides, SyntaxNode aBody) = group[first];
                (IMethodSymbol b, string bSides, SyntaxNode bBody) = group[second];
                if (!Flipped(aSides, bSides) || IsJustified(a) || IsJustified(b) || !SameSignature(a, b) ||
                    !MirroredBodies(aBody, bBody))
                    continue;
                context.ReportDiagnostic(Diagnostic.Create(MirroredPairRule, a.Locations[0], [b.Locations[0]], a.Name, b.Name));
            }
        }
    }

    /// <summary>
    /// The name with each direction token replaced by its axis placeholder, and the side each token names, in order: a
    /// mirrored name has the same placeholders with every side flipped.
    /// </summary>
    private static CanonicalMethodName Canonical(string name)
    {
        var sides = new StringBuilder();
        string canonical = DirectionToken.Replace(name, match =>
        {
            (char axis, char side) = Classify(match.Value);
            sides.Append(side);
            return "\u0001" + axis + "\u0001";
        });
        return new CanonicalMethodName(canonical, sides.ToString());
    }

    /// <summary>The axis a direction token names and which of its two sides.</summary>
    private static MirrorClassification Classify(string token) => token.ToUpperInvariant() switch
    {
        "LEFT" => new MirrorClassification('h', '0'),
        "RIGHT" => new MirrorClassification('h', '1'),
        "UP" => new MirrorClassification('v', '0'),
        "DOWN" => new MirrorClassification('v', '1'),
        "ADD" => new MirrorClassification('s', '0'),
        "SUBTRACT" => new MirrorClassification('s', '1'),
        "FIRST" => new MirrorClassification('o', '0'),
        "SECOND" => new MirrorClassification('o', '1'),
        "CLOCKWISE" => new MirrorClassification('r', '0'),
        "COUNTERCLOCKWISE" or "ANTICLOCKWISE" => new MirrorClassification('r', '1'),
        _ => throw new ArgumentOutOfRangeException(nameof(token), token, "Not a direction token."),
    };

    /// <summary>
    /// Whether two side sequences of equal length name the opposite side somewhere: a mirrored name flips its facing and
    /// may keep another axis (<c>FacingRightAimingUp</c> mirrors <c>FacingLeftAimingUp</c>).
    /// </summary>
    private static bool Flipped(string left, string right)
    {
        if (left.Length != right.Length)
            return false;
        for (int index = 0; index < left.Length; index++)
            if (left[index] != right[index])
                return true;
        return false;
    }

    /// <summary>Whether two methods take the same parameter types and return the same type.</summary>
    private static bool SameSignature(IMethodSymbol a, IMethodSymbol b) =>
        a.IsStatic == b.IsStatic && a.Parameters.Length == b.Parameters.Length &&
        SymbolEqualityComparer.Default.Equals(a.ReturnType, b.ReturnType) &&
        a.Parameters.Zip(b.Parameters, (x, y) => SymbolEqualityComparer.Default.Equals(x.Type, y.Type) && x.RefKind == y.RefKind).All(same => same);

    /// <summary>A body whose only work is one call, possibly returned: the direction already is that call's argument.</summary>
    private static bool IsForwardingWrapper(SyntaxNode body) =>
        body is InvocationExpressionSyntax ||
        body is BlockSyntax { Statements.Count: 1 } block && block.Statements[0] is
            ExpressionStatementSyntax { Expression: InvocationExpressionSyntax } or
            ReturnStatementSyntax { Expression: InvocationExpressionSyntax };

    /// <summary>Whether two bodies are the same token sequence once mirrored.</summary>
    private static bool MirroredBodies(SyntaxNode a, SyntaxNode b)
    {
        ImmutableArray<SyntaxToken> left = Significant(a);
        ImmutableArray<SyntaxToken> right = Significant(b);
        if (left.Length != right.Length)
            return false;
        for (int index = 0; index < left.Length; index++)
            if (!MirroredTokens(left[index], right[index]))
                return false;
        return true;
    }

    /// <summary>The body's tokens without a unary minus applied to a numeric literal, so literals may differ in sign.</summary>
    private static ImmutableArray<SyntaxToken> Significant(SyntaxNode body) =>
        [.. body.DescendantTokens().Where(token => !(token.IsKind(SyntaxKind.MinusToken) &&
            token.Parent is PrefixUnaryExpressionSyntax { Operand: LiteralExpressionSyntax literal } &&
            literal.IsKind(SyntaxKind.NumericLiteralExpression)))];

    /// <summary>Whether two tokens are the same, or each other's mirror.</summary>
    private static bool MirroredTokens(SyntaxToken a, SyntaxToken b)
    {
        if (a.IsKind(SyntaxKind.IdentifierToken) && b.IsKind(SyntaxKind.IdentifierToken))
        {
            // A mirror flips every name that has a direction: the same directional name on both sides is not one.
            if (a.ValueText == b.ValueText)
                return Canonical(a.ValueText).Sides.Length == 0;
            (string aCanonical, string aSides) = Canonical(a.ValueText);
            (string bCanonical, string bSides) = Canonical(b.ValueText);
            return aCanonical == bCanonical && Flipped(aSides, bSides);
        }
        if (a.IsKind(SyntaxKind.NumericLiteralToken) && b.IsKind(SyntaxKind.NumericLiteralToken))
            return a.ValueText == b.ValueText;
        SyntaxKind aKind = a.Kind(), bKind = b.Kind();
        if (aKind == bKind)
            return aKind is not (SyntaxKind.StringLiteralToken or SyntaxKind.CharacterLiteralToken) || a.ValueText == b.ValueText;
        return MirrorClass(aKind) is { } aClass && MirrorClass(bKind) == aClass;
    }

    /// <summary>The class of operators that stand for each other in mirrored code, or null for any other token.</summary>
    private static int? MirrorClass(SyntaxKind kind)
    {
        if (kind is SyntaxKind.PlusToken or SyntaxKind.MinusToken)
            return 0;
        if (kind is SyntaxKind.PlusEqualsToken or SyntaxKind.MinusEqualsToken)
            return 1;
        if (kind is SyntaxKind.PlusPlusToken or SyntaxKind.MinusMinusToken)
            return 2;
        if (kind is SyntaxKind.LessThanToken or SyntaxKind.GreaterThanToken or SyntaxKind.LessThanEqualsToken or SyntaxKind.GreaterThanEqualsToken)
            return 3;
        return null;
    }

    /// <summary>Whether <paramref name="method"/> carries a deliberate-asymmetry justification.</summary>
    private static bool IsJustified(IMethodSymbol method) =>
        method.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == MirroredAsymmetryAttributeName);
}
