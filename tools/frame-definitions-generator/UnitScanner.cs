using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>One call that is handed the graphics a spawned object draws with, and the enum members its other arguments name.</summary>
/// <param name="Targets">Every sheet the spawn's graphics can make it draw from.</param>
/// <param name="Arguments">The enum members the call's other arguments name.</param>
/// <param name="Callee">The method called.</param>
/// <param name="Graphics">The graphics expression, as written, for the self-check's report.</param>
/// <param name="Copied">Whether the graphics are read from a settable instance member: another object's graphics, copied.</param>
/// <param name="OpenSpriteObjectKind">
/// Whether the call spawns a room sprite object whose kind its argument names without a constant: a kind other code
/// chose, such as one a cue or frame table carries.
/// </param>
internal sealed record SpawnCall(IReadOnlyList<SpawnTarget> Targets, IReadOnlySet<IFieldSymbol> Arguments, IMethodSymbol Callee, string Graphics, bool Copied,
    bool OpenSpriteObjectKind);

/// <summary>What one unit's code references: the units it may run and the enum members it names.</summary>
internal sealed class UnitScan
{
    /// <summary>The units the code may run: members it calls, reads, constructs or takes as method groups, and initializations.</summary>
    public List<Unit> Targets { get; } = [];

    /// <summary>The enum members the code names, except where it only tests or keys by them.</summary>
    public List<IFieldSymbol> EnumMembers { get; } = [];

    /// <summary>
    /// The units the code only defers: methods it takes as delegates (method groups), which it hands on to run later, for
    /// whoever invokes them, rather than running them itself.
    /// </summary>
    public HashSet<Unit> Deferred { get; } = [];

    /// <summary>The units the code runs itself: everything in <see cref="Targets"/> it does not only defer.</summary>
    public HashSet<Unit> Runs { get; } = [];

