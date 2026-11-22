using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// The motion each enemy projectile kind starts with (#1843): a projectile behaviour deriving from
/// <c>EnemyProjectileMotionBehavior&lt;TMotion&gt;</c> owns the motions <c>TMotion</c> names and starts each projectile it is
/// bound to in its kind's motion, which its base constructor call names either as a table naming every kind it serves
/// (<c>startingMotions</c>), or as one motion for every kind it is bound to (<c>initial</c>), passed down through its own
/// constructors' parameters from the construction that makes it. A table names its kinds outright; one motion reaches the
/// kinds the spawners bind that construction's behaviour to, which the projectile's construction names (through the spawn
/// service handing the constructor its own parameters). A kind several spawners bind to different behaviours starts with
/// any of their motions.
/// </summary>
internal sealed class StartingMotions
{
    private readonly CoreModel _model;
    private readonly HashSet<INamedTypeSymbol> _motionTypes = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<IFieldSymbol, HashSet<IFieldSymbol>> _kindMotions = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<BaseObjectCreationExpressionSyntax, Start> _constructions = [];
    private readonly HashSet<SyntaxNode> _values = [];

    /// <summary>Reads every motion type, every behaviour's starting table or motion, and every binding of a kind to a behaviour.</summary>
    /// <exception cref="InvalidDataException">A table, a starting motion or a bound behaviour cannot be read.</exception>
    public StartingMotions(CoreModel model)
    {
        _model = model;
        foreach (INamedTypeSymbol type in model.Types)
        {
            if (MotionTypeOf(type) is { } motionType)
                _motionTypes.Add(motionType);
        }
        if (_motionTypes.Count == 0)
            throw new InvalidDataException($"No Core type derives from {model.EnemyProjectileMotionBehavior.ToDisplayString()} with a motion type.");
        // A behaviour whose constructors name a table names its kinds' motions outright, whoever constructs it.
        foreach (INamedTypeSymbol type in model.Types.Where(type => type.TypeKind == TypeKind.Class && MotionTypeOf(type) is not null))
        {
            foreach (IMethodSymbol constructor in GameConstructors.Of(type))
            {
                if (Resolve(constructor, _ => Argument.Unbound, depth: 0) is { Table: { } table })
                    AddTable(table);
            }
        }
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (BaseObjectCreationExpressionSyntax creation in tree.GetRoot().DescendantNodes().OfType<BaseObjectCreationExpressionSyntax>())
            {
                semantic ??= model.ModelOf(tree);
                if (semantic.GetSymbolInfo(creation).Symbol is not IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
                    || MotionTypeOf(constructor.ContainingType) is null || GameConstructors.IsRestore(constructor))
                    continue;
                SemanticModel bound = semantic;
                _constructions.Add(creation, Resolve(constructor, parameter => Argument.Of(creation.ArgumentList, parameter, bound), depth: 0));
            }
        }
        // A projectile is bound to its kind and its behaviour as it is built (#2554): its one constructor takes both.
        IMethodSymbol bind = model.EnemyProjectile.InstanceConstructors.SingleOrDefault()
            ?? throw new InvalidDataException($"{model.EnemyProjectile.Name} declares no one constructor binding its kind and behaviour.");
        IParameterSymbol kindParameter = bind.Parameters.Single(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type, model.RoomEnemyProjectileKind));
        IParameterSymbol behaviourParameter = bind.Parameters.Single(parameter => model.IsProjectileBehaviour((INamedTypeSymbol)parameter.Type));
        ReadBindings(bind, kindParameter, behaviourParameter, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));
    }

    /// <summary>Every family motion type: the motion type argument of each behaviour deriving from <c>EnemyProjectileMotionBehavior&lt;TMotion&gt;</c>.</summary>
    public IReadOnlySet<INamedTypeSymbol> MotionTypes => _motionTypes;

    /// <summary>Whether <paramref name="field"/> is a motion: a member of a family motion type.</summary>
    public bool IsMotion(IFieldSymbol field) => _motionTypes.Contains(field.ContainingType.OriginalDefinition);

    /// <summary>The motions a projectile of <paramref name="kind"/> may start with; empty when it starts holding still.</summary>
    public IReadOnlySet<IFieldSymbol> Of(IFieldSymbol kind) =>
        _kindMotions.TryGetValue(kind.OriginalDefinition, out HashSet<IFieldSymbol>? motions) ? motions : new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);

    /// <summary>
    /// Whether <paramref name="node"/> is, or is within, a starting motion this reader took as a kind's: a starting table, or
    /// a motion handed down to a behaviour's one starting motion. Naming a motion there gives it to no object at run time.
    /// </summary>
    public bool IsStartingValue(SyntaxNode node) => node.AncestorsAndSelf().Any(_values.Contains);

    /// <summary>
    /// The motion type <paramref name="type"/>'s base chain hands <c>EnemyProjectileMotionBehavior&lt;TMotion&gt;</c>; null when it
    /// derives from none or hands it a type parameter.
    /// </summary>
    private INamedTypeSymbol? MotionTypeOf(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, _model.EnemyProjectileMotionBehavior))
                return current.TypeArguments[0] is INamedTypeSymbol { TypeKind: TypeKind.Enum } motion ? motion.OriginalDefinition : null;
        }
        return null;
    }

    /// <summary>
    /// What the construction calling <paramref name="constructor"/> with <paramref name="argumentFor"/>'s arguments starts its
    /// kinds with: followed through each constructor's base call (a constructor initializer, a primary constructor's base
    /// call, or the implicit call of the base's parameterless constructor) to <c>EnemyProjectileMotionBehavior&lt;TMotion&gt;</c>'s.
    /// </summary>
    private Start Resolve(IMethodSymbol constructor, Func<IParameterSymbol, Argument> argumentFor, int depth)
    {
        if (depth > 16)
            throw new InvalidDataException($"{constructor.ToDisplayString()}'s base calls nest deeper than this generator follows.");
        if (SymbolEqualityComparer.Default.Equals(constructor.ContainingType.OriginalDefinition, _model.EnemyProjectileMotionBehavior))
            return Started(constructor, argumentFor);
        (IMethodSymbol baseConstructor, ArgumentListSyntax? arguments, SemanticModel? semantic) = BaseCall(constructor);
        IMethodSymbol declared = constructor.OriginalDefinition;
        return Resolve(baseConstructor, parameter =>
        {
            ExpressionSyntax? passed = arguments is null ? null : CallSite.ArgumentFor(arguments, parameter);
            if (passed is null)
                return Argument.Default;
            // A parameter of this constructor passed on takes what its own caller passed, or its own default.
            if (semantic!.GetSymbolInfo(passed).Symbol is IParameterSymbol handed
                && SymbolEqualityComparer.Default.Equals(handed.ContainingSymbol.OriginalDefinition, declared))
            {
                Argument outer = argumentFor(handed);
                return outer is { Bound: true, Expression: null } ? DefaultOf(handed) : outer;
            }
            return new Argument(passed, semantic, Bound: true);
        }, depth + 1);
    }

    /// <summary>
    /// The starting motions <c>EnemyProjectileMotionBehavior&lt;TMotion&gt;</c>'s <paramref name="constructor"/> is handed: its table,
    /// or its one motion (none when the kinds start holding still).
    /// </summary>
    private Start Started(IMethodSymbol constructor, Func<IParameterSymbol, Argument> argumentFor)
    {
        foreach (IParameterSymbol parameter in constructor.Parameters)
        {
            Argument argument = argumentFor(parameter);
            if (parameter.Type is INamedTypeSymbol { Name: "IReadOnlyDictionary" })
            {
                if (argument is not { Bound: true, Expression: { } table, Semantic: { } semantic })
                    return Start.Unknown;
                if (semantic.GetSymbolInfo(table).Symbol is not { } member || member is not (IFieldSymbol or IPropertySymbol) || !_model.IsSource(member))
                    throw new InvalidDataException($"{_model.RelativePath(table.SyntaxTree)}: the starting-motion table {table} is no Core field or property.");
                return new Start(CoreModel.InitializerOf(member.OriginalDefinition), null);
            }
            if (parameter.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                if (!argument.Bound)
                    return Start.Unknown;
                var motions = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
                if (argument is { Expression: { } initial, Semantic: { } semantic })
                {
                    foreach (ExpressionSyntax choice in CoreModel.Choices(initial))
                    {
                        if (choice is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression or (int)SyntaxKind.DefaultLiteralExpression })
                            continue;
                        if (semantic.GetSymbolInfo(choice).Symbol is not IFieldSymbol motion || !IsMotion(motion))
                            return Start.Unknown;
                        motions.Add(motion.OriginalDefinition);
                        _values.Add(choice);
                    }
                }
                return new Start(null, motions);
            }
        }
        throw new InvalidDataException($"{constructor.ToDisplayString()} takes neither a starting-motion table nor a starting motion.");
    }

    /// <summary>The base constructor <paramref name="constructor"/> calls, with the arguments its call passes and their semantic model.</summary>
    private (IMethodSymbol Constructor, ArgumentListSyntax? Arguments, SemanticModel? Semantic) BaseCall(IMethodSymbol constructor)
    {
        foreach (SyntaxReference reference in constructor.OriginalDefinition.DeclaringSyntaxReferences)
        {
            SyntaxNode declaration = reference.GetSyntax();
            SemanticModel semantic = _model.ModelOf(declaration.SyntaxTree);
            (SyntaxNode? call, ArgumentListSyntax? arguments) = declaration switch
            {
                ConstructorDeclarationSyntax { Initializer: { } initializer } => (initializer, initializer.ArgumentList),
                TypeDeclarationSyntax type when type.BaseList?.Types.OfType<PrimaryConstructorBaseTypeSyntax>().FirstOrDefault() is { } baseCall =>
                    (baseCall, baseCall.ArgumentList),
                _ => ((SyntaxNode?)null, (ArgumentListSyntax?)null),
            };
            if (call is not null)
            {
                return semantic.GetSymbolInfo(call).Symbol is IMethodSymbol called
                    ? (called, arguments, semantic)
                    : throw new InvalidDataException($"{_model.RelativePath(call.SyntaxTree)}: {call} calls no one constructor.");
            }
        }
        // No base call written: the base's constructor taking no arguments.
        INamedTypeSymbol baseType = constructor.ContainingType.BaseType
            ?? throw new InvalidDataException($"{constructor.ContainingType.ToDisplayString()} has no base type.");
        IMethodSymbol[] parameterless = [.. GameConstructors.Of(baseType).Where(candidate => candidate.Parameters.All(parameter => parameter.IsOptional))];
        return parameterless is [var implicitBase]
            ? (implicitBase, null, null)
            : throw new InvalidDataException($"{constructor.ToDisplayString()} calls none of {baseType.ToDisplayString()}'s constructors this generator can tell.");
    }

    /// <summary>What a parameter left to its default passes: its declared default value, or nothing for a default of null.</summary>
    private Argument DefaultOf(IParameterSymbol parameter)
    {
        foreach (SyntaxReference reference in parameter.OriginalDefinition.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is ParameterSyntax { Default.Value: var value })
                return new Argument(value, _model.ModelOf(value.SyntaxTree), Bound: true);
        }
        return Argument.Default;
    }

    /// <summary>Reads a starting-motion table's <c>[Kind] = Motion</c> entries, a null motion holding still.</summary>
    /// <exception cref="InvalidDataException">An entry is keyed by no kind or names no motion.</exception>
    private void AddTable(ExpressionSyntax table)
    {
        if (!_values.Add(table))
            return;
        SemanticModel semantic = _model.ModelOf(table.SyntaxTree);
        int entries = 0;
        foreach (AssignmentExpressionSyntax entry in table.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (entry.Left is not ImplicitElementAccessSyntax { ArgumentList.Arguments: [var key] })
                continue;
            entries++;
            if (semantic.GetSymbolInfo(key.Expression).Symbol is not IFieldSymbol kind
                || !SymbolEqualityComparer.Default.Equals(kind.ContainingType, _model.RoomEnemyProjectileKind))
                throw new InvalidDataException($"{_model.RelativePath(table.SyntaxTree)}: a starting-motion table keys an entry by {key}, which is no projectile kind.");
            if (entry.Right is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression })
                continue;
            if (semantic.GetSymbolInfo(entry.Right).Symbol is not IFieldSymbol motion || !IsMotion(motion))
                throw new InvalidDataException($"{_model.RelativePath(table.SyntaxTree)}: the starting-motion entry for {kind.Name} names no motion: {entry.Right}");
            AddKind(kind, [motion.OriginalDefinition]);
        }
        if (entries == 0)
            throw new InvalidDataException($"{_model.RelativePath(table.SyntaxTree)}: a starting-motion table names no kind: {table}");
    }

    /// <summary>Records that <paramref name="kind"/> may start with each of <paramref name="motions"/>.</summary>
    private void AddKind(IFieldSymbol kind, IEnumerable<IFieldSymbol> motions)
    {
        if (!_kindMotions.TryGetValue(kind.OriginalDefinition, out HashSet<IFieldSymbol>? known))
            _kindMotions.Add(kind.OriginalDefinition, known = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default));
        known.UnionWith(motions);
    }

    /// <summary>
    /// Reads every call of <paramref name="method"/>, which binds a kind (<paramref name="kindParameter"/>) to a behaviour
    /// (<paramref name="behaviourParameter"/>): a call handing on both of its own method's parameters is followed to that
    /// method's calls; any other call's kinds take the one motion each construction its behaviour can be starts with.
    /// </summary>
    private void ReadBindings(IMethodSymbol method, IParameterSymbol kindParameter, IParameterSymbol behaviourParameter, HashSet<IMethodSymbol> visiting)
    {
        if (!visiting.Add(method.OriginalDefinition))
            return;
        foreach (CallSite site in _model.CallSites(method))
        {
            ExpressionSyntax kind = site.ArgumentFor(kindParameter)
                ?? throw new InvalidDataException($"A call of {method.ToDisplayString()} passes no kind.");
            ExpressionSyntax behaviour = site.ArgumentFor(behaviourParameter)
                ?? throw new InvalidDataException($"A call of {method.ToDisplayString()} passes no behaviour.");
            if (_model.HandedParameter(site.Semantic.GetSymbolInfo(kind).Symbol) is { ContainingSymbol: IMethodSymbol kindHolder } handedKind
                && _model.HandedParameter(site.Semantic.GetSymbolInfo(behaviour).Symbol) is { ContainingSymbol: IMethodSymbol behaviourHolder } handedBehaviour
                && SymbolEqualityComparer.Default.Equals(kindHolder.OriginalDefinition, behaviourHolder.OriginalDefinition))
            {
                ReadBindings(kindHolder, handedKind, handedBehaviour, visiting);
                continue;
            }
            IFieldSymbol[] motions = [.. Constructions(behaviour, site.Semantic, new HashSet<ISymbol>(SymbolEqualityComparer.Default))
                .SelectMany(creation => _constructions.TryGetValue(creation, out Start start) ? start.Motions(creation, _model) : [])];
            if (motions.Length == 0)
                continue;
            foreach (string name in KindsOf(kind, site.Semantic))
                AddKind(_model.RoomEnemyProjectileKind.GetMembers(name).OfType<IFieldSymbol>().Single(), motions);
        }
    }

    /// <summary>
    /// The kinds <paramref name="kind"/> can be: what the tracer reads from it, through the values a local holding it is given
    /// (the Ceres debris choosing its dark or light kind), what each call passes for a parameter holding it, and what each
    /// implementation of a property holding it gives (a Fune's or Namihe's fireball, from the spitter's state).
    /// </summary>
    /// <exception cref="InvalidDataException">Some value is no kind the tracer can follow.</exception>
    private HashSet<string> KindsOf(ExpressionSyntax kind, SemanticModel semantic)
    {
        var kinds = new HashSet<string>(StringComparer.Ordinal);
        foreach ((ExpressionSyntax value, SemanticModel valueSemantic) in KindValues(kind, semantic, new HashSet<ISymbol>(SymbolEqualityComparer.Default)))
        {
            kinds.UnionWith(new ProgramTracer(_model, reaches: null).Programs(_model.RoomEnemyProjectileKind, value, valueSemantic)
                ?? throw new InvalidDataException($"{_model.RelativePath(kind.SyntaxTree)}: a behaviour starting in a motion is bound to kind {value}, which this generator cannot trace."));
        }
        return kinds;
    }

    /// <summary>
    /// The expressions <paramref name="kind"/> takes its kind from: itself, or, for a local, a parameter or a source property
    /// holding it, the values given to that holder, followed on to theirs.
    /// </summary>
    /// <exception cref="InvalidDataException">A call passes no kind for a parameter holding it.</exception>
    private IEnumerable<(ExpressionSyntax Value, SemanticModel Semantic)> KindValues(ExpressionSyntax kind, SemanticModel semantic, HashSet<ISymbol> visiting)
    {
        ISymbol? holder = semantic.GetSymbolInfo(kind).Symbol;
        IEnumerable<(ExpressionSyntax Value, SemanticModel Semantic)>? given = holder switch
        {
            ILocalSymbol local => ValuesOf(local),
            IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: not (MethodKind.AnonymousFunction or MethodKind.LocalFunction) } method } parameter =>
                [.. _model.CallSites(method).Select(site => (site.ArgumentFor(parameter)
                    ?? throw new InvalidDataException($"A call of {method.ToDisplayString()} passes no kind."), site.Semantic))],
            IPropertySymbol { IsAbstract: true } property when _model.IsSource(property) =>
                [.. _model.Implementations(property).SelectMany(ValuesOf)],
            _ => null,
        };
        if (given is null)
        {
            yield return (kind, semantic);
            yield break;
        }
        if (!visiting.Add(holder!))
            yield break;
        foreach ((ExpressionSyntax value, SemanticModel valueSemantic) in given)
        {
            foreach ((ExpressionSyntax Value, SemanticModel Semantic) found in KindValues(value, valueSemantic, visiting))
                yield return found;
        }
    }

    /// <summary>
    /// The constructions <paramref name="expression"/>, a behaviour, can be: a construction itself; a field's, property's or
    /// local's initializer or assigned values (a cached property's <c>field ??= new ...</c> among them); every construction
    /// of the containing type for <c>this</c>; for a parameter, what each call passes; for a call of one of Core's own
    /// methods, what that method returns, where a parameter it returns (<c>Ensure.NotNull(rocks)</c>'s value) is what that
    /// call passes for it (<paramref name="returnedFrom"/>), not what every call of the method passes.
    /// </summary>
    /// <exception cref="InvalidDataException">The behaviour is none of these.</exception>
    private IEnumerable<BaseObjectCreationExpressionSyntax> Constructions(ExpressionSyntax expression, SemanticModel semantic, HashSet<ISymbol> visiting,
        ReturningCall? returnedFrom = null)
    {
        foreach (ExpressionSyntax choice in CoreModel.Choices(expression))
        {
            switch (choice)
            {
                case BaseObjectCreationExpressionSyntax creation:
                    yield return creation;
                    continue;
                // No behaviour handed in is none.
                case LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression }:
                    continue;
                case AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.CoalesceAssignmentExpression } cached:
                    foreach (BaseObjectCreationExpressionSyntax creation in Constructions(cached.Right, semantic, visiting, returnedFrom))
                        yield return creation;
                    continue;
                // A behaviour handed in, or this one when none is (Spore Spawn's spores' own behaviour), is either.
                case BinaryExpressionSyntax { RawKind: (int)SyntaxKind.CoalesceExpression } either:
                    foreach (BaseObjectCreationExpressionSyntax creation in Constructions(either.Left, semantic, visiting, returnedFrom)
                        .Concat(Constructions(either.Right, semantic, visiting, returnedFrom)))
                        yield return creation;
                    continue;
                case ThisExpressionSyntax:
                    INamedTypeSymbol self = semantic.GetEnclosingSymbol(choice.SpanStart)?.ContainingType
                        ?? throw new InvalidDataException($"{_model.RelativePath(choice.SyntaxTree)}: this outside every type.");
                    foreach (BaseObjectCreationExpressionSyntax creation in _constructions.Keys.Where(creation =>
                        _model.ModelOf(creation.SyntaxTree).GetSymbolInfo(creation).Symbol is IMethodSymbol made
                        && SymbolEqualityComparer.Default.Equals(made.ContainingType.OriginalDefinition, self.OriginalDefinition)))
                        yield return creation;
                    continue;
            }
            ISymbol? symbol = semantic.GetSymbolInfo(choice).Symbol;
            // A parameter of the method this value is returned from is what the call returning it passes for it.
            if (returnedFrom is not null && symbol is IParameterSymbol returnedParameter
                && SymbolEqualityComparer.Default.Equals(returnedParameter.ContainingSymbol.OriginalDefinition, returnedFrom.Method.OriginalDefinition))
            {
                if (CallSite.ArgumentFor(returnedFrom.Arguments, returnedParameter.OriginalDefinition) is { } passed)
                {
                    foreach (BaseObjectCreationExpressionSyntax creation in Constructions(passed, returnedFrom.Semantic, visiting, returnedFrom.Outer))
                        yield return creation;
                }
                continue;
            }
            if (symbol is null || !visiting.Add(symbol.OriginalDefinition))
            {
                if (symbol is null)
                    throw new InvalidDataException($"{_model.RelativePath(choice.SyntaxTree)}: the bound behaviour {choice} binds to nothing.");
                continue;
            }
            IEnumerable<(ExpressionSyntax Value, SemanticModel Semantic)> values = symbol switch
            {
                IParameterSymbol parameter when _model.HandedParameter(parameter) is { ContainingSymbol: IMethodSymbol callee } handed =>
                    _model.CallSites(callee).Select(site => (site.ArgumentFor(handed), site.Semantic))
                        .Where(value => value.Item1 is not null).Select(value => (value.Item1!, value.Semantic)),
                IFieldSymbol or IPropertySymbol or ILocalSymbol when _model.IsSource(symbol) => ValuesOf(symbol),
                IMethodSymbol called when choice is InvocationExpressionSyntax && _model.IsSource(called) => ReturnsOf(called),
                _ => throw new InvalidDataException($"{_model.RelativePath(choice.SyntaxTree)}: this generator cannot tell which behaviour {choice} is."),
            };
            // A value a called method returns is read with that call's arguments for the method's parameters.
            ReturningCall? call = symbol is IMethodSymbol returning && choice is InvocationExpressionSyntax invocation
                ? new ReturningCall(returning, invocation.ArgumentList, semantic, returnedFrom)
                : null;
            foreach ((ExpressionSyntax value, SemanticModel valueSemantic) in values)
            {
                foreach (BaseObjectCreationExpressionSyntax creation in Constructions(value, valueSemantic, visiting, call))
                    yield return creation;
            }
        }
    }

    /// <summary>What <paramref name="method"/>, one of Core's own, returns: its expression body or each of its return statements' values.</summary>
    /// <exception cref="InvalidDataException">It returns nothing this generator can read.</exception>
    private List<(ExpressionSyntax Value, SemanticModel Semantic)> ReturnsOf(IMethodSymbol method)
    {
        var values = new List<(ExpressionSyntax, SemanticModel)>();
        foreach (SyntaxReference reference in method.OriginalDefinition.DeclaringSyntaxReferences)
        {
            SyntaxNode declaration = reference.GetSyntax();
            SemanticModel semantic = _model.ModelOf(declaration.SyntaxTree);
            if (declaration is BaseMethodDeclarationSyntax { ExpressionBody.Expression: var body })
            {
                values.Add((body, semantic));
                continue;
            }
            foreach (ReturnStatementSyntax returned in declaration.DescendantNodes().OfType<ReturnStatementSyntax>())
            {
                if (returned.Expression is { } value)
                    values.Add((value, semantic));
            }
        }
        if (values.Count == 0)
            throw new InvalidDataException($"{method.ToDisplayString()} returns no behaviour this generator can read.");
        return values;
    }

    /// <summary>The values <paramref name="member"/>, a field, property or local, is given: its initializer and every assignment to it.</summary>
    /// <exception cref="InvalidDataException">It is given none.</exception>
    private List<(ExpressionSyntax Value, SemanticModel Semantic)> ValuesOf(ISymbol member)
    {
        var values = new List<(ExpressionSyntax, SemanticModel)>();
        foreach (SyntaxReference reference in member.OriginalDefinition.DeclaringSyntaxReferences)
        {
            SyntaxNode declaration = reference.GetSyntax();
            ExpressionSyntax? initial = declaration switch
            {
                PropertyDeclarationSyntax { Initializer.Value: var value } => value,
                PropertyDeclarationSyntax { ExpressionBody.Expression: var body } => body,
                VariableDeclaratorSyntax { Initializer.Value: var value } => value,
                _ => null,
            };
            if (initial is not null)
                values.Add((initial, _model.ModelOf(initial.SyntaxTree)));
            // Assignments are searched where the member can be assigned: its type for a field or property, its method for a local.
            SyntaxNode? scope = member is ILocalSymbol
                ? declaration.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault()
                : declaration.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if (scope is null)
                continue;
            SemanticModel semantic = _model.ModelOf(scope.SyntaxTree);
            foreach (AssignmentExpressionSyntax assignment in scope.DescendantNodes().OfType<AssignmentExpressionSyntax>())
            {
                if (assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
                    && SymbolEqualityComparer.Default.Equals(semantic.GetSymbolInfo(assignment.Left).Symbol?.OriginalDefinition, member.OriginalDefinition))
                    values.Add((assignment.Right, semantic));
            }
        }
        return values.Count > 0
            ? values
            : throw new InvalidDataException($"{member.ToDisplayString()} is given no behaviour this generator can read.");
    }

    /// <summary>What a constructor argument passes.</summary>
    /// <param name="Expression">The expression passed; null when the parameter is left to its default.</param>
    /// <param name="Semantic">The semantic model the expression binds in.</param>
    /// <param name="Bound">Whether the argument is known: false for a parameter no construction decides here.</param>
    private readonly record struct Argument(ExpressionSyntax? Expression, SemanticModel? Semantic, bool Bound)
    {
        /// <summary>A parameter no construction decides here.</summary>
        public static Argument Unbound => new(null, null, Bound: false);

        /// <summary>A parameter left to its default.</summary>
        public static Argument Default => new(null, null, Bound: true);

        /// <summary>What <paramref name="arguments"/>, binding in <paramref name="semantic"/>, pass for <paramref name="parameter"/>.</summary>
        public static Argument Of(ArgumentListSyntax? arguments, IParameterSymbol parameter, SemanticModel semantic) =>
            arguments is not null && CallSite.ArgumentFor(arguments, parameter) is { } passed ? new Argument(passed, semantic, Bound: true) : Default;
    }

    /// <summary>What a construction starts its kinds with.</summary>
    /// <param name="Table">The starting-motion table naming each kind's motion, when one does.</param>
    /// <param name="Initial">The one motion every kind starts with (empty when they start holding still), when one does.</param>
    /// <summary>
    /// The call a value being read is returned from: the called method, the arguments the call passes it, the call's model,
    /// and the call the call itself is returned from, when it is.
    /// </summary>
    private sealed record ReturningCall(IMethodSymbol Method, ArgumentListSyntax Arguments, SemanticModel Semantic, ReturningCall? Outer);

    private readonly record struct Start(ExpressionSyntax? Table, IReadOnlySet<IFieldSymbol>? Initial)
    {
        /// <summary>A start a parameter no construction decides here hands down.</summary>
        public static Start Unknown => new(null, null);

        /// <summary>
        /// The one motion <paramref name="creation"/>'s kinds start with; none for a table, whose kinds it names outright.
        /// </summary>
        /// <exception cref="InvalidDataException">The construction's start is unknown.</exception>
        public IEnumerable<IFieldSymbol> Motions(BaseObjectCreationExpressionSyntax creation, CoreModel model) =>
            Table is not null ? []
                : Initial ?? throw new InvalidDataException($"{model.RelativePath(creation.SyntaxTree)}: the starting motion of {creation} is not one this generator can read.");
    }
}
