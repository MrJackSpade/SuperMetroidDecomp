using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>A constant an expression evaluates to: an enum member's value with its enum.</summary>
/// <param name="Type">The enum.</param>
/// <param name="Value">The member's constant value.</param>
internal sealed record DispatchConstant(INamedTypeSymbol Type, object Value)
{
    /// <summary>Whether the two constants are the same enum value.</summary>
    public bool Same(DispatchConstant other) => SymbolEqualityComparer.Default.Equals(Type, other.Type) && Equals(Value, other.Value);

    /// <summary>The constant of an enum member.</summary>
    public static DispatchConstant? Of(IFieldSymbol member) =>
        member is { HasConstantValue: true, ContainingType.TypeKind: TypeKind.Enum, ConstantValue: { } value }
            ? new DispatchConstant(member.ContainingType.OriginalDefinition, value)
            : null;
}

/// <summary>
/// Evaluates an entity's (or its factory's) dispatch for one definition: the entity's own slot holds that definition,
/// and the behaviour's parts table maps it to its part, so a switch over the part, a test of the slot's definition, or a
/// conditional on either takes the branches that definition takes. Whatever it cannot evaluate keeps every branch.
/// </summary>
internal sealed class DefinitionPruner
{
    private readonly CoreModel _model;
    private readonly SemanticModel _semantic;
    private readonly DefinitionContext _context;
    private readonly Unit _unit;

    /// <summary>Creates the pruner for <paramref name="unit"/>'s code under <paramref name="context"/>.</summary>
    public DefinitionPruner(CoreModel model, SemanticModel semantic, DefinitionContext context, Unit unit)
    {
        _model = model;
        _semantic = semantic;
        _context = context;
        _unit = unit;
    }

    /// <summary>The children of <paramref name="node"/> the definition's dispatch runs, or null when every child may run.</summary>
    public IReadOnlyList<SyntaxNode>? TakenChildren(SyntaxNode node)
    {
        switch (node)
        {
            case SwitchExpressionSyntax switchExpression when Value(switchExpression.GoverningExpression) is { } governing:
            {
                var taken = new List<SyntaxNode> { switchExpression.GoverningExpression };
                foreach (SwitchExpressionArmSyntax arm in switchExpression.Arms)
                {
                    bool? matches = Matches(governing, arm.Pattern);
                    if (arm.WhenClause is not null && matches == true)
                        matches = null;
                    if (matches == false)
                        continue;
                    if (arm.WhenClause is not null)
                        taken.Add(arm.WhenClause);
                    taken.Add(arm.Expression);
                    if (matches == true)
                        break;
                }
                return taken;
            }
            case SwitchStatementSyntax switchStatement when Value(switchStatement.Expression) is { } governing:
            {
                var taken = new List<SyntaxNode> { switchStatement.Expression };
                SwitchSectionSyntax? defaultSection = null;
                bool matched = false;
                foreach (SwitchSectionSyntax section in switchStatement.Sections)
                {
                    bool? sectionMatches = false;
                    foreach (SwitchLabelSyntax label in section.Labels)
                    {
                        bool? labelMatches = label switch
                        {
                            CaseSwitchLabelSyntax caseLabel => Equal(governing, Value(caseLabel.Value)),
                            CasePatternSwitchLabelSyntax patternLabel => patternLabel.WhenClause is null
                                ? Matches(governing, patternLabel.Pattern)
                                : Matches(governing, patternLabel.Pattern) == false ? false : null,
                            _ => false,
                        };
                        if (label is DefaultSwitchLabelSyntax)
                            defaultSection = section;
                        sectionMatches = Or(sectionMatches, labelMatches);
                    }
                    if (sectionMatches == false)
                        continue;
                    taken.AddRange(section.Labels.OfType<CasePatternSwitchLabelSyntax>().Select(l => l.WhenClause).OfType<SyntaxNode>());
                    taken.AddRange(section.Statements);
                    if (sectionMatches == true)
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched && defaultSection is not null && !(defaultSection.Statements.FirstOrDefault() is { } first && taken.Contains(first)))
                    taken.AddRange(defaultSection.Statements);
                return taken;
            }
            case ConditionalExpressionSyntax conditional when Truth(conditional.Condition) is { } truth:
                return [conditional.Condition, truth ? conditional.WhenTrue : conditional.WhenFalse];
            case IfStatementSyntax ifStatement when Truth(ifStatement.Condition) is { } truth:
                return truth
                    ? [ifStatement.Condition, ifStatement.Statement]
                    : ifStatement.Else is { } otherwise ? [ifStatement.Condition, otherwise.Statement] : [ifStatement.Condition];
            case BinaryExpressionSyntax conjunction when conjunction.IsKind(SyntaxKind.LogicalAndExpression) && Truth(conjunction.Left) == false:
                return [conjunction.Left];
            case BinaryExpressionSyntax disjunction when disjunction.IsKind(SyntaxKind.LogicalOrExpression) && Truth(disjunction.Left) == true:
                return [disjunction.Left];
            default:
                return null;
        }
    }

