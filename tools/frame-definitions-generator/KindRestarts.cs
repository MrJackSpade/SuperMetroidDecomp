using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>One program start a frame-behaviour handler makes on the object it moves.</summary>
/// <param name="Family">The projectile program family started.</param>
/// <param name="Programs">The programs it is started in; null when it may be any of them.</param>
/// <param name="Motions">
/// The motions whose objects run the code making it, with those the objects were given that lead to them (#1881): the
/// motions its kinds' definitions or its family's cues give carry it.
/// </param>
internal sealed record KindRestart(INamedTypeSymbol Family, IReadOnlySet<string>? Programs, IReadOnlySet<IFieldSymbol> Motions);

/// <summary>
/// The restarts a projectile behaviour's frame-behaviour handler (its <c>Move</c> override) makes only under a test of the
/// moving object's motion, for motions no code gives an object but its kind's definition (#1854). A behaviour serving
/// several kinds runs one handler for all of them and branches on the object's motion: Spore Spawn's spawners, stalk links
/// and spores share his projectile behaviour, and only a spawner's cadence restarts it releasing. Such a restart runs on the
/// objects of the kinds whose definitions name a tested motion, so it draws wherever those kinds are spawned: the walks add
/// it where they name those kinds, never to every start of its family, and a handler spawning such a kind with the room's
/// graphics carries it with that room start. A motion its family's own cues set carries a restart too (#1881): the segments
/// publishing that cue run it, as a Golden Torizo super missile's throw cue sets the flight whose wall hit restarts it in its
/// impact; and a motion guarded code assigns leads back to the motions held there, as a charging egg drops. A restart
/// reached otherwise, under a motion other code can assign at run time (a value handed round, another family's cue), or
/// with a switch of the object's graphics, stays as the walks and the family's handler starts read it.
/// </summary>
internal sealed class KindRestarts
{
    /// <summary>The catalog whose definitions give each kind its motion.</summary>
    private const string CatalogType = "EnemyProjectileDefinitionCatalog";

    /// <summary>The catalog's definitions.</summary>
    private const string CatalogMember = "Definitions";

    private readonly CoreModel _model;
    private readonly Dictionary<IFieldSymbol, IFieldSymbol> _kindMotions = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<SyntaxNode, KindRestart> _restarts = [];

    /// <summary>
    /// Reads every kind's motion, where every other motion comes from, and every handler restart under motions that only
    /// kinds' definitions, a family's own cues, or code running under such motions give.
    /// </summary>
    public KindRestarts(CoreModel model)
    {
        _model = model;
        ExpressionSyntax catalog = CoreModel.InitializerOf(model.Member(CatalogType, CatalogMember));
        ReadKindMotions(catalog);
        List<(StatementSyntax Region, HashSet<IFieldSymbol> Motions)> regions = [.. GuardedRegions()];
        Dictionary<IMethodSymbol, HashSet<IFieldSymbol>> methods = GuardedOnlyMethods(regions);
        MotionSources sources = ReadMotionSources(catalog, regions, methods);
        var spawned = new SpawnedStarts(model);
        IEnumerable<(SyntaxNode Code, IReadOnlySet<IFieldSymbol> Motions)> code =
            regions.Select(region => ((SyntaxNode)region.Region, (IReadOnlySet<IFieldSymbol>)region.Motions))
                .Concat(methods.SelectMany(method => method.Key.DeclaringSyntaxReferences
                    .Select(reference => (reference.GetSyntax(), (IReadOnlySet<IFieldSymbol>)method.Value))));
        foreach ((SyntaxNode root, IReadOnlySet<IFieldSymbol> guard) in code)
        {
            // The code runs on objects holding a tested motion, so on objects given a motion that leads to one.
            HashSet<IFieldSymbol> motions = sources.Origins(guard);
            // A restart under a motion an object can be given at run time from anywhere may run on any object of its family.
            if (motions.Count == 0 || motions.Any(sources.Free.Contains))
                continue;
            SemanticModel semantic = model.ModelOf(root.SyntaxTree);
            foreach (BaseObjectCreationExpressionSyntax creation in root.DescendantNodesAndSelf().OfType<BaseObjectCreationExpressionSyntax>())
            {
                if (semantic.GetSymbolInfo(creation).Symbol is not IMethodSymbol { ContainingType: { } created }
                    || model.FamilyOf(created.OriginalDefinition) is not { } family
                    || !SymbolEqualityComparer.Default.Equals(family, created.OriginalDefinition)
                    // Code a lambda or local function defers may run wherever it is handed.
                    || creation.Ancestors().TakeWhile(ancestor => ancestor != root).Any(ancestor => ancestor is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax)
                    // Only a start on the object the handler was handed restarts it: a start on an object the handler spawns
                    // is that spawn's, and one whose code switches the object's graphics draws where the switch says (a
                    // landed Botwoon segment's explosion, on the shared sheet), which the walks read from that code.
                    || spawned.Of(semantic, creation) is not { Receiver: not null, Spawn: null, Switched: false }
                    // A motion another family's cues set would carry the restart into that family's objects, which its
                    // programs do not reach; such a restart stays with every start of its family.
                    || motions.Any(motion => sources.CueSet.TryGetValue(motion, out HashSet<INamedTypeSymbol>? setters)
                        && !setters.All(setter => SymbolEqualityComparer.Default.Equals(setter, family))))
                {
                    continue;
                }
                IReadOnlySet<string>? programs = model.ProgramsOf(semantic, creation, family)
                    ?? (creation.ArgumentList is { Arguments: [var program, ..] } && model.ProgramTypeOf(family) is { } programType
                        ? new ProgramTracer(model, reaches: null).Programs(programType, program.Expression, semantic)
                        : null);
                if (_restarts.TryGetValue(creation, out KindRestart? known))
                {
                    var union = new HashSet<IFieldSymbol>(known.Motions, SymbolEqualityComparer.Default);
                    union.UnionWith(motions);
                    _restarts[creation] = known with { Motions = union };
                }
                else
                {
                    _restarts.Add(creation, new KindRestart(family, programs, motions));
                }
            }
        }
        _cueSet = sources.CueSet;
    }

