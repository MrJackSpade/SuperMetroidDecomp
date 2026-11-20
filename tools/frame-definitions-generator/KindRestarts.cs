using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>One program start a projectile behaviour's motion handler makes on the object it moves.</summary>
/// <param name="Family">The projectile program family started.</param>
/// <param name="Programs">The programs it is started in; null when it may be any of them.</param>
/// <param name="Motions">
/// The motions whose objects run the code making it, with those the objects were given that lead to them (#1881): the
/// motions its kinds start with or its family's cues start carry it.
/// </param>
internal sealed record KindRestart(INamedTypeSymbol Family, IReadOnlySet<string>? Programs, IReadOnlySet<IFieldSymbol> Motions);

/// <summary>
/// The restarts a projectile behaviour's motion handler (its <c>Move</c> override) makes only under a test of the moving
/// object's motion, for motions no code gives an object but its kind's start (#1854, #1843). A behaviour serving several
/// kinds runs one handler for all of them and switches over the object's motion: Spore Spawn's spawners, stalk links and
/// spores share his projectile behaviour, and only a spawner's release restarts it releasing. Such a restart runs on the
/// objects of the kinds whose starting motion (<see cref="StartingMotions"/>) is a tested motion, so it draws wherever those
/// kinds are spawned: the walks add it where they name those kinds, never to every start of its family, and a handler
/// spawning such a kind with the room's graphics carries it with that room start. A motion a case of its family's cue
/// handlers starts carries a restart too (#1881): the segments publishing that case's cue run it, as a Golden Torizo super
/// missile's throw cue starts the flight whose wall hit restarts it in its impact; and a motion guarded code starts leads
/// back to the motions held there, as a charging egg drops. A restart reached otherwise, under a motion other code can start
/// at run time (a spawner, a cue handler acting on whichever cue arrives, another family's cue), or with a switch of the
/// object's graphics, stays as the walks and the family's handler starts read it.
/// </summary>
internal sealed class KindRestarts
{
    private readonly CoreModel _model;
    private readonly StartingMotions _starting;
    private readonly Dictionary<SyntaxNode, KindRestart> _restarts = [];

    /// <summary>For each motion only cue handler cases start, the cues (<see cref="ProjectileProgramFrames.CueKey"/>) whose cases start it.</summary>
    private readonly Dictionary<IFieldSymbol, HashSet<string>> _cueStarted;

    /// <summary>
    /// Reads every kind's starting motion, where every other motion is started, and every handler restart under motions that
    /// only kinds' starts, a family's own cue cases, or code running under such motions give.
    /// </summary>
    public KindRestarts(CoreModel model)
    {
        _model = model;
        _starting = new StartingMotions(model);
        List<(SyntaxNode Region, HashSet<IFieldSymbol> Motions)> regions = [.. GuardedRegions()];
        Dictionary<IMethodSymbol, HashSet<IFieldSymbol>> methods = OnlyReachedFrom(regions);
        (List<(SyntaxNode Region, HashSet<string> Cues)> cueRegions, Dictionary<string, HashSet<INamedTypeSymbol>> publishers) = CueRegions();
        Dictionary<IMethodSymbol, HashSet<string>> cueMethods = OnlyReachedFrom(cueRegions);
        MotionSources sources = ReadMotionSources(regions, methods, cueRegions, cueMethods, publishers);
        _cueStarted = sources.CueStarted;
        var spawned = new SpawnedStarts(model);
        IEnumerable<(SyntaxNode Code, IReadOnlySet<IFieldSymbol> Motions)> code =
            regions.Select(region => (region.Region, (IReadOnlySet<IFieldSymbol>)region.Motions))
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
                    // A motion another family's cues start would carry the restart into that family's objects, which its
                    // programs do not reach; such a restart stays with every start of its family.
                    || motions.Any(motion => sources.CueSet.TryGetValue(motion, out HashSet<INamedTypeSymbol>? setters)
                        && !setters.All(setter => SymbolEqualityComparer.Default.Equals(setter, family))))
                {
                    continue;
                }
                IReadOnlySet<string>? programs = model.ProgramsOf(semantic, creation, family)
                    ?? (model.ProgramArgumentOf(semantic, creation, family) is { } program && model.ProgramTypeOf(family) is { } programType
                        ? new ProgramTracer(model, reaches: null).Programs(programType, program, semantic)
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
    }

