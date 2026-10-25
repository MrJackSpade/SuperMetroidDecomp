using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #1372: a mode, state, direction or menu selection stored as an integer and compared against bare literals keeps its
/// meaning only in a comment, and nothing stops a value with no meaning. In SuperMetroid.Core an integral field or
/// property whose name ends in a categorical noun is reported wherever it is compared to an integer literal, and any
/// such member of the assembly compared for equality to two or more different literals is reported once at its
/// declaration, whatever its name. A member that is a genuine count or quantity says so with
/// <c>[IntegerQuantity(reason)]</c> at its declaration, and a packed native word whose bit fields a typed view decodes
/// says so with <c>[PackedFields(reason)]</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IntegerCodedModeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A categorically named integer is compared to a literal.</summary>
    public const string CategoricalLiteralId = "SME6421";

    /// <summary>An integer member is compared to several different literals.</summary>
    public const string SeveralLiteralsId = "SME6422";

    /// <summary>The attribute that marks an integer member as a genuine count or quantity.</summary>
    public const string IntegerQuantityAttributeName = "IntegerQuantityAttribute";

    /// <summary>The attribute that marks an integer member as a packed native word whose fields a typed view decodes.</summary>
    public const string NativeWordAttributeName = "PackedFieldsAttribute";

    /// <summary>The assembly whose members are checked.</summary>
    public const string CheckedAssembly = "SuperMetroid.Core";

    /// <summary>Name endings that say a member holds one of a set of exclusive categories, not a quantity.</summary>
    private static readonly ImmutableArray<string> CategoricalNouns =
        ["Mode", "State", "Phase", "Direction", "Kind", "Page", "Selection", "SelectedItem", "Function", "Stage", "Status", "Type"];

    /// <summary>Reports a categorical integer compared to a literal.</summary>
    private static readonly DiagnosticDescriptor CategoricalLiteralRule = new(
        CategoricalLiteralId,
        "Mode stored as an integer",
        "'{0}' names a category but is an integer compared to {1}; make it an enum (or a state object) with semantic members, converting the native word at its codec",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An integral field or property whose name ends in Mode, State, Phase, Direction, Kind, Page, Selection, " +
            "SelectedItem, Function, Stage, Status or Type, compared to an integer literal, is an integer-coded category (#1372).");

    /// <summary>Reports a member compared to several literals.</summary>
    private static readonly DiagnosticDescriptor SeveralLiteralsRule = new(
        SeveralLiteralsId,
        "Integer compared to several coded values",
        "'{0}' is compared for equality to {1} different literals ({2}); name the values as an enum, or mark the member [IntegerQuantity] with why it is a quantity",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An integral member of SuperMetroid.Core compared by ==, != or an is pattern to two or more different " +
            "integer literals codes categories in its values; a switch over an index is a lookup and is not counted (#1372).",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [CategoricalLiteralRule, SeveralLiteralsRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CheckedAssembly)
                return;
            var equalities = new ConcurrentBag<MemberEquality>();
            start.RegisterSyntaxNodeAction(node => AnalyzeBinary(node, equalities),
                SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression, SyntaxKind.GreaterThanExpression, SyntaxKind.GreaterThanOrEqualExpression);
            start.RegisterSyntaxNodeAction(node => AnalyzeIsPattern(node, equalities), SyntaxKind.IsPatternExpression);
            start.RegisterSyntaxNodeAction(node => AnalyzeSwitch(node, equalities), SyntaxKind.SwitchExpression, SyntaxKind.SwitchStatement);
            start.RegisterCompilationEndAction(end =>
            {
                foreach (IGrouping<ISymbol, MemberEquality> member in
                         equalities.GroupBy(entry => entry.Member, SymbolEqualityComparer.Default))
                {
                    long[] values = [.. member.Select(entry => entry.Value).Distinct().OrderBy(value => value)];
                    if (values.Length < 2 || IsCategorical(member.Key))
                        continue;
                    end.ReportDiagnostic(Diagnostic.Create(SeveralLiteralsRule, member.Key.Locations[0], member.Key.Name,
                        values.Length, string.Join(", ", values)));
                }
            });
        });
    }

    /// <summary>Checks a comparison of a member against an integer literal.</summary>
    private static void AnalyzeBinary(SyntaxNodeAnalysisContext context, ConcurrentBag<MemberEquality> equalities)
    {
        var binary = (BinaryExpressionSyntax)context.Node;
        bool equality = binary.IsKind(SyntaxKind.EqualsExpression) || binary.IsKind(SyntaxKind.NotEqualsExpression);
        Check(context, binary.Left, binary.Right, binary, equality, equalities);
        Check(context, binary.Right, binary.Left, binary, equality, equalities);
    }

    /// <summary>Checks <c>member is literal</c>, <c>is not</c>, <c>or</c> and relational patterns.</summary>
    private static void AnalyzeIsPattern(SyntaxNodeAnalysisContext context, ConcurrentBag<MemberEquality> equalities)
    {
        var expression = (IsPatternExpressionSyntax)context.Node;
        if (Member(expression.Expression, context) is { } member)
            ReportAll(context, member, Literals([expression.Pattern], context), expression.GetLocation(), equalities, countEqualities: true);
    }

    /// <summary>Checks a switch statement's or expression's constant arms over a member.</summary>
    private static void AnalyzeSwitch(SyntaxNodeAnalysisContext context, ConcurrentBag<MemberEquality> equalities)
    {
        if (context.Node is SwitchExpressionSyntax switchExpression && Member(switchExpression.GoverningExpression, context) is { } governed)
        {
            ReportAll(context, governed, Literals([.. switchExpression.Arms.Select(arm => arm.Pattern)], context),
                switchExpression.GoverningExpression.GetLocation(), equalities, countEqualities: false);
        }
        else if (context.Node is SwitchStatementSyntax switchStatement && Member(switchStatement.Expression, context) is { } member)
        {
            var literals = new List<ModeLiteral>();
            foreach (SwitchLabelSyntax label in switchStatement.Sections.SelectMany(section => section.Labels))
            {
                if (label is CaseSwitchLabelSyntax caseLabel && Literal(caseLabel.Value, context) is { } value)
                    literals.Add(new ModeLiteral(value, true));
                else if (label is CasePatternSwitchLabelSyntax patternLabel)
                    literals.AddRange(Literals([patternLabel.Pattern], context));
            }
            ReportAll(context, member, literals, switchStatement.Expression.GetLocation(), equalities, countEqualities: false);
        }
    }

    /// <summary>The integer literals of constant (equality) and relational patterns among <paramref name="patterns"/>.</summary>
    private static List<ModeLiteral> Literals(IEnumerable<PatternSyntax> patterns, SyntaxNodeAnalysisContext context)
    {
        var literals = new List<ModeLiteral>();
        foreach (PatternSyntax pattern in patterns.SelectMany(root => root.DescendantNodesAndSelf().OfType<PatternSyntax>()))
        {
            if (pattern is ConstantPatternSyntax constant && Literal(constant.Expression, context) is { } value)
                literals.Add(new ModeLiteral(value, true));
            else if (pattern is RelationalPatternSyntax relational && Literal(relational.Expression, context) is { } bound)
                literals.Add(new ModeLiteral(bound, false));
        }
        return literals;
    }

    /// <summary>
    /// Reports one comparison of a categorical member against its literals, or records its equalities for the
    /// several-literals rule; a switch over an index is a lookup, so its arms are not counted there.
    /// </summary>
    private static void ReportAll(SyntaxNodeAnalysisContext context, ISymbol member, List<ModeLiteral> literals,
        Location location, ConcurrentBag<MemberEquality> equalities, bool countEqualities)
    {
        if (literals.Count == 0)
            return;
        if (IsCategorical(member))
        {
            context.ReportDiagnostic(Diagnostic.Create(CategoricalLiteralRule, location, member.Name,
                string.Join(" and ", literals.Select(literal => literal.Value).Distinct())));
            return;
        }
        if (!countEqualities)
            return;
        foreach ((long value, bool equality) in literals)
        {
            if (equality)
                equalities.Add(new MemberEquality(member, value));
        }
    }

    /// <summary>Checks one side of a comparison as the member and the other as the literal.</summary>
    private static void Check(SyntaxNodeAnalysisContext context, ExpressionSyntax memberSide, ExpressionSyntax literalSide,
        SyntaxNode comparison, bool equality, ConcurrentBag<MemberEquality> equalities)
    {
        if (Member(memberSide, context) is { } member && Literal(literalSide, context) is { } value)
            Report(context, member, value, comparison.GetLocation(), equality, equalities);
    }

    /// <summary>Reports a categorical member at once, and records an equality for the several-literals rule.</summary>
    private static void Report(SyntaxNodeAnalysisContext context, ISymbol member, long value, Location location, bool equality,
        ConcurrentBag<MemberEquality> equalities)
    {
        if (IsCategorical(member))
            context.ReportDiagnostic(Diagnostic.Create(CategoricalLiteralRule, location, member.Name, value));
        else if (equality)
            equalities.Add(new MemberEquality(member, value));
    }

    /// <summary>
    /// The source-declared integral field or property <paramref name="expression"/> reads, looking through parentheses,
    /// casts and <c>unchecked</c>; null for anything else or for a member justified as a quantity.
    /// </summary>
    private static ISymbol? Member(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
        while (true)
        {
            switch (expression)
            {
                case ParenthesizedExpressionSyntax parenthesized:
                    expression = parenthesized.Expression;
                    continue;
                case CastExpressionSyntax cast:
                    expression = cast.Expression;
                    continue;
                case CheckedExpressionSyntax checkedExpression:
                    expression = checkedExpression.Expression;
                    continue;
                default:
                    break;
            }
            break;
        }
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken).Symbol;
        ITypeSymbol? type = symbol switch
        {
            IFieldSymbol field => field.Type,
            IPropertySymbol property when !property.IsIndexer => property.Type,
            _ => null,
        };
        if (symbol is null || type is null || !IsIntegral(type) || !symbol.Locations.Any(location => location.IsInSource) ||
            !SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, context.Compilation.Assembly) ||
            symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name is IntegerQuantityAttributeName or NativeWordAttributeName))
            return null;
        return symbol.OriginalDefinition;
    }

    /// <summary>The value of an integer literal, possibly negated; null for anything else.</summary>
    private static long? Literal(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
        bool negative = false;
        while (true)
        {
            if (expression is ParenthesizedExpressionSyntax parenthesized)
                expression = parenthesized.Expression;
            else if (expression is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.UnaryMinusExpression } minus)
            {
                negative = !negative;
                expression = minus.Operand;
            }
            else
                break;
        }
        if (expression is not LiteralExpressionSyntax literal || !literal.IsKind(SyntaxKind.NumericLiteralExpression))
            return null;
        Optional<object?> constant = context.SemanticModel.GetConstantValue(literal, context.CancellationToken);
        if (!constant.HasValue || constant.Value is float or double or decimal || constant.Value is null)
            return null;
        long value = Convert.ToInt64(constant.Value, System.Globalization.CultureInfo.InvariantCulture);
        return negative ? -value : value;
    }

    /// <summary>Whether a member's name ends in a categorical noun.</summary>
    private static bool IsCategorical(ISymbol member) =>
        CategoricalNouns.Any(noun => member.Name.EndsWith(noun, StringComparison.Ordinal));

    /// <summary>Whether a type is one of the integral primitives.</summary>
    private static bool IsIntegral(ITypeSymbol type) => type.SpecialType is
        SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16 or
        SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64;
}
