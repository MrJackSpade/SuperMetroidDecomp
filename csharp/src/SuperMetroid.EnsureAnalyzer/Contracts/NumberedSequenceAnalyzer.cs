using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #1369: a sequence spelled out as numbered sibling members (<c>Segment0Distance</c>..<c>Segment4Distance</c>) cannot be
/// iterated, so every caller repeats a statement per member and maps an index back to a member with a hand-written
/// switch. A family of at least three same-typed fields or properties whose names differ only by a gap-free ordinal
/// run is reported, as is a switch whose consecutive integer arms each pick one member of such a family, and a
/// hand-written list type whose indexer is a switch over constant indices or ignores its index. A gap-free family that
/// is really a set of distinct identities says so with <c>[NumberedIdentities(reason)]</c> on its members or its type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NumberedSequenceAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A type spells a sequence as numbered sibling members.</summary>
    public const string FamilyId = "SME6415";

    /// <summary>A switch maps consecutive integer constants to the members of a numbered family.</summary>
    public const string IndexSwitchId = "SME6416";

    /// <summary>A hand-written list type stands in for an array.</summary>
    public const string HandWrittenListId = "SME6417";

    /// <summary>The attribute that justifies a gap-free numbered family of distinct identities at its site.</summary>
    public const string NumberedIdentitiesAttributeName = "NumberedIdentitiesAttribute";

    /// <summary>The smallest numbered family reported at type level.</summary>
    private const int MinimumFamilySize = 3;

    /// <summary>The longest ordinal digit run read as a number; longer runs are identities, not positions.</summary>
    private const int MaximumOrdinalDigits = 4;

    /// <summary>Reports a numbered member family.</summary>
    private static readonly DiagnosticDescriptor FamilyRule = new(
        FamilyId,
        "Sequence spelled as numbered members",
        "'{0}'..'{1}' of {2} are one sequence of {3} elements; hold one indexed collection and say what the index means on it",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Members with the same meaning and type that differ only by an ordinal in their name are one array or indexed " +
            "collection; a gap-free family of distinct identities declares [NumberedIdentities] with its reason (#1369).");

    /// <summary>Reports an index-to-member switch.</summary>
    private static readonly DiagnosticDescriptor IndexSwitchRule = new(
        IndexSwitchId,
        "Index mapped to numbered members by a switch",
        "This switch maps indices {0}..{1} to '{2}'..'{3}'; index one collection instead",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A switch whose consecutive integer arms each pick one member of a numbered family is an indexer written by " +
            "hand; the family is one collection (#1369).");

    /// <summary>Reports a hand-written list type.</summary>
    private static readonly DiagnosticDescriptor HandWrittenListRule = new(
        HandWrittenListId,
        "Hand-written list type",
        "{0} is a hand-written list whose indexer {1}; use an array, an ImmutableArray, or a single value",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A type whose indexer switches over constant indices, or returns the same value for every index, and whose " +
            "enumerator only walks that indexer stands in for an array (#1369).");

    /// <summary>One run of decimal digits in a member name.</summary>
    private static readonly Regex DigitRun = new(@"\d+", RegexOptions.CultureInvariant);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [FamilyRule, IndexSwitchRule, HandWrittenListRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        context.RegisterSyntaxNodeAction(AnalyzeSwitchExpression, SyntaxKind.SwitchExpression);
        context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
    }

    /// <summary>Reports the numbered families and the hand-written list shape of one type.</summary>
    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct or TypeKind.Interface) || IsJustified(type))
            return;
        foreach (ImmutableArray<(ISymbol Member, int Ordinal)> family in Families(type))
        {
            ISymbol first = family[0].Member;
            ISymbol last = family[family.Length - 1].Member;
            context.ReportDiagnostic(Diagnostic.Create(FamilyRule, first.Locations[0], first.Name, last.Name, type.Name, family.Length));
        }
        if (HandWrittenListShape(type, context.CancellationToken) is { } shape)
            context.ReportDiagnostic(Diagnostic.Create(HandWrittenListRule, type.Locations[0], type.Name, shape));
    }

    /// <summary>The type's numbered families: same-typed source members whose names share a pattern and whose ordinals run gap-free from 0 or 1.</summary>
    private static IEnumerable<ImmutableArray<(ISymbol Member, int Ordinal)>> Families(INamedTypeSymbol type)
    {
        var groups = new Dictionary<(string Pattern, bool IsStatic), List<(ISymbol Member, int Ordinal)>>();
        foreach (ISymbol member in type.GetMembers())
        {
            if (member is not (IFieldSymbol or IPropertySymbol) || member.IsImplicitlyDeclared ||
                !member.Locations.Any(location => location.IsInSource) || member is IPropertySymbol { IsIndexer: true })
                continue;
            foreach ((string pattern, int ordinal) in Patterns(member.Name))
            {
                if (!groups.TryGetValue((pattern, member.IsStatic), out List<(ISymbol Member, int Ordinal)>? group))
                    groups[(pattern, member.IsStatic)] = group = [];
                group.Add((member, ordinal));
            }
        }
        foreach (List<(ISymbol Member, int Ordinal)> group in groups.Values)
        {
            if (group.Count < MinimumFamilySize || group.Any(entry => IsJustified(entry.Member)))
                continue;
            ITypeSymbol? memberType = MemberType(group[0].Member);
            if (group.Any(entry => !SymbolEqualityComparer.Default.Equals(MemberType(entry.Member), memberType)))
                continue;
            ImmutableArray<(ISymbol Member, int Ordinal)> ordered = [.. group.OrderBy(entry => entry.Ordinal)];
            if (IsGapFreeFromZeroOrOne(ordered.Select(entry => entry.Ordinal)))
                yield return ordered;
        }
    }

    /// <summary>Every name pattern of <paramref name="name"/>: one digit run replaced by a placeholder, with that run's value.</summary>
    private static IEnumerable<(string Pattern, int Ordinal)> Patterns(string name)
    {
        foreach (Match run in DigitRun.Matches(name))
        {
            if (run.Length > MaximumOrdinalDigits)
                continue;
            yield return (name.Substring(0, run.Index) + "#" + name.Substring(run.Index + run.Length),
                int.Parse(run.Value, System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    /// <summary>Whether the ordinals are distinct and consecutive, starting at 0 or 1.</summary>
    private static bool IsGapFreeFromZeroOrOne(IEnumerable<int> ordinals)
    {
        int[] sorted = [.. ordinals.OrderBy(ordinal => ordinal)];
        if (sorted.Length == 0 || sorted[0] is not (0 or 1))
            return false;
        for (int index = 1; index < sorted.Length; index++)
        {
            if (sorted[index] != sorted[index - 1] + 1)
                return false;
        }
        return true;
    }

    /// <summary>The declared type of a field or property.</summary>
    private static ITypeSymbol? MemberType(ISymbol member) => member switch
    {
        IFieldSymbol field => field.Type,
        IPropertySymbol property => property.Type,
        _ => null,
    };

    /// <summary>Whether <paramref name="symbol"/> or its containing type carries a justified numbered-identities attribute.</summary>
    private static bool IsJustified(ISymbol symbol)
    {
        for (ISymbol? current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingSymbol)
        {
            if (current.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == NumberedIdentitiesAttributeName))
                return true;
        }
        return false;
    }

    /// <summary>Reports a switch expression whose integer arms each yield one member of a numbered family.</summary>
    private static void AnalyzeSwitchExpression(SyntaxNodeAnalysisContext context)
    {
        var switchExpression = (SwitchExpressionSyntax)context.Node;
        var arms = new List<(long Index, ISymbol Member)>();
        foreach (SwitchExpressionArmSyntax arm in switchExpression.Arms)
        {
            if (arm.Pattern is DiscardPatternSyntax)
                continue;
            if (arm.Pattern is not ConstantPatternSyntax constant || arm.WhenClause is not null ||
                IntegerConstant(context.SemanticModel, constant.Expression, context.CancellationToken) is not { } index ||
                MemberOf(context.SemanticModel, arm.Expression, context.CancellationToken) is not { } member)
                return;
            arms.Add((index, member));
        }
        ReportIndexSwitch(context, arms);
    }

    /// <summary>Reports a switch statement whose integer sections each return or assign one member of a numbered family.</summary>
    private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
    {
        var switchStatement = (SwitchStatementSyntax)context.Node;
        var arms = new List<(long Index, ISymbol Member)>();
        foreach (SwitchSectionSyntax section in switchStatement.Sections)
        {
            if (section.Labels.Any(label => label is DefaultSwitchLabelSyntax))
                continue;
            if (section.Labels.Count != 1 || section.Labels[0] is not CaseSwitchLabelSyntax label ||
                IntegerConstant(context.SemanticModel, label.Value, context.CancellationToken) is not { } index ||
                SectionMember(context.SemanticModel, section, context.CancellationToken) is not { } member)
                return;
            arms.Add((index, member));
        }
        ReportIndexSwitch(context, arms);
    }

    /// <summary>The member a switch section returns or assigns in its first statement.</summary>
    private static ISymbol? SectionMember(SemanticModel model, SwitchSectionSyntax section, CancellationToken cancellation) =>
        section.Statements.FirstOrDefault() switch
        {
            ReturnStatementSyntax { Expression: { } returned } => MemberOf(model, returned, cancellation),
            ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment } =>
                MemberOf(model, assignment.Left, cancellation) ?? MemberOf(model, assignment.Right, cancellation),
            _ => null,
        };

    /// <summary>Reports arms that map consecutive indices to distinct members of one numbered family.</summary>
    private static void ReportIndexSwitch(SyntaxNodeAnalysisContext context, List<(long Index, ISymbol Member)> arms)
    {
        if (arms.Count < 2)
            return;
        (long Index, ISymbol Member)[] ordered = [.. arms.OrderBy(arm => arm.Index)];
        for (int position = 1; position < ordered.Length; position++)
        {
            if (ordered[position].Index != ordered[position - 1].Index + 1)
                return;
        }
        ISymbol[] members = [.. ordered.Select(arm => arm.Member)];
        if (members.Distinct(SymbolEqualityComparer.Default).Count() != members.Length ||
            members.Any(member => !SymbolEqualityComparer.Default.Equals(member.ContainingType, members[0].ContainingType)) ||
            members.Any(IsJustified))
            return;
        // The members must be one numbered family: some name pattern every one of them shares.
        IEnumerable<string> shared = Patterns(members[0].Name).Select(pattern => pattern.Pattern);
        foreach (ISymbol member in members.Skip(1))
            shared = shared.Intersect(Patterns(member.Name).Select(pattern => pattern.Pattern));
        if (!shared.Any())
            return;
        context.ReportDiagnostic(Diagnostic.Create(IndexSwitchRule, context.Node.GetLocation(),
            ordered[0].Index, ordered[ordered.Length - 1].Index, members[0].Name, members[members.Length - 1].Name));
    }

    /// <summary>The value of an integer constant expression, or null.</summary>
    private static long? IntegerConstant(SemanticModel model, ExpressionSyntax expression, CancellationToken cancellation)
    {
        Optional<object?> value = model.GetConstantValue(expression, cancellation);
        return value.HasValue ? value.Value switch
        {
            int number => number,
            long number => number,
            short number => number,
            ushort number => number,
            byte number => number,
            sbyte number => number,
            uint number => number,
            _ => null,
        } : null;
    }

    /// <summary>The field or property an expression reads or assigns, or null.</summary>
    private static ISymbol? MemberOf(SemanticModel model, ExpressionSyntax expression, CancellationToken cancellation) =>
        model.GetSymbolInfo(expression, cancellation).Symbol is { } symbol && symbol is IFieldSymbol or IPropertySymbol { IsIndexer: false }
            ? symbol
            : null;

    /// <summary>
    /// How the type's indexer stands in for an array, or null: it has a hand-written <c>GetEnumerator</c> and an integer
    /// indexer whose getter switches over constant indices or never reads its index.
    /// </summary>
    private static string? HandWrittenListShape(INamedTypeSymbol type, CancellationToken cancellation)
    {
        if (!type.GetMembers("GetEnumerator").Any(member => member.Locations.Any(location => location.IsInSource)))
            return null;
        foreach (IPropertySymbol indexer in type.GetMembers().OfType<IPropertySymbol>())
        {
            if (!indexer.IsIndexer || indexer.Parameters.Length != 1 || indexer.Parameters[0].Type.SpecialType != SpecialType.System_Int32)
                continue;
            string parameter = indexer.Parameters[0].Name;
            foreach (SyntaxReference reference in indexer.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax(cancellation) is not IndexerDeclarationSyntax declaration ||
                    GetterBody(declaration) is not { } getter)
                    continue;
                if (SwitchesOnIndex(getter, parameter))
                    return "switches over constant indices";
                if (!ReadsIndexBeyondBoundsCheck(getter, parameter))
                    return "returns the same value for every index";
            }
        }
        return null;
    }

    /// <summary>The getter of an indexer: its expression body, or its get accessor's body.</summary>
    private static SyntaxNode? GetterBody(IndexerDeclarationSyntax declaration)
    {
        if (declaration.ExpressionBody is { } arrow)
            return arrow.Expression;
        AccessorDeclarationSyntax? get = declaration.AccessorList?.Accessors
            .FirstOrDefault(accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration));
        return get?.ExpressionBody is { } getArrow ? getArrow.Expression : get?.Body;
    }

    /// <summary>
    /// Whether the getter switches over the index, or returns a value chosen by testing the index against a constant. A
    /// comparison inside a calculation does not count: a getter that computes its value from the index is not an array.
    /// </summary>
    private static bool SwitchesOnIndex(SyntaxNode getter, string parameter) =>
        getter.DescendantNodesAndSelf().Any(node => node switch
        {
            SwitchExpressionSyntax switchExpression => IsIdentifier(switchExpression.GoverningExpression, parameter) &&
                switchExpression.Arms.Count(arm => arm.Pattern is ConstantPatternSyntax) >= 2,
            SwitchStatementSyntax switchStatement => IsIdentifier(switchStatement.Expression, parameter) &&
                switchStatement.Sections.SelectMany(section => section.Labels).Count(label => label is CaseSwitchLabelSyntax) >= 2,
            _ => false,
        }) || Results(getter).Any(result => result is ConditionalExpressionSyntax conditional &&
            conditional.Condition is BinaryExpressionSyntax comparison && comparison.IsKind(SyntaxKind.EqualsExpression) &&
            (IsIdentifier(comparison.Left, parameter) && comparison.Right is LiteralExpressionSyntax ||
             IsIdentifier(comparison.Right, parameter) && comparison.Left is LiteralExpressionSyntax));

    /// <summary>The values the getter returns: its expression body, or its return statements' expressions.</summary>
    private static IEnumerable<ExpressionSyntax> Results(SyntaxNode getter) =>
        getter is ExpressionSyntax expression
            ? [expression]
            : getter.DescendantNodes().OfType<ReturnStatementSyntax>().Select(statement => statement.Expression).OfType<ExpressionSyntax>();

    /// <summary>
    /// Whether the getter reads its index anywhere but a bounds check: an <c>if</c> whose statement throws, or an argument
    /// of a call whose name begins with <c>Throw</c> or of an <c>Ensure</c> guard.
    /// </summary>
    private static bool ReadsIndexBeyondBoundsCheck(SyntaxNode getter, string parameter) =>
        getter.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(identifier =>
            identifier.Identifier.ValueText == parameter && !IsInBoundsCheck(identifier));

    /// <summary>Whether <paramref name="node"/> sits in a bounds check that throws rather than in the value the getter yields.</summary>
    private static bool IsInBoundsCheck(SyntaxNode node) => node.Ancestors().Any(ancestor => ancestor switch
    {
        ThrowStatementSyntax or ThrowExpressionSyntax => true,
        InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } } => true,
        IfStatementSyntax check => check.Condition.Span.Contains(node.Span) &&
            check.Statement.DescendantNodesAndSelf().Any(statement => statement is ThrowStatementSyntax or ThrowExpressionSyntax),
        InvocationExpressionSyntax call => call.ArgumentList.Span.Contains(node.Span) && call.Expression switch
        {
            MemberAccessExpressionSyntax access => access.Name.Identifier.ValueText.StartsWith("Throw", StringComparison.Ordinal) ||
                access.Expression is IdentifierNameSyntax { Identifier.ValueText: "Ensure" },
            _ => false,
        },
        _ => false,
    });

    /// <summary>Whether <paramref name="expression"/> is the identifier <paramref name="name"/>.</summary>
    private static bool IsIdentifier(ExpressionSyntax expression, string name) =>
        expression is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == name;
}