    /// <summary>
    /// By cue (<see cref="ProjectileProgramFrames.CueKey"/>), the programs <paramref name="family"/>'s objects are restarted in
    /// under a motion only its own cue cases start (#1881); null when one restart may be in any of its programs. A segment
    /// publishing such a cue runs them.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>?> CueMotionRestarts(INamedTypeSymbol family)
    {
        var restarts = new Dictionary<string, HashSet<string>?>(StringComparer.Ordinal);
        foreach (KindRestart restart in _restarts.Values.Where(restart => SymbolEqualityComparer.Default.Equals(restart.Family, family)))
        {
            foreach (string cue in restart.Motions.Where(_cueStarted.ContainsKey).SelectMany(motion => _cueStarted[motion]))
            {
                if (!restarts.TryGetValue(cue, out HashSet<string>? programs))
                    restarts.Add(cue, programs = new HashSet<string>(StringComparer.Ordinal));
                if (programs is null)
                    continue;
                if (restart.Programs is null)
                    restarts[cue] = null;
                else
                    programs.UnionWith(restart.Programs);
            }
        }
        return restarts.ToDictionary(pair => pair.Key, pair => (IReadOnlySet<string>?)pair.Value, StringComparer.Ordinal);
    }

    /// <summary>Whether <paramref name="node"/> is a restart the kinds whose motions run it carry, rather than code its walk records.</summary>
    public bool IsRestart(SyntaxNode node) => _restarts.ContainsKey(node);

    /// <summary>The restarts the objects of <paramref name="kind"/> run: those under a motion it starts with.</summary>
    public IEnumerable<KindRestart> Of(IFieldSymbol kind)
    {
        IReadOnlySet<IFieldSymbol> motions = _starting.Of(kind);
        return motions.Count == 0 ? [] : _restarts.Values.Where(restart => restart.Motions.Any(motions.Contains));
    }

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

    /// <summary>
    /// Where each motion an object can hold is started, besides its kind's start (#1881). Every place Core starts a motion
    /// (<c>StartMotion</c>) can give it to an object: a case of a cue handler gives it to the objects of the families publishing
    /// that case's cues, and a method only such cases run to those of its cases' cues; a start in a guarded branch, or in a
    /// method only such branches run, gives it to objects already holding the branch's motions (a Golden Torizo egg charging
    /// drops); any other start, and any other naming outside a test, can give it to any object. A cue handler acting on
    /// whichever cue of its type arrives (a claw thrown by its one cue) is such another start: the objects of every family
    /// publishing that type may run it, which a restart under a motion any code starts already reads.
    /// </summary>
    private MotionSources ReadMotionSources(List<(SyntaxNode Region, HashSet<IFieldSymbol> Motions)> regions, Dictionary<IMethodSymbol, HashSet<IFieldSymbol>> methods,
        List<(SyntaxNode Region, HashSet<string> Cues)> cueRegions, Dictionary<IMethodSymbol, HashSet<string>> cueMethods,
        Dictionary<string, HashSet<INamedTypeSymbol>> publishers)
    {
        var sources = new MotionSources();
        var names = new HashSet<string>(_starting.MotionTypes.SelectMany(type => type.GetMembers().OfType<IFieldSymbol>()).Select(member => member.Name), StringComparer.Ordinal);
        foreach (SyntaxTree tree in _model.Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (IdentifierNameSyntax name in tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                // Only a name spelled as one of the motions can name one; the rest need no binding.
                if (!names.Contains(name.Identifier.Text))
                    continue;
                semantic ??= _model.ModelOf(tree);
                if (semantic.GetSymbolInfo(name).Symbol is not IFieldSymbol named || !_starting.IsMotion(named)
                    || _starting.IsStartingValue(name)
                    || name.Ancestors().Any(node => ProjectileProgramFrames.IsTest(node, name))
                    || name.Ancestors().OfType<InvocationExpressionSyntax>().Any(invocation => invocation.Expression is IdentifierNameSyntax { Identifier.Text: "nameof" }))
                {
                    continue;
                }
                IFieldSymbol motion = named.OriginalDefinition;
                if (!IsStarted(semantic, name))
                {
                    sources.Free.Add(motion);
                }
                else if (Containing(semantic, name, cueRegions, cueMethods) is { } cues)
                {
                    Of(sources.CueStarted, motion).UnionWith(cues);
                    Of(sources.CueSet, motion).UnionWith(cues.SelectMany(cue => publishers[cue]));
                }
                else if (Containing(semantic, name, regions, methods) is { } guard)
                {
                    Of(sources.Derived, motion).Add(guard);
                }
                else
                {
                    sources.Free.Add(motion);
                }
            }
        }
        return sources;
    }