    /// <summary>Whether <paramref name="condition"/> holds for the definition, or null when it cannot be evaluated.</summary>
    private bool? Truth(ExpressionSyntax condition)
    {
        switch (condition)
        {
            case ParenthesizedExpressionSyntax parenthesized:
                return Truth(parenthesized.Expression);
            case PrefixUnaryExpressionSyntax negation when negation.IsKind(SyntaxKind.LogicalNotExpression):
                return Truth(negation.Operand) is { } inner ? !inner : null;
            case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.TrueLiteralExpression):
                return true;
            case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.FalseLiteralExpression):
                return false;
            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.EqualsExpression):
                return Equal(Value(binary.Left), Value(binary.Right));
            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.NotEqualsExpression):
                return Equal(Value(binary.Left), Value(binary.Right)) is { } equal ? !equal : null;
            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.LogicalAndExpression):
                return And(Truth(binary.Left), Truth(binary.Right));
            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.LogicalOrExpression):
                return Or(Truth(binary.Left), Truth(binary.Right));
            case IsPatternExpressionSyntax isPattern:
                return Value(isPattern.Expression) is { } value ? Matches(value, isPattern.Pattern) : null;
            default:
                return null;
        }
    }

    /// <summary>Whether <paramref name="pattern"/> matches <paramref name="value"/>, or null when it cannot be evaluated.</summary>
    private bool? Matches(DispatchConstant value, PatternSyntax pattern) => pattern switch
    {
        ConstantPatternSyntax constant => Equal(value, Value(constant.Expression)),
        BinaryPatternSyntax binary when binary.IsKind(SyntaxKind.OrPattern) => Or(Matches(value, binary.Left), Matches(value, binary.Right)),
        BinaryPatternSyntax binary when binary.IsKind(SyntaxKind.AndPattern) => And(Matches(value, binary.Left), Matches(value, binary.Right)),
        UnaryPatternSyntax unary when unary.IsKind(SyntaxKind.NotPattern) => Matches(value, unary.Pattern) is { } inner ? !inner : null,
        ParenthesizedPatternSyntax parenthesized => Matches(value, parenthesized.Pattern),
        DiscardPatternSyntax => true,
        VarPatternSyntax => true,
        _ => null,
    };

    /// <summary>
    /// The constant <paramref name="expression"/> is for the definition: the entity's own slot's definition, the part the
    /// behaviour's parts table maps it to, or an enum member; null when it is none of these.
    /// </summary>
    private DispatchConstant? Value(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case ParenthesizedExpressionSyntax parenthesized:
                return Value(parenthesized.Expression);
            case MemberAccessExpressionSyntax { Name.Identifier.Text: "DefinitionId" } access when IsOwnSlot(access.Expression):
                return DispatchConstant.Of(_context.Definition);
            case ElementAccessExpressionSyntax { ArgumentList.Arguments: [var key] } element
                when _context.Behaviour.PartsTable is { } table && _context.Part is { } part
                     && SymbolEqualityComparer.Default.Equals(_semantic.GetSymbolInfo(element.Expression).Symbol?.OriginalDefinition, table)
                     && Equal(Value(key.Expression), DispatchConstant.Of(_context.Definition)) == true:
                return DispatchConstant.Of(part);
        }
        return _semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol member ? DispatchConstant.Of(member) : null;
    }

    /// <summary>
    /// Whether <paramref name="expression"/> is the slot the dispatched definition occupies: an entity's own slot member
    /// or primary-constructor slot, or the factory's slot parameter.
    /// </summary>
    private bool IsOwnSlot(ExpressionSyntax expression)
    {
        if (expression is MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } thisAccess)
            expression = thisAccess.Name;
        ISymbol? symbol = _semantic.GetSymbolInfo(expression).Symbol;
        ITypeSymbol? type = symbol switch
        {
            IPropertySymbol { IsStatic: false } property => property.Type,
            IFieldSymbol { IsStatic: false } field => field.Type,
            IParameterSymbol parameter => parameter.Type,
            _ => null,
        };
        if (type is null || !SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _model.RoomEnemySlot))
            return false;
        return symbol switch
        {
            IParameterSymbol { ContainingSymbol: IMethodSymbol owner } =>
                (owner.MethodKind == MethodKind.Constructor && InEntityChain(owner.ContainingType))
                || UnitScanner.IsCreate(Unit.Member(owner.OriginalDefinition), _context.Behaviour),
            _ => InEntityChain(symbol!.ContainingType),
        };
    }

    /// <summary>Whether <paramref name="type"/> is the unit's entity type or one of its base types.</summary>
    private bool InEntityChain(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = _unit.Type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, type.OriginalDefinition))
                return _model.IsEntityType(_unit.Type);
        }
        return false;
    }

    /// <summary>Whether two constants are the same value, or null when either is unknown.</summary>
    private static bool? Equal(DispatchConstant? left, DispatchConstant? right) =>
        left is null || right is null ? null : left.Same(right);

    /// <summary>Three-valued conjunction.</summary>
    private static bool? And(bool? left, bool? right) =>
        left == false || right == false ? false : left == true && right == true ? true : null;

    /// <summary>Three-valued disjunction.</summary>
    private static bool? Or(bool? left, bool? right) =>
        left == true || right == true ? true : left == false && right == false ? false : null;
}