    /// <summary>The definitions whose sheets the code draws a frame from by name (an explicit <c>EnemySheetDraw</c>).</summary>
    public HashSet<IFieldSymbol> SheetDraws { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>The calls in the code that hand a spawned object its graphics.</summary>
    public List<SpawnCall> SpawnCalls { get; } = [];

    /// <summary>
    /// The Core methods each enum member the code names is handed to as a call's argument: a kind passed to the helper that
    /// allocates and starts the object (#1534).
    /// </summary>
    public Dictionary<IFieldSymbol, HashSet<Unit>> HandedTo { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>
    /// The Core methods this code passes one of its own parameters to unchanged: what a forwarding method was handed, such as
    /// Mother Brain's room actors passing her bomb's afterburn kind on to Ridley's projectile code, reaches them (#1537).
    /// </summary>
    public HashSet<Unit> ForwardsTo { get; } = [];

    /// <summary>
    /// The members this code's object creations hand to each Core constructor parameter (#1852): a behaviour object handed
    /// to another behaviour's constructor, as Spore Spawn's spawners are handed the spores' behaviour and Mother Brain's
    /// projectiles the turret bullets', runs where the receiver reads that parameter, not where the receiver is built.
    /// </summary>
    public Dictionary<IParameterSymbol, HashSet<Unit>> ConstructorArguments { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>The constructor parameters the code reads: a primary constructor's parameters its type's members capture.</summary>
    public HashSet<IParameterSymbol> ConstructorParameterReads { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>How many of each member's references in <see cref="Targets"/> are constructor arguments.</summary>
    private readonly Dictionary<Unit, int> _constructorArgumentReferences = [];

    /// <summary>Records that the code hands <paramref name="member"/> to the constructor parameter <paramref name="parameter"/>.</summary>
    public void HandToConstructor(IParameterSymbol parameter, Unit member)
    {
        if (!ConstructorArguments.TryGetValue(parameter, out HashSet<Unit>? members))
            ConstructorArguments.Add(parameter, members = []);
        members.Add(member);
        _constructorArgumentReferences[member] = _constructorArgumentReferences.GetValueOrDefault(member) + 1;
    }

    /// <summary>Whether the code references <paramref name="unit"/> only by handing it to constructors, never running or reading it itself.</summary>
    public bool OnlyHandsToConstructors(Unit unit) =>
        _constructorArgumentReferences.TryGetValue(unit, out int handed) && Targets.Count(target => target.Equals(unit)) == handed;

    /// <summary>
    /// The projectile program families the code refers to, each with the programs its constructions start it in (#1513);
    /// null for a family the code reaches otherwise, or constructs naming a program no constant decides, which may then be
    /// started in any of its programs.
    /// </summary>
    public Dictionary<INamedTypeSymbol, HashSet<string>?> FamilyPrograms { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>
    /// The program arguments of the constructions of each family that no constant decides (#1784), as a spawner choosing
    /// the program its callers hand it does: the walk reaching the code traces what its callers pass, and adds those
    /// programs to the family's <see cref="FamilyPrograms"/>.
    /// </summary>
    public Dictionary<INamedTypeSymbol, List<(ExpressionSyntax Program, SemanticModel Semantic)>> FamilyProgramArguments { get; } =
        new(SymbolEqualityComparer.Default);

    /// <summary>
    /// Where graphics this code assigns to a spawned object (after spawning it, before or between draws) can make it draw
    /// from: the object may draw with these as well as with the graphics it was spawned with.
    /// </summary>
    public List<(IReadOnlyList<SpawnTarget> Targets, string Graphics)> Reassignments { get; } = [];

    /// <summary>Every sheet what this code spawns (constructs, starts) can draw from, whichever call spawns it.</summary>
    public IReadOnlyList<SpawnTarget> UnitTargets =>
        SpawnCalls.Count == 0 && Reassignments.Count == 0
            ? [SpawnTarget.Spawner]
            : [.. SpawnCalls.SelectMany(c => c.Targets).Concat(Reassignments.SelectMany(r => r.Targets)).Distinct()];

    /// <summary>
    /// Every sheet <paramref name="member"/>, named by this code, can be spawned to draw from: the sheets of the calls whose
    /// own arguments name it, or of every spawn the code makes when no call's arguments do; and any graphics the code
    /// reassigns.
    /// </summary>
    public IReadOnlyList<SpawnTarget> TargetsOf(IFieldSymbol member)
    {
        var naming = SpawnCalls.Where(c => c.Arguments.Contains(member)).ToList();
        if (naming.Count == 0)
            return UnitTargets;
        return [.. naming.SelectMany(c => c.Targets).Concat(Reassignments.SelectMany(r => r.Targets)).Distinct()];
    }
}

/// <summary>The enemy definition one per-definition walk evaluates its behaviour's dispatch for.</summary>
/// <param name="Behaviour">The behaviour serving the definition.</param>
/// <param name="Definition">The definition's EnemyDefinitionId member.</param>
/// <param name="Part">The part its behaviour's parts table maps it to, when it has one.</param>
internal sealed record DefinitionContext(Behaviour Behaviour, IFieldSymbol Definition, IFieldSymbol? Part);

/// <summary>Reads, once per unit, the semantic references in that unit's code.</summary>
/// <param name="model">The Core facts.</param>
internal sealed class UnitScanner(CoreModel model)
{
    private readonly Dictionary<Unit, UnitScan> _scans = [];
    private readonly Dictionary<(Unit Unit, string Definition), UnitScan> _definitionScans = [];

    /// <summary>The references in <paramref name="unit"/>'s code.</summary>
    public UnitScan Scan(Unit unit) => Scan(unit, context: null);

    /// <summary>
    /// The references in <paramref name="unit"/>'s code, with <paramref name="context"/>'s definition's dispatch evaluated
    /// when the unit is its behaviour's entity code or entity factory: switches over the definition's part, and tests of
    /// the entity's own slot's definition, take only the branches that definition takes.
    /// </summary>
    public UnitScan Scan(Unit unit, DefinitionContext? context)
    {
        if (context is null || !IsDispatch(unit, context.Behaviour))
            return Cached(unit, null);
        var key = (unit, context.Definition.Name);
        if (!_definitionScans.TryGetValue(key, out UnitScan? scan))
        {
            scan = Build(unit, context);
            _definitionScans.Add(key, scan);
        }
        return scan;
    }

    /// <summary>Whether <paramref name="unit"/> is code that dispatches on its slot's definition: an entity type's, or the behaviour's entity factory.</summary>
    public bool IsDispatch(Unit unit, Behaviour behaviour) =>
        model.IsEntityType(unit.Type) || IsCreate(unit, behaviour);

    /// <summary>Whether <paramref name="unit"/> is <paramref name="behaviour"/>'s entity factory method.</summary>
    public static bool IsCreate(Unit unit, Behaviour behaviour) =>
        unit is { Kind: UnitKind.Member, Symbol: IMethodSymbol { Name: "Create" } create }
        && SymbolEqualityComparer.Default.Equals(create.ContainingType.OriginalDefinition, behaviour.Type);

    /// <summary>The unevaluated scan of <paramref name="unit"/>.</summary>
    private UnitScan Cached(Unit unit, DefinitionContext? context)
    {
        if (!_scans.TryGetValue(unit, out UnitScan? scan))
        {
            scan = Build(unit, context);
            _scans.Add(unit, scan);
        }
        return scan;
    }

    /// <summary>Scans <paramref name="unit"/>, evaluating <paramref name="context"/>'s dispatch when it is given.</summary>
    private UnitScan Build(Unit unit, DefinitionContext? context)
    {
        var scan = new UnitScan();
        switch (unit.Kind)
        {
            case UnitKind.StaticInitialization:
                AddStaticConstructor(scan, unit.Type);
                break;
            case UnitKind.InstanceInitialization:
                AddInstanceInitializers(scan, unit.Type);
                break;
            default:
                ScanMember(scan, unit, context);
                break;
        }
        return scan;
    }

    /// <summary>
    /// Adds a type's static constructor: what reading any of its static data runs besides that member's own initializer.
    /// Each static field or property initializer is its own unit, reached by the code that reads it.
    /// </summary>
    private static void AddStaticConstructor(UnitScan scan, INamedTypeSymbol type)
    {
        foreach (IMethodSymbol constructor in type.StaticConstructors)
        {
            if (!constructor.IsImplicitlyDeclared)
                scan.Targets.Add(Unit.Member(constructor.OriginalDefinition));
        }
    }

    /// <summary>Adds the initialized instance fields and properties of a type, which every non-chaining constructor runs.</summary>
    private static void AddInstanceInitializers(UnitScan scan, INamedTypeSymbol type)
    {
        foreach (ISymbol member in type.GetMembers())
        {
            if (member.IsStatic || member.IsImplicitlyDeclared)
                continue;
            switch (member)
            {
                case IFieldSymbol { IsConst: false } field when field.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is VariableDeclaratorSyntax { Initializer: not null }):
                    scan.Targets.Add(Unit.Member(field.OriginalDefinition));
                    break;
                case IPropertySymbol property when property.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is PropertyDeclarationSyntax { Initializer: not null }):
                    scan.Targets.Add(Unit.Member(property.OriginalDefinition));
                    break;
            }
        }
    }

    /// <summary>Scans one member's declaration, and for a constructor the initialization it runs.</summary>
    private void ScanMember(UnitScan scan, Unit unit, DefinitionContext? context)
    {
        ISymbol member = unit.Symbol;
        switch (member)
        {
            case IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor } constructor:
                ScanConstructor(scan, unit, constructor, context);
                break;
            // Synthesized members (a record's equality, printing and cloning) have no code of their own; their
            // declaring reference is the whole type declaration, which is not what they run.
            case { IsImplicitlyDeclared: true }:
                break;
            case IFieldSymbol field:
                foreach (SyntaxReference reference in field.DeclaringSyntaxReferences)
                {
                    if (reference.GetSyntax() is VariableDeclaratorSyntax { Initializer: { } initializer })
                        Visit(scan, initializer, unit, context);
                }
                break;
            case IPropertySymbol or IEventSymbol or IMethodSymbol:
                foreach (SyntaxReference reference in member.DeclaringSyntaxReferences)
                {
                    SyntaxNode declaration = reference.GetSyntax();
                    // A positional record's property is its constructor parameter, which has no code of its own.
                    if (declaration is not ParameterSyntax)
                        Visit(scan, declaration, unit, context);
                }
                break;
            default:
                throw new InvalidOperationException($"{member.ToDisplayString()} is not a member this generator scans.");
        }
    }

    /// <summary>
    /// Scans a constructor: its body and initializer, or a primary constructor's parameters and base arguments; the
    /// type's static initialization; its instance initializers unless it chains to another constructor of its type;
    /// and the base constructor it implicitly calls.
    /// </summary>
    private void ScanConstructor(UnitScan scan, Unit unit, IMethodSymbol constructor, DefinitionContext? context)
    {
        INamedTypeSymbol type = constructor.ContainingType;
        if (constructor.MethodKind == MethodKind.StaticConstructor)
        {
            foreach (SyntaxReference reference in constructor.DeclaringSyntaxReferences)
                Visit(scan, reference.GetSyntax(), unit, context);
            return;
        }

        bool chainsToThis = false;
        bool callsBase = false;
        // An implicitly declared constructor (a default or record copy constructor) runs only initialization.
        IEnumerable<SyntaxReference> declarations = constructor.IsImplicitlyDeclared ? [] : constructor.DeclaringSyntaxReferences;
        foreach (SyntaxReference reference in declarations)
        {
            switch (reference.GetSyntax())
            {
                case ConstructorDeclarationSyntax declaration:
                    Visit(scan, declaration, unit, context);
                    chainsToThis |= declaration.Initializer?.ThisOrBaseKeyword.IsKind(SyntaxKind.ThisKeyword) == true;
                    callsBase |= declaration.Initializer?.ThisOrBaseKeyword.IsKind(SyntaxKind.BaseKeyword) == true;
                    break;
                case TypeDeclarationSyntax primary:
                    if (primary.ParameterList is { } parameters)
                        Visit(scan, parameters, unit, context);
                    foreach (PrimaryConstructorBaseTypeSyntax baseCall in primary.BaseList?.Types.OfType<PrimaryConstructorBaseTypeSyntax>()
                                 ?? Enumerable.Empty<PrimaryConstructorBaseTypeSyntax>())
                    {
                        Visit(scan, baseCall, unit, context);
                        callsBase = true;
                    }
                    break;
            }
        }
        scan.Targets.Add(Unit.StaticInitialization(type));
        if (chainsToThis)
            return;
        scan.Targets.Add(Unit.InstanceInitialization(type));
        if (!callsBase && type.BaseType is { } baseType && model.IsSource(baseType.OriginalDefinition)
            && baseType.OriginalDefinition.InstanceConstructors.FirstOrDefault(c => c.Parameters.All(p => p.IsOptional || p.IsParams)) is { } implicitBase)
        {
            scan.Targets.Add(Unit.Member(implicitBase.OriginalDefinition));
        }
    }

    /// <summary>
    /// Binds every reference in <paramref name="root"/>'s code. Attributes, nameof and typed-start entries are not code
    /// that runs; a value a case label, constant pattern or equality test compares against is tested, not used; and a
    /// table keyed by an enum member looks things up by it rather than producing it: those are skipped. With a
    /// definition context, the dispatch on that definition takes only the branches it takes.
    /// </summary>
    private void Visit(UnitScan scan, SyntaxNode root, Unit unit, DefinitionContext? context)
    {
        SemanticModel semantic = model.ModelOf(root.SyntaxTree);
        DefinitionPruner? pruner = context is null ? null : new DefinitionPruner(model, semantic, context, unit);
        var pending = new Stack<SyntaxNode>([root]);
        while (pending.Count > 0)
        {
            SyntaxNode node = pending.Pop();
            // A frame-behaviour restart only some kinds' motions run is read, like a typed start, as those kinds' (#1854).
            if (node is AttributeListSyntax or CaseSwitchLabelSyntax or ConstantPatternSyntax or RelationalPatternSyntax
                || model.IsStartEntry(node) || model.KindRestarts.IsRestart(node))
            {
                continue;
            }
            if (node is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "nameof" } } nameOf
                && semantic.GetSymbolInfo(nameOf).Symbol is null)
            {
                continue;
            }
            Bind(scan, semantic, node);
            if (pruner is not null && pruner.TakenChildren(node) is { } taken)
            {
                foreach (SyntaxNode child in taken)
                    pending.Push(child);
                continue;
            }
            bool comparison = node.IsKind(SyntaxKind.EqualsExpression) || node.IsKind(SyntaxKind.NotEqualsExpression);
            SyntaxNode? key = KeyOf(node);
            foreach (SyntaxNode child in node.ChildNodes())
            {
                if ((comparison || ReferenceEquals(child, key)) && IsEnumMember(semantic, child))
                    continue;
                pending.Push(child);
            }
        }
    }