    /// <summary>The families whose segments' cues set each motion they set.</summary>
    private readonly Dictionary<IFieldSymbol, HashSet<INamedTypeSymbol>> _cueSet;

    /// <summary>
    /// By motion, the programs <paramref name="family"/>'s objects are restarted in under a motion only its own segments'
    /// cues set (#1881); null when one restart may be in any of its programs. A segment publishing such a cue runs them.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>?> CueMotionRestarts(INamedTypeSymbol family)
    {
        var restarts = new Dictionary<string, HashSet<string>?>(StringComparer.Ordinal);
        foreach (KindRestart restart in _restarts.Values.Where(restart => SymbolEqualityComparer.Default.Equals(restart.Family, family)))
        {
            foreach (IFieldSymbol motion in restart.Motions.Where(_cueSet.ContainsKey))
            {
                if (!restarts.TryGetValue(motion.Name, out HashSet<string>? programs))
                    restarts.Add(motion.Name, programs = new HashSet<string>(StringComparer.Ordinal));
                if (programs is null)
                    continue;
                if (restart.Programs is null)
                    restarts[motion.Name] = null;
                else
                    programs.UnionWith(restart.Programs);
            }
        }
        return restarts.ToDictionary(pair => pair.Key, pair => (IReadOnlySet<string>?)pair.Value, StringComparer.Ordinal);
    }

    /// <summary>Whether <paramref name="node"/> is a restart the kinds whose motions run it carry, rather than code its walk records.</summary>
    public bool IsRestart(SyntaxNode node) => _restarts.ContainsKey(node);

    /// <summary>The restarts the objects of <paramref name="kind"/> run: those under its definition's motion.</summary>
    public IEnumerable<KindRestart> Of(IFieldSymbol kind) =>
        _kindMotions.TryGetValue(kind.OriginalDefinition, out IFieldSymbol? motion)
            ? _restarts.Values.Where(restart => restart.Motions.Contains(motion))
            : [];

    /// <summary>
    /// The restarts the object <paramref name="spawn"/> spawns in <paramref name="family"/> may run: those of the kinds its spawn
    /// call is handed, traced through the parameters handing them in; every restart of the family when the kind is not traced
    /// (a chained afterburn handed the kind its predecessor carried).
    /// </summary>
    public IEnumerable<KindRestart> OfSpawn(StartedObject spawn, INamedTypeSymbol family)
    {
        HashSet<string>? kinds = spawn is { Kind: { } kind, KindSemantic: { } semantic }
            ? new ProgramTracer(_model, reaches: null).Programs(_model.RoomEnemyProjectileKind, kind, semantic)
            : null;
        if (kinds is null)
            return _restarts.Values.Where(restart => SymbolEqualityComparer.Default.Equals(restart.Family, family)).Distinct();
        return kinds.SelectMany(name => _model.RoomEnemyProjectileKind.GetMembers(name).OfType<IFieldSymbol>()).SelectMany(Of).Distinct();
    }