    /// <summary>Whether <paramref name="name"/> is the motion an <c>EnemyProjectile.StartMotion</c> call starts, directly or as a choice of a conditional.</summary>
    private bool IsStarted(SemanticModel semantic, IdentifierNameSyntax name) =>
        ProjectileProgramFrames.ArgumentOf(name) is { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } }
        && semantic.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: "StartMotion" } start
        && SymbolEqualityComparer.Default.Equals(start.ContainingType.OriginalDefinition, _model.EnemyProjectile);

    /// <summary>
    /// The tags of the regions holding <paramref name="name"/>, or of the method only such regions run that holds it; null when
    /// it is in neither.
    /// </summary>
    private static HashSet<TTag>? Containing<TTag>(SemanticModel semantic, SyntaxNode name, List<(SyntaxNode Region, HashSet<TTag> Tags)> regions,
        Dictionary<IMethodSymbol, HashSet<TTag>> methods)
    {
        HashSet<TTag>? tags = null;
        foreach ((SyntaxNode region, HashSet<TTag> held) in regions)
        {
            if (region.SyntaxTree == name.SyntaxTree && region.Span.Contains(name.Span))
                (tags ??= []).UnionWith(held);
        }
        if (tags is null && EnclosingMethod(semantic, name) is { } method && methods.TryGetValue(method, out HashSet<TTag>? under))
            tags = [.. under];
        return tags;
    }

    /// <summary>The value for <paramref name="key"/>, created when absent.</summary>
    private static TValue Of<TValue>(Dictionary<IFieldSymbol, TValue> values, IFieldSymbol key) where TValue : new()
    {
        if (!values.TryGetValue(key, out TValue? value))
            values.Add(key, value = new TValue());
        return value;
    }

    /// <summary>Where the motions objects hold are started, besides their kinds' starts.</summary>
    private sealed class MotionSources
    {
        /// <summary>The motions code can give any object.</summary>
        public HashSet<IFieldSymbol> Free { get; } = new(SymbolEqualityComparer.Default);

        /// <summary>For each motion cue cases start, the families publishing those cues.</summary>
        public Dictionary<IFieldSymbol, HashSet<INamedTypeSymbol>> CueSet { get; } = new(SymbolEqualityComparer.Default);

        /// <summary>For each motion cue cases start, the cues whose cases start it.</summary>
        public Dictionary<IFieldSymbol, HashSet<string>> CueStarted { get; } = new(SymbolEqualityComparer.Default);

        /// <summary>For each motion guarded code starts, the motions the objects hold where it does.</summary>
        public Dictionary<IFieldSymbol, List<HashSet<IFieldSymbol>>> Derived { get; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// The motions an object holding one of <paramref name="motions"/> was given by its kind, a cue or anywhere: each of
        /// them, and, for each guarded start giving one, the origins of the motions held there.
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
    /// The code of each projectile behaviour's <c>Move</c> override that runs only under a test of the moving object's motion,
    /// with the motions tested: a case of a switch over the motion whose labels name motions only, and a branch testing it
    /// (<c>motion == M</c>, <c>motion is A or B</c>).
    /// </summary>
    private IEnumerable<(SyntaxNode Region, HashSet<IFieldSymbol> Motions)> GuardedRegions()
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
                foreach (SyntaxNode node in move.DescendantNodes())
                {
                    var motions = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
                    switch (node)
                    {
                        case IfStatementSyntax branch when TestsMotion(semantic, branch.Condition, motions):
                            yield return (branch.Statement, motions);
                            break;
                        case SwitchSectionSyntax { Parent: SwitchStatementSyntax { Expression: var governing } } section
                            when IsObjectMotion(semantic, governing) && section.Labels.All(label => LabelMotions(semantic, label, motions)):
                            yield return (section, motions);
                            break;
                    }
                }
            }
        }
    }

    /// <summary>Whether <paramref name="label"/> matches constant motions only, adding them to <paramref name="motions"/>.</summary>
    private bool LabelMotions(SemanticModel semantic, SwitchLabelSyntax label, HashSet<IFieldSymbol> motions)
    {
        switch (label)
        {
            case CaseSwitchLabelSyntax constant when Motion(semantic, constant.Value) is { } motion:
                motions.Add(motion);
                return true;
            case CasePatternSwitchLabelSyntax { WhenClause: null } pattern:
                return PatternMotions(semantic, pattern.Pattern, motions);
            default:
                return false;
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
        semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol motion && _starting.IsMotion(motion) ? motion.OriginalDefinition : null;

    /// <summary>
    /// Whether <paramref name="expression"/> reads the moving object's motion: the motion a <c>Move</c> override is handed, the
    /// projectile's <c>Motion</c>, or a local holding either.
    /// </summary>
    private bool IsObjectMotion(SemanticModel semantic, ExpressionSyntax expression) => semantic.GetSymbolInfo(expression).Symbol switch
    {
        IParameterSymbol parameter => parameter.ContainingSymbol is IMethodSymbol { Name: "Move", IsOverride: true }
            && parameter.Type is INamedTypeSymbol type && _starting.MotionTypes.Contains(type.OriginalDefinition),
        IPropertySymbol { Name: "Motion" } property => SymbolEqualityComparer.Default.Equals(property.ContainingType.OriginalDefinition, _model.EnemyProjectile),
        ILocalSymbol local => local.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax()).OfType<VariableDeclaratorSyntax>()
            .Any(declarator => declarator.Initializer?.Value is { } value && IsObjectMotion(_model.ModelOf(value.SyntaxTree), value)),
        _ => false,
    };

    /// <summary>
    /// The cases of the projectile families' cue handlers that run only for named cues, with those cues
    /// (<see cref="ProjectileProgramFrames.CueKey"/>), and the families publishing each cue: in a handler of a family's cue type,
    /// a case of a switch over the cue, or over a member of it (its kind), whose labels name enum members runs only for
    /// those cues, which only the families of that cue type publish.
    /// </summary>
    private (List<(SyntaxNode Region, HashSet<string> Cues)> Regions, Dictionary<string, HashSet<INamedTypeSymbol>> Publishers) CueRegions()
    {
        var regions = new List<(SyntaxNode, HashSet<string>)>();
        var publishers = new Dictionary<string, HashSet<INamedTypeSymbol>>(StringComparer.Ordinal);
        foreach (IGrouping<INamedTypeSymbol, INamedTypeSymbol> byCue in _model.Families
            .Where(family => _model.CueTypeOf(family) is not null)
            .GroupBy<INamedTypeSymbol, INamedTypeSymbol>(family => _model.CueTypeOf(family)!, SymbolEqualityComparer.Default))
        {
            INamedTypeSymbol cueType = byCue.Key;
            foreach (SyntaxNode handler in _model.ProgramFrames.CueHandlers(cueType))
            {
                SemanticModel semantic = _model.ModelOf(handler.SyntaxTree);
                foreach (SwitchSectionSyntax section in handler.DescendantNodes().OfType<SwitchSectionSyntax>())
                {
                    if (section.Parent is not SwitchStatementSyntax { Expression: var governing }
                        || !IsCue(semantic, governing, cueType) && !(governing is MemberAccessExpressionSyntax { Expression: var owner } && IsCue(semantic, owner, cueType)))
                        continue;
                    HashSet<string> cues = [.. section.Labels.SelectMany(label => label.DescendantNodes().OfType<IdentifierNameSyntax>())
                        .Select(name => semantic.GetSymbolInfo(name).Symbol).OfType<IFieldSymbol>()
                        .Where(member => member.ContainingType.TypeKind == TypeKind.Enum).Select(member => ProjectileProgramFrames.CueKey(member.OriginalDefinition))];
                    if (cues.Count == 0)
                        continue;
                    regions.Add((section, cues));
                    foreach (string cue in cues)
                    {
                        if (!publishers.TryGetValue(cue, out HashSet<INamedTypeSymbol>? families))
                            publishers.Add(cue, families = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default));
                        families.UnionWith(byCue);
                    }
                }
            }
        }
        return (regions, publishers);

        static bool IsCue(SemanticModel semantic, ExpressionSyntax expression, INamedTypeSymbol cueType) =>
            semantic.GetTypeInfo(expression).Type is { } type && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, cueType);
    }

    /// <summary>
    /// The Core methods run only from <paramref name="regions"/>, with the tags of the regions they run under: each method the
    /// regions reach whose every reference in Core is in a region or in another such method. A method a caller may reach
    /// without naming it (an override or implementation) can run from anywhere, and so can code calling it.
    /// </summary>
    private Dictionary<IMethodSymbol, HashSet<TTag>> OnlyReachedFrom<TTag>(List<(SyntaxNode Region, HashSet<TTag> Tags)> regions)
    {
        // The methods the regions reach.
        var candidates = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var pending = new Stack<SyntaxNode>(regions.Select(region => region.Region));
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
        // Where each candidate is referenced: a region's tags, or the candidate whose code references it.
        var references = candidates.ToDictionary<IMethodSymbol, IMethodSymbol, List<(HashSet<TTag>? Tags, IMethodSymbol? From)>>(
            method => method, _ => [], SymbolEqualityComparer.Default);
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
                        || !references.TryGetValue(referenced, out List<(HashSet<TTag>?, IMethodSymbol?)>? sites))
                        continue;
                    if (regions.FirstOrDefault(region => region.Region.SyntaxTree == tree && region.Region.Span.Contains(name.Span)) is { Region: not null } containing)
                        sites.Add((containing.Tags, null));
                    else if (EnclosingMethod(semantic, name) is { } from && candidates.Contains(from))
                        sites.Add((null, from));
                    else
                        outside.Add(referenced);
                }
            }
        }
        // A candidate referenced from a candidate that can run from anywhere can too.
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
        var tags = candidates.Where(method => !outside.Contains(method))
            .ToDictionary<IMethodSymbol, IMethodSymbol, HashSet<TTag>>(method => method, _ => [], SymbolEqualityComparer.Default);
        changed = true;
        while (changed)
        {
            changed = false;
            foreach ((IMethodSymbol method, HashSet<TTag> under) in tags)
            {
                foreach ((HashSet<TTag>? region, IMethodSymbol? from) in references[method])
                {
                    IEnumerable<TTag> added = region ?? (IEnumerable<TTag>)tags[from!];
                    foreach (TTag tag in added.ToList())
                        changed |= under.Add(tag);
                }
            }
        }
        return tags;
    }

    /// <summary>The method whose code <paramref name="node"/> is (a lambda's or local function's being its member's); null outside every method.</summary>
    private static IMethodSymbol? EnclosingMethod(SemanticModel semantic, SyntaxNode node) =>
        node.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault() is { } declaration
            && semantic.GetDeclaredSymbol(declaration) is IMethodSymbol method
            ? method.OriginalDefinition
            : null;
}