    /// <summary>The key of a keyed initializer entry: <c>[key] = value</c>'s index, or a <c>{ key, value }</c> element's first expression.</summary>
    private static SyntaxNode? KeyOf(SyntaxNode node) => node switch
    {
        ImplicitElementAccessSyntax { ArgumentList: { Arguments.Count: 1 } index } => index,
        InitializerExpressionSyntax { RawKind: (int)SyntaxKind.ComplexElementInitializerExpression, Expressions: [var first, _, ..] } => first,
        _ => null,
    };

    /// <summary>Whether <paramref name="node"/> (or the one expression an index's brackets hold) names an enum member.</summary>
    private static bool IsEnumMember(SemanticModel semantic, SyntaxNode node)
    {
        SyntaxNode expression = node is BracketedArgumentListSyntax { Arguments: [var argument] } ? argument.Expression : node;
        return expression is ExpressionSyntax && semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum };
    }

    /// <summary>Records the symbols <paramref name="node"/> references.</summary>
    private void Bind(UnitScan scan, SemanticModel semantic, SyntaxNode node)
    {
        switch (node)
        {
            case SimpleNameSyntax or BaseObjectCreationExpressionSyntax or ElementAccessExpressionSyntax or ImplicitElementAccessSyntax
                or ConstructorInitializerSyntax or PrimaryConstructorBaseTypeSyntax or BinaryExpressionSyntax
                or PrefixUnaryExpressionSyntax or PostfixUnaryExpressionSyntax or AssignmentExpressionSyntax or CastExpressionSyntax:
                SymbolInfo info = semantic.GetSymbolInfo(node);
                int before = scan.Targets.Count;
                if (info.Symbol is { } symbol)
                {
                    Add(scan, symbol);
                    RecordFamilyStart(scan, semantic, node, symbol);
                    if (node is SimpleNameSyntax simple)
                        RecordConstructorFlow(scan, semantic, simple, symbol, before);
                }
                else if (info.CandidateReason is CandidateReason.MemberGroup or CandidateReason.OverloadResolutionFailure)
                {
                    foreach (ISymbol candidate in info.CandidateSymbols)
                        Add(scan, candidate);
                }
                // A lambda's code runs as part of this code's (as a program's cue callback does); a method group is handed on.
                bool deferred = node is SimpleNameSyntax name && info.Symbol is IMethodSymbol && !IsInvoked(name);
                foreach (Unit added in scan.Targets.Skip(before))
                    (deferred ? scan.Deferred : scan.Runs).Add(added);
                if (node is BaseObjectCreationExpressionSyntax creation && info.Symbol is IMethodSymbol constructor)
                    RecordSheetDraw(scan, semantic, creation, constructor);
                // An object constructed with graphics holds them as its own (a room-drawn object's), so the creation hands
                // them on as a spawn call does, to the constructor parameter that receives them. A spawn primitive building
                // the object from the kind and graphics its own callers handed it (the sprite-object list's spawn) spawns
                // what those callers' calls name, and each of those calls records its own kind and graphics.
                if (node is BaseObjectCreationExpressionSyntax { ArgumentList: { } constructed } && info.Symbol is IMethodSymbol made
                    && made.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p.Type.OriginalDefinition, model.SpriteGraphics))
                    && !BuildsFromHandedSpawn(semantic, constructed, made))
                {
                    RecordSpawnCall(scan, semantic, constructed, made);
                }
                break;
            case CommonForEachStatementSyntax loop:
                ForEachStatementInfo loopInfo = semantic.GetForEachStatementInfo(loop);
                foreach (ISymbol? member in new ISymbol?[] { loopInfo.GetEnumeratorMethod, loopInfo.MoveNextMethod, loopInfo.CurrentProperty })
                {
                    if (member is not null)
                        Add(scan, member);
                }
                break;
        }
        if (node is InvocationExpressionSyntax invocation && semantic.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
            && method.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p.Type.OriginalDefinition, model.SpriteGraphics)))
            RecordSpawnCall(scan, semantic, invocation.ArgumentList, method);
        if (node is InvocationExpressionSyntax handing && semantic.GetSymbolInfo(handing).Symbol is IMethodSymbol callee && model.IsSource(callee))
            RecordHandedMembers(scan, semantic, handing.ArgumentList, callee);
        // A graphics switch is code changing another object's graphics (a cue handler clearing a projectile's). An object
        // setting its own (a slot's construction or Clear, its free-slot value) is its lifecycle, not a spawn's graphics.
        if (node is AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression } assignment
            && assignment.Left is MemberAccessExpressionSyntax { Expression: not (ThisExpressionSyntax or BaseExpressionSyntax) }
            && semantic.GetSymbolInfo(assignment.Left).Symbol is IPropertySymbol or IFieldSymbol
            && semantic.GetTypeInfo(assignment.Left).Type is { } assigned
            && SymbolEqualityComparer.Default.Equals(assigned.OriginalDefinition, model.SpriteGraphics))
        {
            var classifier = new GraphicsClassifier(model);
            // Reshading keeps the owner the object already has.
            IReadOnlyList<SpawnTarget> switched = classifier.IsReshading(semantic, assignment.Right) ? [] : classifier.Classify(semantic, assignment.Right);
            // Graphics a method was handed and installs in the object it spawns (the pool's and the engine's spawn
            // primitives) are its callers' spawn graphics, which their own calls already name: no switch. A cue handler's
            // switch to the room's graphics is the effect of the programs publishing the cue, which the program frames
            // route to the shared sheet from that step on (#1513).
            bool cueSwitch = assignment.Ancestors().OfType<SwitchSectionSyntax>().FirstOrDefault() is { } section
                && model.ProgramFrames.IsRoomGraphicsCueSection(section);
            if (switched.Any(target => target.Source != GraphicsSource.Parameter) && !cueSwitch)
                scan.Reassignments.Add((switched, assignment.ToString()));
        }
        if (node is ExpressionSyntax expression && !SyntaxFacts.IsInTypeOnlyContext(expression)
            && semantic.GetConversion(expression) is { IsUserDefined: true, MethodSymbol: { } conversion })
            Add(scan, conversion);
    }

    /// <summary>
    /// Records a reference to a projectile program family's code (#1513): a construction starts it in the program its
    /// argument names, or, when no constant decides it, in the programs the walk finds its callers hand it (#1784); any
    /// other reference may start it in any.
    /// </summary>
    private void RecordFamilyStart(UnitScan scan, SemanticModel semantic, SyntaxNode node, ISymbol symbol)
    {
        if (symbol is INamedTypeSymbol || symbol.ContainingType is not { } containing || model.FamilyOf(containing.OriginalDefinition) is not { } family)
            return;
        BaseObjectCreationExpressionSyntax? construction = node is BaseObjectCreationExpressionSyntax creation
            && symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } && SymbolEqualityComparer.Default.Equals(containing.OriginalDefinition, family)
            ? creation : null;
        IReadOnlySet<string>? programs = construction is null ? null : model.ProgramsOf(semantic, construction, family);
        if (programs is null && construction is { ArgumentList.Arguments: [var traced, ..] })
        {
            if (!scan.FamilyProgramArguments.TryGetValue(family, out List<(ExpressionSyntax, SemanticModel)>? arguments))
                scan.FamilyProgramArguments.Add(family, arguments = []);
            arguments.Add((traced.Expression, semantic));
            programs = new HashSet<string>(StringComparer.Ordinal);
        }
        if (programs is null)
        {
            scan.FamilyPrograms[family] = null;
            return;
        }
        if (!scan.FamilyPrograms.TryGetValue(family, out HashSet<string>? named))
            scan.FamilyPrograms[family] = new HashSet<string>(programs, StringComparer.Ordinal);
        else
            named?.UnionWith(programs);
    }

    /// <summary>
    /// Records a constructor parameter <paramref name="name"/> reads, or the member it hands to a Core constructor's
    /// parameter as an argument of an object creation (#1852), so the walk links the member to the code reading that
    /// parameter. A named argument's label names the parameter without reading it.
    /// </summary>
    private void RecordConstructorFlow(UnitScan scan, SemanticModel semantic, SimpleNameSyntax name, ISymbol symbol, int before)
    {
        if (name.Parent is NameColonSyntax)
            return;
        if (symbol is IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.Constructor } } parameter)
        {
            scan.ConstructorParameterReads.Add(parameter.OriginalDefinition);
            return;
        }
        ExpressionSyntax value = name.Parent is MemberAccessExpressionSyntax access && access.Name == name ? access : name;
        if (value.Parent is not ArgumentSyntax { Parent: ArgumentListSyntax { Parent: BaseObjectCreationExpressionSyntax } } argument
            || CoreModel.ParameterOf(semantic, argument) is not { } handed || !model.IsSource(handed))
        {
            return;
        }
        foreach (Unit member in scan.Targets.Skip(before).Where(unit => unit.Kind == UnitKind.Member).ToList())
            scan.HandToConstructor(handed, member);
    }

    /// <summary>Whether <paramref name="name"/> is the method an invocation calls, rather than a method group passed on.</summary>
    private static bool IsInvoked(SimpleNameSyntax name)
    {
        SyntaxNode callee = name.Parent is MemberAccessExpressionSyntax access && access.Name == name ? access
            : name.Parent is MemberBindingExpressionSyntax binding ? binding
            : name;
        return callee.Parent is InvocationExpressionSyntax invocation && invocation.Expression == callee;
    }

    /// <summary>Records an <c>EnemySheetDraw</c> whose first argument names its definition's sheet as a constant.</summary>
    private void RecordSheetDraw(UnitScan scan, SemanticModel semantic, BaseObjectCreationExpressionSyntax creation, IMethodSymbol constructor)
    {
        if (!SymbolEqualityComparer.Default.Equals(constructor.ContainingType.OriginalDefinition, model.EnemySheetDraw)
            || creation.ArgumentList is not { Arguments: [var first, ..] })
        {
            return;
        }
        if (semantic.GetSymbolInfo(first.Expression).Symbol is IFieldSymbol definition
            && SymbolEqualityComparer.Default.Equals(definition.ContainingType, model.EnemyDefinitionId))
        {
            scan.SheetDraws.Add(definition.OriginalDefinition);
        }
    }

    /// <summary>Records each enum member a call's arguments name as handed to <paramref name="callee"/>.</summary>
    private static void RecordHandedMembers(UnitScan scan, SemanticModel semantic, ArgumentListSyntax arguments, IMethodSymbol callee)
    {
        foreach (ArgumentSyntax argument in arguments.Arguments)
        {
            // A parameter of the calling method passed on unchanged: the caller forwards what it was handed (#1537).
            if (semantic.GetSymbolInfo(argument.Expression).Symbol is IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: not MethodKind.AnonymousFunction } })
                scan.ForwardsTo.Add(Unit.Member(callee.OriginalDefinition));
            foreach (SyntaxNode part in argument.Expression.DescendantNodesAndSelf())
            {
                if (part is not ExpressionSyntax value
                    || semantic.GetSymbolInfo(value).Symbol is not IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member)
                    continue;
                IFieldSymbol key = member.OriginalDefinition;
                if (!scan.HandedTo.TryGetValue(key, out HashSet<Unit>? callees))
                    scan.HandedTo.Add(key, callees = []);
                callees.Add(Unit.Member(callee.OriginalDefinition));
            }
        }
    }

    /// <summary>
    /// Whether the construction <paramref name="arguments"/> pass to <paramref name="constructor"/> builds the object from
    /// the graphics and sprite-object kind the enclosing method was itself handed: both arguments are that method's
    /// parameters.
    /// </summary>
    private bool BuildsFromHandedSpawn(SemanticModel semantic, ArgumentListSyntax arguments, IMethodSymbol constructor)
    {
        bool graphicsHanded = false, kindHanded = false;
        for (int index = 0; index < arguments.Arguments.Count; index++)
        {
            ArgumentSyntax argument = arguments.Arguments[index];
            IParameterSymbol? parameter = argument.NameColon is { } name
                ? constructor.Parameters.FirstOrDefault(p => p.Name == name.Name.Identifier.Text)
                : index < constructor.Parameters.Length ? constructor.Parameters[index] : null;
            bool handed = semantic.GetSymbolInfo(argument.Expression).Symbol is IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: not MethodKind.AnonymousFunction } };
            if (parameter is not null && SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, model.SpriteGraphics))
                graphicsHanded = handed;
            else if (parameter is not null && SymbolEqualityComparer.Default.Equals(parameter.Type, model.RoomSpriteObjectKind))
                kindHanded = handed;
        }
        return graphicsHanded && kindHanded;
    }

    /// <summary>
    /// Records a call handed a spawned object's graphics: where they make it draw from, and the enum members its other
    /// arguments name.
    /// </summary>
    private void RecordSpawnCall(UnitScan scan, SemanticModel semantic, ArgumentListSyntax arguments, IMethodSymbol method)
    {
        ExpressionSyntax? graphics = null;
        bool openSpriteObjectKind = false;
        var others = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        for (int index = 0; index < arguments.Arguments.Count; index++)
        {
            ArgumentSyntax argument = arguments.Arguments[index];
            IParameterSymbol? parameter = argument.NameColon is { } name
                ? method.Parameters.FirstOrDefault(p => p.Name == name.Name.Identifier.Text)
                : index < method.Parameters.Length ? method.Parameters[index] : null;
            if (parameter is not null && SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, model.SpriteGraphics))
            {
                graphics = argument.Expression;
                continue;
            }
            if (parameter is not null && SymbolEqualityComparer.Default.Equals(parameter.Type, model.RoomSpriteObjectKind)
                && !argument.Expression.DescendantNodesAndSelf().Any(part => part is ExpressionSyntax value
                    && semantic.GetSymbolInfo(value).Symbol is IFieldSymbol named
                    && SymbolEqualityComparer.Default.Equals(named.ContainingType, model.RoomSpriteObjectKind)))
                openSpriteObjectKind = true;
            foreach (SyntaxNode part in argument.Expression.DescendantNodesAndSelf())
            {
                if (part is ExpressionSyntax value && semantic.GetSymbolInfo(value).Symbol is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member)
                    others.Add(member.OriginalDefinition);
            }
        }
        if (graphics is null)
            return;
        bool copied = semantic.GetSymbolInfo(graphics).Symbol is IPropertySymbol { IsStatic: false, SetMethod: not null }
            or IFieldSymbol { IsStatic: false, IsReadOnly: false, IsConst: false };
        scan.SpawnCalls.Add(new SpawnCall(new GraphicsClassifier(model).Classify(semantic, graphics), others, method.OriginalDefinition,
            graphics.ToString(), copied, openSpriteObjectKind));
    }

    /// <summary>Records one referenced symbol as an enum member or as the units it reaches.</summary>
    private void Add(UnitScan scan, ISymbol symbol)
    {
        switch (symbol)
        {
            case IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member:
                scan.EnumMembers.Add(member.OriginalDefinition);
                break;
            case IFieldSymbol field:
                IFieldSymbol definition = field.OriginalDefinition;
                if (!model.IsSource(definition) || definition.IsImplicitlyDeclared)
                    return;
                scan.Targets.Add(Unit.Member(definition));
                // Reading static data runs the type's static constructor; a constant is inlined and runs nothing.
                if (definition.IsStatic && !definition.IsConst)
                    scan.Targets.Add(Unit.StaticInitialization(definition.ContainingType));
                break;
            case IPropertySymbol property:
                if (model.NormalizeProperty(property) is not { } normalized)
                    return;
                scan.Targets.Add(Unit.Member(normalized));
                if (normalized.IsStatic && CoreModel.IsDataProperty(normalized))
                    scan.Targets.Add(Unit.StaticInitialization(normalized.ContainingType));
                break;
            case IMethodSymbol method:
                switch (model.NormalizeMethod(method))
                {
                    // An accessor reaches its property, with the static data that property may be.
                    case IPropertySymbol accessed:
                        Add(scan, accessed);
                        break;
                    case { } target:
                        scan.Targets.Add(Unit.Member(target));
                        break;
                }
                break;
            case IEventSymbol @event when model.IsSource(@event):
                scan.Targets.Add(Unit.Member(@event.OriginalDefinition));
                break;
        }
    }
}