    /// <summary>Reads each catalog entry's kind and the motion its definition names.</summary>
    /// <exception cref="InvalidDataException">An entry's key is no kind, or its definition names no one motion.</exception>
    private void ReadKindMotions(ExpressionSyntax catalog)
    {
        SemanticModel semantic = _model.ModelOf(catalog.SyntaxTree);
        foreach (AssignmentExpressionSyntax entry in catalog.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (entry.Left is not ImplicitElementAccessSyntax { ArgumentList.Arguments: [var key] })
                continue;
            if (semantic.GetSymbolInfo(key.Expression).Symbol is not IFieldSymbol kind
                || !SymbolEqualityComparer.Default.Equals(kind.ContainingType, _model.RoomEnemyProjectileKind))
                throw new InvalidDataException($"{CatalogType}.{CatalogMember} keys an entry by {key}, which is no projectile kind.");
            IFieldSymbol[] motions = entry.Right is BaseObjectCreationExpressionSyntax { ArgumentList: { } arguments }
                ? [.. arguments.Arguments
                    .Where(argument => CoreModel.ParameterOf(semantic, argument) is { } parameter
                        && SymbolEqualityComparer.Default.Equals(parameter.Type, _model.EnemyProjectileFrameBehavior))
                    .Select(argument => semantic.GetSymbolInfo(argument.Expression).Symbol).OfType<IFieldSymbol>()
                    .Where(motion => SymbolEqualityComparer.Default.Equals(motion.ContainingType, _model.EnemyProjectileFrameBehavior))]
                : [];
            if (motions is not [var named])
                throw new InvalidDataException($"{CatalogType}.{CatalogMember}'s entry for {kind.Name} names no one motion: {entry.Right}");
            _kindMotions.Add(kind.OriginalDefinition, named.OriginalDefinition);
        }
        if (_kindMotions.Count == 0)
            throw new InvalidDataException($"{CatalogType}.{CatalogMember} names no kind's motion.");
    }

    /// <summary>
    /// Where each motion an object can hold comes from, besides its kind's definition (#1881). Every place Core names a
    /// motion outside the catalog, other than to test it, can give it to an object: a cue constructed in a family's animation
    /// data is published by that family's segments; an assignment of the moving object's motion in a guarded branch, or in a
    /// method only such branches run, gives it to objects already holding the branch's motions (a Golden Torizo egg charging
    /// drops); any other naming, and the default motion (a freed or cleared object's), can give it to any object.
    /// </summary>
    private MotionSources ReadMotionSources(ExpressionSyntax catalog, List<(StatementSyntax Region, HashSet<IFieldSymbol> Motions)> regions,
        Dictionary<IMethodSymbol, HashSet<IFieldSymbol>> methods)
    {
        INamedTypeSymbol motionType = _model.EnemyProjectileFrameBehavior;
        var sources = new MotionSources(new HashSet<IFieldSymbol>(motionType.GetMembers().OfType<IFieldSymbol>()
            .Where(member => member.HasConstantValue && Convert.ToInt64(member.ConstantValue, System.Globalization.CultureInfo.InvariantCulture) == 0),
            SymbolEqualityComparer.Default));
        // The animation data each family's segments publish cues from.
        var data = new Dictionary<SyntaxNode, INamedTypeSymbol>();
        foreach (INamedTypeSymbol family in _model.Families)
        {
            foreach (ExpressionSyntax initializer in _model.ProgramFrames.AnimationData(family))
                data.TryAdd(initializer, family);
        }
        var names = new HashSet<string>(motionType.GetMembers().OfType<IFieldSymbol>().Select(member => member.Name), StringComparer.Ordinal);
        foreach (SyntaxTree tree in _model.Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (IdentifierNameSyntax name in tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                // Only a name spelled as one of the motions can name one; the rest need no binding.
                if (!names.Contains(name.Identifier.Text))
                    continue;
                semantic ??= _model.ModelOf(tree);
                if (semantic.GetSymbolInfo(name).Symbol is not IFieldSymbol named || !SymbolEqualityComparer.Default.Equals(named.ContainingType, motionType)
                    || name.Ancestors().Any(node => node == catalog || ProjectileProgramFrames.IsTest(node, name))
                    || name.Ancestors().OfType<InvocationExpressionSyntax>().Any(invocation => invocation.Expression is IdentifierNameSyntax { Identifier.Text: "nameof" }))
                {
                    continue;
                }
                IFieldSymbol motion = named.OriginalDefinition;
                if (name.Ancestors().Select(node => data.GetValueOrDefault(node)).FirstOrDefault(family => family is not null) is { } publisher)
                    Of(sources.CueSet, motion).Add(publisher);
                else if (GuardOfAssignment(semantic, name, regions, methods) is { } guard)
                    Of(sources.Derived, motion).Add(guard);
                else
                    sources.Free.Add(motion);
            }
        }
        return sources;
    }

