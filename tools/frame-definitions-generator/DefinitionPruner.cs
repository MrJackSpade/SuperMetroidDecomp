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
/// conditional on either takes the branches that definition takes, and a lookup of the definition's entry in a parts table
/// of factories runs that entry alone. Whatever it cannot evaluate keeps every branch.
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
            // A factory's creations name each definition's own creation (#2553): only the definition's entry runs.
            case InitializerExpressionSyntax initializer when UnitScanner.IsCreate(_unit, _context.Behaviour) && initializer.Expressions.Count > 0
                && initializer.Expressions.All(entry => entry is AssignmentExpressionSyntax { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [_] } }):
            {
                DispatchConstant? definition = DispatchConstant.Of(_context.Definition);
                var taken = new List<SyntaxNode>();
                foreach (AssignmentExpressionSyntax entry in initializer.Expressions.Cast<AssignmentExpressionSyntax>())
                {
                    ExpressionSyntax key = ((ImplicitElementAccessSyntax)entry.Left).ArgumentList.Arguments[0].Expression;
                    if (Equal(Value(key), definition) != false)
                        taken.Add(entry.Right);
                }
                return taken;
            }
            // A factory's creations name the definitions each builds (#2554): a registrar's Spawns or Loads names the
            // definitions its creation builds, a part offer the one part its creation builds. Under another definition the
            // call runs only those names, never the creation, unless the creation offers that definition's own, as Mama
            // Turtle's creation offers her babies theirs.
            case InvocationExpressionSyntax { ArgumentList.Arguments: [var named, var creation, ..] } invocation
                when UnitScanner.IsCreate(_unit, _context.Behaviour) && CreationNames(invocation, named) is { } names:
                return names.Any(name => Equal(name, DispatchConstant.Of(_context.Definition)) != false) || OffersOwnDefinition(creation)
                    ? null
                    : [invocation.Expression, named];
            // Under a definition one of the factory's part offers builds, a block making that offer runs only the statements
            // making it (#2554): the part is built by the creation it is offered, not by the code building whoever offers it.
            case BlockSyntax block when UnitScanner.IsCreate(_unit, _context.Behaviour) && block.Statements.Any(OffersOwnDefinition):
                return [.. block.Statements.Where(OffersOwnDefinition)];
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
            // A factory looking its definition's entry up in its parts table of factories (#2559: Parts[definition] and the
            // factory it holds builds the definition's entity) runs that entry alone, as an if-chain's branch would.
            case ElementAccessExpressionSyntax { ArgumentList.Arguments: [var key] } lookup when OwnFactoryEntry(lookup, key) is { } entry:
                return [key.Expression, entry];
            default:
                return null;
        }
    }

    /// <summary>
    /// The value of the definition's own entry when <paramref name="lookup"/> reads it from the behaviour's parts table and
    /// that value is no part (an enum member, which <see cref="Value"/> evaluates) but code, such as the factory building
    /// the definition's entity; null for any other lookup.
    /// </summary>
    /// <exception cref="InvalidDataException">The entry is declared in another file than the lookup.</exception>
    private ExpressionSyntax? OwnFactoryEntry(ElementAccessExpressionSyntax lookup, ArgumentSyntax key)
    {
        if (_context.Behaviour.PartsTable is not { } table || _context.Part is not null
            || !SymbolEqualityComparer.Default.Equals(_semantic.GetSymbolInfo(lookup.Expression).Symbol?.OriginalDefinition, table)
            || Equal(Value(key.Expression), DispatchConstant.Of(_context.Definition)) != true
            || !_context.Behaviour.PartEntries.TryGetValue(_context.Definition.Name, out ExpressionSyntax? entry))
        {
            return null;
        }
        return entry.SyntaxTree == _semantic.SyntaxTree
            ? entry
            : throw new InvalidDataException(
                $"{_context.Behaviour.Type.ToDisplayString()}: the parts table entry for {_context.Definition.Name} is not declared in the file that looks it up.");
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
    /// The constant <paramref name="expression"/> is for the definition: the entity's own slot's definition (read directly or
    /// through a local holding it), the part the behaviour's parts table maps it to, or an enum member; null when it is none
    /// of these.
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
            // A factory naming its place's definition once (#2146: EnemyDefinitionId definition = place.DefinitionId, then an
            // if-chain building each definition's own entity) tests that definition wherever it reads the local.
            case IdentifierNameSyntax name when _semantic.GetSymbolInfo(name).Symbol is ILocalSymbol local && SingleValue(local) is { } value:
                return Value(value);
        }
        return _semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol member ? DispatchConstant.Of(member) : null;
    }

    /// <summary>
    /// The one value <paramref name="local"/> holds: its declaration's initializer, when nothing in the member declaring it
    /// assigns it again or takes it by reference; null for any other local.
    /// </summary>
    private ExpressionSyntax? SingleValue(ILocalSymbol local)
    {
        if (local.DeclaringSyntaxReferences is not [var reference]
            || reference.GetSyntax() is not VariableDeclaratorSyntax { Initializer.Value: var value }
            || reference.SyntaxTree != _semantic.SyntaxTree
            || value.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault() is not { } member)
            return null;
        foreach (SyntaxNode node in member.DescendantNodes())
        {
            ExpressionSyntax? written = node switch
            {
                AssignmentExpressionSyntax assignment => assignment.Left,
                PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.PreIncrementExpression or (int)SyntaxKind.PreDecrementExpression } prefix => prefix.Operand,
                PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.PostIncrementExpression or (int)SyntaxKind.PostDecrementExpression } postfix => postfix.Operand,
                ArgumentSyntax { RefKindKeyword.RawKind: not (int)SyntaxKind.None } argument => argument.Expression,
                _ => null,
            };
            if (written is not null && SymbolEqualityComparer.Default.Equals(_semantic.GetSymbolInfo(written).Symbol, local))
                return null;
        }
        return value;
    }

    /// <summary>
    /// Whether <paramref name="expression"/> is the slot the dispatched definition occupies, or the enemy holding it (#2558):
    /// an entity's own slot or occupant member or primary-constructor parameter, or the factory's slot or occupant parameter.
    /// </summary>
    private bool IsOwnSlot(ExpressionSyntax expression)
    {
        // Core's validations return the value they validate (Ensure.NotNull(place) is the place).
        while (expression is InvocationExpressionSyntax { ArgumentList.Arguments: [var validated, ..] } validation
               && _semantic.GetSymbolInfo(validation).Symbol is IMethodSymbol { ContainingType.Name: "Ensure" } method
               && _model.IsSource(method))
            expression = validated.Expression;
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
        if (type is null || !(SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _model.RoomEnemyPlace)
                || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _model.RoomEnemyOccupant)))
            return false;
        return symbol switch
        {
            IParameterSymbol { ContainingSymbol: IMethodSymbol owner } =>
                (owner.MethodKind == MethodKind.Constructor && InEntityChain(owner.ContainingType))
                || UnitScanner.IsCreate(Unit.Member(owner.OriginalDefinition), _context.Behaviour)
                // A creation in the factory's creations (#2553) is handed the factory's own slot.
                || (owner is { MethodKind: MethodKind.AnonymousFunction, ContainingSymbol: IMethodSymbol { AssociatedSymbol: IPropertySymbol creations } }
                    && UnitScanner.IsCreate(Unit.Member(creations.OriginalDefinition), _context.Behaviour))
                // So is a creation the factory hands the registrar or offers a part (#2554).
                || (owner is { MethodKind: MethodKind.AnonymousFunction, ContainingSymbol: IMethodSymbol creating }
                    && UnitScanner.IsCreate(Unit.Member(creating.OriginalDefinition), _context.Behaviour)),
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

    /// <summary>The registrar's creation calls, which name the definitions their creation builds.</summary>
    private static readonly string[] RegistrarCreationCalls = ["Spawns", "Loads"];

    /// <summary>The part offers, which name the one part their creation builds.</summary>
    private static readonly string[] PartOfferCalls = ["Offer", "OfferLoaded"];

    /// <summary>
    /// The definitions <paramref name="invocation"/>, a registrar's creation call or a part offer, names in its first
    /// argument <paramref name="named"/>, each as a constant or unknown (null); null for any other call, or one whose
    /// definitions are no list written in place nor one constant.
    /// </summary>
    private IReadOnlyList<DispatchConstant?>? CreationNames(InvocationExpressionSyntax invocation, ArgumentSyntax named)
    {
        if (_semantic.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { ContainingType.Name: var owner, Name: var name } method || !_model.IsSource(method))
            return null;
        if (owner == "EnemySpeciesRegistrar" && RegistrarCreationCalls.Contains(name) && named.Expression is CollectionExpressionSyntax list)
            return [.. list.Elements.Select(element => element is ExpressionElementSyntax { Expression: var definition } ? Value(definition) : null)];
        if (owner == "EnemyPartOffers" && PartOfferCalls.Contains(name))
            return [Value(named.Expression)];
        return null;
    }

    /// <summary>Whether <paramref name="node"/> makes a part offer naming the dispatched definition, or holds code that does.</summary>
    private bool OffersOwnDefinition(SyntaxNode node) =>
        node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Any(invocation =>
            invocation.ArgumentList.Arguments is [var named, _, ..]
            && _semantic.GetSymbolInfo(invocation).Symbol is IMethodSymbol { ContainingType.Name: "EnemyPartOffers", Name: var name } method
            && _model.IsSource(method) && PartOfferCalls.Contains(name)
            && Equal(Value(named.Expression), DispatchConstant.Of(_context.Definition)) == true);

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