    /// <summary>
    /// The motions the moving object holds where <paramref name="name"/> is assigned as its motion: those of the guarded
    /// branch around the assignment, or of the method only guarded branches run that holds it; null when the name is not
    /// assigned as a motion there.
    /// </summary>
    private HashSet<IFieldSymbol>? GuardOfAssignment(SemanticModel semantic, IdentifierNameSyntax name,
        List<(StatementSyntax Region, HashSet<IFieldSymbol> Motions)> regions, Dictionary<IMethodSymbol, HashSet<IFieldSymbol>> methods)
    {
        SyntaxNode value = name.Parent is MemberAccessExpressionSyntax access && access.Name == name ? access : name;
        if (value.Parent is not AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression } assignment || assignment.Right != value
            || semantic.GetTypeInfo(assignment.Left).Type is not { } assigned
            || !SymbolEqualityComparer.Default.Equals(assigned.OriginalDefinition, _model.EnemyProjectileFrameBehavior))
            return null;
        HashSet<IFieldSymbol>? guard = null;
        foreach ((StatementSyntax region, HashSet<IFieldSymbol> motions) in regions)
        {
            if (region.SyntaxTree == name.SyntaxTree && region.Span.Contains(name.Span))
                (guard ??= new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default)).UnionWith(motions);
        }
        if (guard is null && EnclosingMethod(semantic, name) is { } method && methods.TryGetValue(method, out HashSet<IFieldSymbol>? under))
            guard = new HashSet<IFieldSymbol>(under, SymbolEqualityComparer.Default);
        return guard;
    }

    /// <summary>The value for <paramref name="key"/>, created when absent.</summary>
    private static TValue Of<TValue>(Dictionary<IFieldSymbol, TValue> values, IFieldSymbol key) where TValue : new()
    {
        if (!values.TryGetValue(key, out TValue? value))
            values.Add(key, value = new TValue());
        return value;
    }

    /// <summary>Where the motions objects hold come from, besides their kinds' definitions.</summary>
    /// <param name="Free">The motions code can give any object.</param>
    private sealed record MotionSources(HashSet<IFieldSymbol> Free)
    {
        /// <summary>For each motion a family's cues set, the families whose segments publish those cues.</summary>
        public Dictionary<IFieldSymbol, HashSet<INamedTypeSymbol>> CueSet { get; } = new(SymbolEqualityComparer.Default);

        /// <summary>For each motion guarded code assigns, the motions the objects hold where it does.</summary>
        public Dictionary<IFieldSymbol, List<HashSet<IFieldSymbol>>> Derived { get; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// The motions an object holding one of <paramref name="motions"/> was given by its kind, a cue or anywhere: each of
        /// them, and, for each guarded assignment giving one, the origins of the motions held there.
        /// </summary>
        public HashSet<IFieldSymbol> Origins(IEnumerable<IFieldSymbol> motions)
        {
            var origins = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
            var pending = new Stack<IFieldSymbol>(motions.Select(motion => motion.OriginalDefinition));
            while (pending.TryPop(out IFieldSymbol? motion))
            {
                if (!origins.Add(motion))
                    continue;
                foreach (IFieldSymbol held in Derived.GetValueOrDefault(motion)?.SelectMany(guard => guard) ?? [])
                    pending.Push(held);
            }
            return origins;
        }
    }

    /// <summary>
    /// The branches of each projectile behaviour's <c>Move</c> override that run only under a test of the moving object's
    /// motion (<c>motion == M</c>, <c>motion is A or B</c>), with the motions tested.
    /// </summary>
    private IEnumerable<(StatementSyntax Region, HashSet<IFieldSymbol> Motions)> GuardedRegions()
    {
        foreach (SyntaxTree tree in _model.Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (MethodDeclarationSyntax move in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                // Only an override spelled Move can move a projectile.
                if (move.Identifier.Text != "Move" || !move.Modifiers.Any(SyntaxKind.OverrideKeyword))
                    continue;
                semantic ??= _model.ModelOf(tree);
                if (semantic.GetDeclaredSymbol(move) is not IMethodSymbol symbol || !_model.IsProjectileBehaviour(symbol.ContainingType))
                    continue;
                foreach (IfStatementSyntax branch in move.DescendantNodes().OfType<IfStatementSyntax>())
                {
                    var motions = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
                    if (TestsMotion(semantic, branch.Condition, motions))
                        yield return (branch.Statement, motions);
                }
            }
        }
    }

    /// <summary>
    /// Whether <paramref name="condition"/> tests the moving object's motion against constant motions only, adding them to
    /// <paramref name="motions"/>.
    /// </summary>
    private bool TestsMotion(SemanticModel semantic, ExpressionSyntax condition, HashSet<IFieldSymbol> motions)
    {
        switch (condition)
        {
            case ParenthesizedExpressionSyntax parenthesized:
                return TestsMotion(semantic, parenthesized.Expression, motions);
            case BinaryExpressionSyntax equality when equality.IsKind(SyntaxKind.EqualsExpression):
                IFieldSymbol? tested = IsObjectMotion(semantic, equality.Left) ? Motion(semantic, equality.Right)
                    : IsObjectMotion(semantic, equality.Right) ? Motion(semantic, equality.Left)
                    : null;
                if (tested is null)
                    return false;
                motions.Add(tested);
                return true;
            case IsPatternExpressionSyntax test when IsObjectMotion(semantic, test.Expression):
                return PatternMotions(semantic, test.Pattern, motions);
            default:
                return false;
        }
    }

    /// <summary>Whether <paramref name="pattern"/> matches constant motions only, joined by <c>or</c>, adding them to <paramref name="motions"/>.</summary>
    private bool PatternMotions(SemanticModel semantic, PatternSyntax pattern, HashSet<IFieldSymbol> motions)
    {
        switch (pattern)
        {
            case ParenthesizedPatternSyntax parenthesized:
                return PatternMotions(semantic, parenthesized.Pattern, motions);
            case BinaryPatternSyntax either when either.IsKind(SyntaxKind.OrPattern):
                return PatternMotions(semantic, either.Left, motions) & PatternMotions(semantic, either.Right, motions);
            case ConstantPatternSyntax constant when Motion(semantic, constant.Expression) is { } motion:
                motions.Add(motion);
                return true;
            default:
                return false;
        }
    }

    /// <summary>The motion <paramref name="expression"/> names as a constant; null when it names none.</summary>
    private IFieldSymbol? Motion(SemanticModel semantic, ExpressionSyntax expression) =>
        semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol motion && SymbolEqualityComparer.Default.Equals(motion.ContainingType, _model.EnemyProjectileFrameBehavior)
            ? motion.OriginalDefinition
            : null;

    /// <summary>
    /// Whether <paramref name="expression"/> reads the moving object's motion: its <c>FrameBehavior</c>, or a local holding it.
    /// </summary>
    private bool IsObjectMotion(SemanticModel semantic, ExpressionSyntax expression) => semantic.GetSymbolInfo(expression).Symbol switch
    {
        IPropertySymbol { Name: "FrameBehavior" } property => SymbolEqualityComparer.Default.Equals(property.Type, _model.EnemyProjectileFrameBehavior),
        ILocalSymbol local => local.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax()).OfType<VariableDeclaratorSyntax>()
            .Any(declarator => declarator.Initializer?.Value is { } value && IsObjectMotion(_model.ModelOf(value.SyntaxTree), value)),
        _ => false,
    };

    /// <summary>
    /// The Core methods run only from the guarded branches, with the motions under which they run: each method the branches
    /// reach whose every reference in Core is in a branch or in another such method. A method a caller may reach without
    /// naming it (an override or implementation) can run under any motion, and so can code calling it.
    /// </summary>
    private Dictionary<IMethodSymbol, HashSet<IFieldSymbol>> GuardedOnlyMethods(List<(StatementSyntax Region, HashSet<IFieldSymbol> Motions)> regions)
    {
        // The methods the branches reach.
        var candidates = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var pending = new Stack<SyntaxNode>(regions.Select(region => (SyntaxNode)region.Region));
        while (pending.TryPop(out SyntaxNode? root))
        {
            SemanticModel semantic = _model.ModelOf(root.SyntaxTree);
            foreach (SimpleNameSyntax name in root.DescendantNodesAndSelf().OfType<SimpleNameSyntax>())
            {
                if (semantic.GetSymbolInfo(name).Symbol is not IMethodSymbol method)
                    continue;
                IMethodSymbol definition = (method.ReducedFrom ?? method).OriginalDefinition;
                if (definition.MethodKind != MethodKind.Ordinary || !_model.IsSource(definition) || !candidates.Add(definition))
                    continue;
                foreach (SyntaxReference reference in definition.DeclaringSyntaxReferences)
                    pending.Push(reference.GetSyntax());
            }
        }
        // Where each candidate is referenced: a branch's motions, or the candidate whose code references it.
        var references = candidates.ToDictionary<IMethodSymbol, IMethodSymbol, List<(HashSet<IFieldSymbol>? Motions, IMethodSymbol? From)>>(
            method => method, _ => new List<(HashSet<IFieldSymbol>? Motions, IMethodSymbol? From)>(), SymbolEqualityComparer.Default);
        var outside = new HashSet<IMethodSymbol>(candidates.Where(method => method.IsVirtual || method.IsOverride || method.IsAbstract
            || method.ContainingType.AllInterfaces.SelectMany(type => type.GetMembers()).Any(member => SymbolEqualityComparer.Default.Equals(
                method.ContainingType.FindImplementationForInterfaceMember(member)?.OriginalDefinition, method))), SymbolEqualityComparer.Default);
        var names = new HashSet<string>(candidates.Select(method => method.Name), StringComparer.Ordinal);
        foreach (SyntaxTree tree in _model.Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (SimpleNameSyntax name in tree.GetRoot().DescendantNodes().OfType<SimpleNameSyntax>())
            {
                // Only a name spelled as a candidate can reference one; the rest need no binding.
                if (!names.Contains(name.Identifier.Text))
                    continue;
                semantic ??= _model.ModelOf(tree);
                SymbolInfo info = semantic.GetSymbolInfo(name);
                foreach (ISymbol symbol in info.Symbol is { } bound ? [bound] : info.CandidateSymbols)
                {
                    if (symbol is not IMethodSymbol method || (method.ReducedFrom ?? method).OriginalDefinition is not { } referenced
                        || !references.TryGetValue(referenced, out List<(HashSet<IFieldSymbol>?, IMethodSymbol?)>? sites))
                        continue;
                    if (regions.FirstOrDefault(region => region.Region.SyntaxTree == tree && region.Region.Span.Contains(name.Span)) is { Region: not null } containing)
                        sites.Add((containing.Motions, null));
                    else if (EnclosingMethod(semantic, name) is { } from && candidates.Contains(from))
                        sites.Add((null, from));
                    else
                        outside.Add(referenced);
                }
            }
        }
        // A candidate referenced from a candidate that can run unguarded can too.
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (IMethodSymbol method in candidates.Where(method => !outside.Contains(method)))
            {
                if (references[method].Any(site => site.From is { } from && outside.Contains(from)))
                    changed |= outside.Add(method);
            }
        }
        var motions = candidates.Where(method => !outside.Contains(method))
            .ToDictionary<IMethodSymbol, IMethodSymbol, HashSet<IFieldSymbol>>(method => method, _ => new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default), SymbolEqualityComparer.Default);
        changed = true;
        while (changed)
        {
            changed = false;
            foreach ((IMethodSymbol method, HashSet<IFieldSymbol> under) in motions)
            {
                foreach ((HashSet<IFieldSymbol>? branch, IMethodSymbol? from) in references[method])
                {
                    IEnumerable<IFieldSymbol> added = branch ?? (IEnumerable<IFieldSymbol>)motions[from!];
                    foreach (IFieldSymbol motion in added.ToList())
                        changed |= under.Add(motion);
                }
            }
        }
        return motions;
    }

    /// <summary>The method whose code <paramref name="node"/> is (a lambda's or local function's being its member's); null outside every method.</summary>
    private static IMethodSymbol? EnclosingMethod(SemanticModel semantic, SyntaxNode node) =>
        node.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault() is { } declaration
            && semantic.GetDeclaredSymbol(declaration) is IMethodSymbol method
            ? method.OriginalDefinition
            : null;
}
