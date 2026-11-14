using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// The projectile frames a program family shows from the programs it is started in (#1513), split by the graphics they
/// draw under.
/// </summary>
/// <param name="Spawned">The frames shown under the graphics the object was spawned with.</param>
/// <param name="Room">
/// The frames shown once one of the programs' cues has switched the object to the room's graphics, which draw from the
/// shared sheet whatever graphics the object was spawned with.
/// </param>
internal sealed record ProgramFrameSplit(IReadOnlySet<string> Spawned, IReadOnlySet<string> Room);

/// <summary>
/// Reads, from SuperMetroid.Core's source, what each projectile animation family's animations show (#1513, #1600): its
/// segments (the beats its <c>Segments</c> names), its animations by the segment each starts at (its <c>StartingAt</c>),
/// the frame table (the <c>ShownProjectileFrames</c> its <c>Present</c> looks a pose up in) of a family whose beats show
/// poses rather than projectile frames, and the cues whose handlers switch an object to the room's graphics. A family's
/// programs are the segments its animations start at. An animation shows what the segments its phases play show; a
/// segment shows the projectile frames its beats name, or the table entries of the poses they construct, inlining the
/// segments' own beat helpers with their constant arguments; a pose argument no constant decides (a loop's phase) matches
/// every entry the pose's other arguments do. A segment its beats name (a cue continuing the play in the animation starting
/// there) is run on into. A family is started in the programs its start names, together with every one of its programs
/// Core names outside its segments, animations and starts, since code naming a program can switch an object to it. A
/// family whose segments or animations are computed, whose segments are not named members, or whose <c>Present</c> looks
/// its pose up through other state shows every frame it can.
/// </summary>
/// <param name="model">The Core facts.</param>
internal sealed class ProjectileProgramFrames(CoreModel model)
{
    /// <summary>A pose argument no constant decides.</summary>
    private const string Any = "*";

    private readonly Dictionary<INamedTypeSymbol, FamilyProgramSet> _families = new(SymbolEqualityComparer.Default);
    private HashSet<IFieldSymbol>? _roomCues;
    private HashSet<SwitchSectionSyntax>? _roomCueSections;
    private readonly Dictionary<INamedTypeSymbol, Dictionary<INamedTypeSymbol, HashSet<string>?>> _cueStarts = new(SymbolEqualityComparer.Default);
    private Dictionary<INamedTypeSymbol, HashSet<string>?>? _frameBehaviourStarts;

    /// <summary>The frames <paramref name="family"/>'s segments or table hold, which its programs choose among.</summary>
    public IReadOnlySet<string> TableFrames(INamedTypeSymbol family) => Of(family).TableFrames;

    /// <summary>
    /// Whether <paramref name="section"/> is a cue handler's case that switches an object to the room's graphics, whose
    /// effect the programs publishing its cue carry: the frames they show after it draw from the shared sheet.
    /// </summary>
    public bool IsRoomGraphicsCueSection(SwitchSectionSyntax section)
    {
        FindRoomCues();
        return _roomCueSections!.Contains(section);
    }

    /// <summary>
    /// The frames <paramref name="family"/> shows when started in <paramref name="programs"/>, or in any of its programs
    /// when none are named.
    /// </summary>
    /// <exception cref="InvalidDataException">The family's animations or table cannot be read, or name a segment or pose they lack.</exception>
    public ProgramFrameSplit Split(INamedTypeSymbol family, IReadOnlySet<string>? programs) => Shown(family, Of(family), programs);

    /// <summary>The frames <paramref name="set"/>, <paramref name="family"/>'s, shows when started in <paramref name="programs"/>, or in any.</summary>
    private static ProgramFrameSplit Shown(INamedTypeSymbol family, FamilyProgramSet set, IReadOnlySet<string>? programs)
    {
        if (!set.Precise)
            return new ProgramFrameSplit(set.TableFrames, new HashSet<string>(StringComparer.Ordinal));
        var spawned = new HashSet<string>(StringComparer.Ordinal);
        var room = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<(AnimationNode Node, bool Switched)>();
        var pending = new Queue<(AnimationNode Node, bool Switched)>();
        IEnumerable<string> entries = programs ?? set.Nodes.Keys.Where(node => node.IsStart).Select(node => node.Name);
        foreach (string entry in entries.Concat(set.NamedOutside))
            pending.Enqueue((AnimationNode.Start(entry), false));
        while (pending.TryDequeue(out (AnimationNode Node, bool Switched) next))
        {
            if (!visited.Add(next))
                continue;
            if (!set.Nodes.TryGetValue(next.Node, out List<ProgramItem>? items))
            {
                throw new InvalidDataException(next.Node.IsStart
                    ? $"{family.ToDisplayString()} is started in program {next.Node.Name}, which no animation of its StartingAt starts at."
                    : $"{family.ToDisplayString()}'s animations play segment {next.Node.Name}, which its Segments lack.");
            }
            bool switched = next.Switched;
            foreach (ProgramItem item in items)
            {
                switch (item.Kind)
                {
                    case ProgramItemKind.RoomGraphics:
                        switched = true;
                        break;
                    case ProgramItemKind.Continue:
                        // An animation plays its phases' segments; a segment naming a segment continues the play in the
                        // animation starting there.
                        pending.Enqueue((next.Node.IsStart ? AnimationNode.Segment(item.Value) : AnimationNode.Start(item.Value), switched));
                        break;
                    case ProgramItemKind.ShowFrame:
                        (switched ? room : spawned).Add(item.Value);
                        break;
                    case ProgramItemKind.ShowPose:
                        string[] shown = [.. set.Table.Where(entry => Matches(item.Value, entry.Key)).Select(entry => entry.Value)];
                        if (shown.Length == 0)
                            throw new InvalidDataException($"{family.ToDisplayString()}'s segment {next.Node.Name} shows pose {item.Value}, which its frame table lacks.");
                        (switched ? room : spawned).UnionWith(shown);
                        break;
                    default:
                        throw new InvalidOperationException($"Undefined program item kind {item.Kind}.");
                }
            }
        }
        return new ProgramFrameSplit(spawned, room);
    }

    /// <summary>Whether pose pattern <paramref name="pattern"/> names table key <paramref name="key"/>: every argument it decides is the key's.</summary>
    private static bool Matches(string pattern, string key)
    {
        string[] wanted = pattern.Split(','), held = key.Split(',');
        if (wanted.Length != held.Length)
            return false;
        for (int index = 0; index < wanted.Length; index++)
        {
            if (wanted[index] != held[index] && !wanted[index].EndsWith("=" + Any, StringComparison.Ordinal))
                return false;
        }
        return true;
    }

    /// <summary>The segments, animations and table of <paramref name="family"/>, read once.</summary>
    private FamilyProgramSet Of(INamedTypeSymbol family)
    {
        if (!_families.TryGetValue(family, out FamilyProgramSet? set))
        {
            set = Read(family);
            _families.Add(family, set);
        }
        return set;
    }

    /// <summary>Reads <paramref name="family"/>'s segments, animations, table and the programs Core names outside them.</summary>
    private FamilyProgramSet Read(INamedTypeSymbol family)
    {
        INamedTypeSymbol animation = TypedAnimationOf(family);
        var segmentType = (INamedTypeSymbol)animation.TypeArguments[0];
        var frameType = (INamedTypeSymbol)animation.TypeArguments[1];
        // A family whose beats name projectile frames shows them as they are; any other presents each pose it shows.
        bool direct = SymbolEqualityComparer.Default.Equals(frameType, model.EnemyProjectileFrameId);
        ISymbol segments = NamedBy(family, "Segments");
        ISymbol startingAt = NamedBy(family, "StartingAt");
        List<ExpressionSyntax> segmentInitializers = model.InitializersWithSpreads(segments);
        ExpressionSyntax startInitializer = CoreModel.InitializerOf(startingAt);
        AssignmentExpressionSyntax[] segmentEntries = [.. segmentInitializers.SelectMany(initializer => KeyedEntries(initializer, segmentType))];
        AssignmentExpressionSyntax[] startEntries = [.. KeyedEntries(startInitializer, segmentType)];
        (ISymbol? table, bool indexedByPose) = direct ? (null, false) : TableOf(family);
        // The programs choose the frames only when each segment and animation is an authored [Segment] = ... entry of a
        // named segment, and each beat names the projectile frame it shows or constructs the pose it presents (#1513). A
        // family whose poses are plain values (a number, a segment or pose member, a single pose), whose segments are not
        // named members (Botwoon's, Mother Brain's turret), or whose segments or animations are computed from its members
        // (Fake Kraid's projectiles) shows every frame it holds.
        bool precise = segmentType.TypeKind == TypeKind.Enum && segmentEntries.Length > 0 && startEntries.Length > 0
            && (direct || (table is not null && indexedByPose && IsConstructedPose(frameType)));
        // A family whose Present names its frame outright, with no table, shows only the frames its code names.
        Dictionary<string, string> entries = table is null ? new Dictionary<string, string>(StringComparer.Ordinal)
            : TableEntries(family, table, frameType, precise);
        HashSet<string> tableFrames = direct
            ? ProjectileFramesNamed([.. segmentInitializers, .. SharedRunsIn(segmentInitializers)])
            : new HashSet<string>(entries.Values, StringComparer.Ordinal);
        if (!precise)
            return new FamilyProgramSet(entries, tableFrames, [], new HashSet<string>(StringComparer.Ordinal), Precise: false);
        var nodes = new Dictionary<AnimationNode, List<ProgramItem>>();
        INamedTypeSymbol helpers = segments.ContainingType;
        foreach (AssignmentExpressionSyntax entry in segmentEntries)
            Add(AnimationNode.Segment(KeyOf(entry)), entry);
        foreach (AssignmentExpressionSyntax entry in startEntries)
            Add(AnimationNode.Start(KeyOf(entry)), entry);
        // An object's own cues can start it, or more of its family, in other programs (an afterburn spawning the next), so
        // whatever its cue handlers start is among the programs every start of the family runs. The family's frame table
        // names segments only as parts of the poses it looks up (Kraid's rocks), which starts nothing. The runs of steps
        // segments share are part of the segments.
        var excluded = new List<SyntaxNode>(segmentInitializers) { startInitializer };
        excluded.AddRange(SharedRunsIn(segmentInitializers));
        if (table is not null)
            excluded.Add(CoreModel.InitializerOf(table));
        HashSet<string> starts = [.. nodes.Keys.Where(node => node.IsStart).Select(node => node.Name)];
        HashSet<string> entered = NamedOutside(family, segmentType, excluded);
        entered.UnionWith(CueStarted(family, (INamedTypeSymbol)animation.TypeArguments[2]) ?? starts);
        // So can the frame behaviours its kinds run (an afterburn hitting a wall switching to its final puff, #1537).
        entered.UnionWith(FrameBehaviourStarted(family) ?? starts);
        var set = new FamilyProgramSet(entries, tableFrames, nodes, entered, Precise: true);
        // The self-check: every frame the segments or table hold is shown by some program, so no beat the reader missed
        // leaves a frame the family draws off its sheets.
        ProgramFrameSplit all = Shown(family, set, programs: null);
        string[] unshown = [.. tableFrames.Where(frame => !all.Spawned.Contains(frame) && !all.Room.Contains(frame)).Order(StringComparer.Ordinal)];
        if (unshown.Length > 0)
            throw new InvalidDataException($"{family.ToDisplayString()}'s frames include frames none of its programs shows as this generator reads them: {string.Join(", ", unshown)}");
        return set;

        string KeyOf(AssignmentExpressionSyntax entry) =>
            ((IFieldSymbol)model.ModelOf(entry.SyntaxTree).GetSymbolInfo(((ImplicitElementAccessSyntax)entry.Left).ArgumentList.Arguments[0].Expression).Symbol!).Name;

        void Add(AnimationNode node, AssignmentExpressionSyntax entry)
        {
            var items = new List<ProgramItem>();
            Visit(entry.Right, model.ModelOf(entry.SyntaxTree), new Dictionary<string, string>(StringComparer.Ordinal), items, segmentType, frameType,
                direct, helpers, depth: 0);
            if (!nodes.TryAdd(node, items))
                throw new InvalidDataException($"{family.ToDisplayString()} lists {(node.IsStart ? "the animation starting at" : "segment")} {node.Name} twice.");
        }
    }

    /// <summary>The <c>[Segment] = ...</c> entries of <paramref name="initializer"/>, each keyed by a member of <paramref name="segmentType"/>.</summary>
    private IEnumerable<AssignmentExpressionSyntax> KeyedEntries(ExpressionSyntax initializer, INamedTypeSymbol segmentType)
    {
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        return initializer.DescendantNodes().OfType<AssignmentExpressionSyntax>().Where(entry =>
            entry is { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var name] } }
            && semantic.GetSymbolInfo(name.Expression).Symbol is IFieldSymbol identity
            && SymbolEqualityComparer.Default.Equals(identity.ContainingType, segmentType));
    }

    /// <summary>The EnemyProjectileFrameId members <paramref name="initializers"/> name.</summary>
    private HashSet<string> ProjectileFramesNamed(IEnumerable<ExpressionSyntax> initializers)
    {
        var frames = new HashSet<string>(StringComparer.Ordinal);
        foreach (ExpressionSyntax initializer in initializers)
        {
            SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
            frames.UnionWith(initializer.DescendantNodes().OfType<IdentifierNameSyntax>()
                .Select(name => semantic.GetSymbolInfo(name).Symbol).OfType<IFieldSymbol>()
                .Where(field => SymbolEqualityComparer.Default.Equals(field.ContainingType, model.EnemyProjectileFrameId))
                .Select(field => field.Name));
        }
        return frames;
    }

    /// <summary>Whether <paramref name="frameType"/> is a pose its programs construct from arguments: neither a plain value nor a lone pose.</summary>
    private static bool IsConstructedPose(INamedTypeSymbol frameType) =>
        frameType.SpecialType == SpecialType.None && frameType.TypeKind != TypeKind.Enum
        && frameType.InstanceConstructors.Any(constructor => constructor.Parameters.Length > 0);

    /// <summary>
    /// Records, in source order, what <paramref name="node"/> contributes to a segment or animation: the projectile frames it
    /// names or the poses its key constructions show, the room-graphics cues it publishes, and the segments it names,
    /// inlining calls of the segments' own beat helpers (<paramref name="helpers"/>) with their arguments bound.
    /// </summary>
    private void Visit(SyntaxNode node, SemanticModel semantic, IReadOnlyDictionary<string, string> bindings, List<ProgramItem> items,
        INamedTypeSymbol segmentType, INamedTypeSymbol frameType, bool direct, INamedTypeSymbol helpers, int depth)
    {
        switch (node)
        {
            case BaseObjectCreationExpressionSyntax creation
                when !direct && semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
                     && SymbolEqualityComparer.Default.Equals(constructor.ContainingType.OriginalDefinition, frameType):
                items.Add(new ProgramItem(ProgramItemKind.ShowPose, PoseOf(semantic, creation, constructor, bindings)));
                return;
            // A run of steps several animations share, spread into a segment's beats, is read where it is declared (#1788).
            case SpreadElementSyntax { Expression: var spread } when SharedRunOf(semantic, spread) is { } shared:
                Visit(shared, model.ModelOf(shared.SyntaxTree), bindings, items, segmentType, frameType, direct, helpers, depth);
                return;
            case InvocationExpressionSyntax invocation
                when semantic.GetSymbolInfo(invocation).Symbol is IMethodSymbol helper
                     && IsBeatHelper(helper, helpers):
                if (depth > 8)
                    throw new InvalidDataException($"{helpers.ToDisplayString()}'s beat helpers nest deeper than this generator inlines: {invocation}");
                var bound = new Dictionary<string, string>(StringComparer.Ordinal);
                for (int index = 0; index < invocation.ArgumentList.Arguments.Count; index++)
                {
                    ArgumentSyntax argument = invocation.ArgumentList.Arguments[index];
                    IParameterSymbol? parameter = argument.NameColon is { } named
                        ? helper.Parameters.FirstOrDefault(candidate => candidate.Name == named.Name.Identifier.Text)
                        : index < helper.Parameters.Length ? helper.Parameters[index] : null;
                    // A segment handed to a helper is read where the helper's body uses it: as part of the pose it shows,
                    // which continues nowhere, or in a cue naming where play goes on.
                    if (parameter is null || !SymbolEqualityComparer.Default.Equals(parameter.Type, segmentType))
                        Visit(argument, semantic, bindings, items, segmentType, frameType, direct, helpers, depth);
                    if (parameter is not null)
                        bound[parameter.Name] = ValueOf(semantic, argument.Expression, bindings);
                }
                VisitBody(helper, bound, items, segmentType, frameType, direct, helpers, depth + 1);
                return;
            // A beat helper handed on as a method group (the hand beam's Stage in .SelectMany(Stage)) runs with arguments no
            // constant decides, so its parameters match every value.
            case IdentifierNameSyntax group
                when (semantic.GetSymbolInfo(group).Symbol ?? semantic.GetSymbolInfo(group).CandidateSymbols.FirstOrDefault()) is IMethodSymbol handedOn
                     && IsBeatHelper(handedOn, helpers):
                if (depth > 8)
                    throw new InvalidDataException($"{helpers.ToDisplayString()}'s beat helpers nest deeper than this generator inlines: {group}");
                VisitBody(handedOn, new Dictionary<string, string>(StringComparer.Ordinal), items, segmentType, frameType, direct, helpers, depth + 1);
                return;
            case IdentifierNameSyntax name when semantic.GetSymbolInfo(name).Symbol is IFieldSymbol field:
                FindRoomCues();
                if (_roomCues!.Contains(field.OriginalDefinition))
                    items.Add(new ProgramItem(ProgramItemKind.RoomGraphics, field.Name));
                else if (SymbolEqualityComparer.Default.Equals(field.ContainingType, segmentType))
                    items.Add(new ProgramItem(ProgramItemKind.Continue, field.Name));
                else if (direct && SymbolEqualityComparer.Default.Equals(field.ContainingType, model.EnemyProjectileFrameId))
                    items.Add(new ProgramItem(ProgramItemKind.ShowFrame, field.Name));
                return;
            // A helper's segment parameter outside a pose names the segment its call was handed.
            case IdentifierNameSyntax name
                when semantic.GetSymbolInfo(name).Symbol is IParameterSymbol parameter && SymbolEqualityComparer.Default.Equals(parameter.Type, segmentType)
                     && bindings.TryGetValue(parameter.Name, out string? segment) && segment != Any:
                items.Add(new ProgramItem(ProgramItemKind.Continue, segment));
                return;
        }
        foreach (SyntaxNode child in node.ChildNodes())
            Visit(child, semantic, bindings, items, segmentType, frameType, direct, helpers, depth);
    }

    /// <summary>
    /// The initializer of the run of shared steps <paramref name="spread"/> names: an array field or property Core declares,
    /// which several segments spread into their beats (#1788); null when it names none.
    /// </summary>
    private ExpressionSyntax? SharedRunOf(SemanticModel semantic, ExpressionSyntax spread) =>
        semantic.GetTypeInfo(spread).Type is IArrayTypeSymbol
        && semantic.GetSymbolInfo(spread).Symbol is (IFieldSymbol or IPropertySymbol) and var run
        && model.IsSource(run)
            ? CoreModel.InitializerOf(run.OriginalDefinition)
            : null;

    /// <summary>The initializers of the runs of shared steps <paramref name="initializers"/> spread in, and of those they spread in.</summary>
    private List<ExpressionSyntax> SharedRunsIn(IEnumerable<ExpressionSyntax> initializers)
    {
        var runs = new List<ExpressionSyntax>();
        var pending = new Queue<ExpressionSyntax>(initializers);
        while (pending.TryDequeue(out ExpressionSyntax? initializer))
        {
            SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
            foreach (SpreadElementSyntax spread in initializer.DescendantNodes().OfType<SpreadElementSyntax>())
            {
                if (SharedRunOf(semantic, spread.Expression) is { } run && !runs.Contains(run))
                {
                    runs.Add(run);
                    pending.Enqueue(run);
                }
            }
        }
        return runs;
    }

    /// <summary>Visits beat helper <paramref name="helper"/>'s body with its parameters bound as <paramref name="bound"/> says.</summary>
    private void VisitBody(IMethodSymbol helper, IReadOnlyDictionary<string, string> bound, List<ProgramItem> items,
        INamedTypeSymbol segmentType, INamedTypeSymbol frameType, bool direct, INamedTypeSymbol helpers, int depth)
    {
        foreach (SyntaxReference reference in helper.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not MethodDeclarationSyntax declaration)
                continue;
            SyntaxNode? body = (SyntaxNode?)declaration.ExpressionBody ?? declaration.Body;
            if (body is not null)
                Visit(body, model.ModelOf(declaration.SyntaxTree), bound, items, segmentType, frameType, direct, helpers, depth);
        }
    }

    /// <summary>
    /// Whether <paramref name="method"/> is a beat helper whose body a segment's call of it is read through: a method of the
    /// segments' type (<paramref name="helpers"/>), or any other Core method yielding beats, but not the beat type's own
    /// members.
    /// </summary>
    private bool IsBeatHelper(IMethodSymbol method, INamedTypeSymbol helpers) =>
        model.IsSource(method) && method.MethodKind == MethodKind.Ordinary && method.DeclaringSyntaxReferences.Length > 0
        && method.ContainingType.OriginalDefinition.Name != "AnimationBeat"
        && (SymbolEqualityComparer.Default.Equals(method.ContainingType.OriginalDefinition, helpers)
            || method.ReturnType.ToDisplayString().Contains("AnimationBeat", StringComparison.Ordinal));

    /// <summary>
    /// The pose <paramref name="creation"/> constructs, as <c>Parameter=Value</c> pairs in parameter order: each argument's
    /// constant, the bound value of a helper parameter it names, an omitted parameter's default, or <see cref="Any"/>.
    /// </summary>
    private static string PoseOf(SemanticModel semantic, BaseObjectCreationExpressionSyntax creation, IMethodSymbol constructor,
        IReadOnlyDictionary<string, string> bindings)
    {
        string[] values = [.. constructor.Parameters.Select(parameter => parameter.HasExplicitDefaultValue
            ? Convert.ToString(parameter.ExplicitDefaultValue, System.Globalization.CultureInfo.InvariantCulture) ?? "null" : Any)];
        SeparatedSyntaxList<ArgumentSyntax> arguments = creation.ArgumentList?.Arguments ?? default;
        for (int index = 0; index < arguments.Count; index++)
        {
            ArgumentSyntax argument = arguments[index];
            int position = argument.NameColon is { } named
                ? constructor.Parameters.IndexOf(constructor.Parameters.FirstOrDefault(parameter => parameter.Name == named.Name.Identifier.Text)!)
                : index;
            if (position >= 0 && position < values.Length)
                values[position] = ValueOf(semantic, argument.Expression, bindings);
        }
        return string.Join(',', constructor.Parameters.Select((parameter, index) => $"{parameter.Name}={values[index]}"));
    }

    /// <summary>The value <paramref name="expression"/> passes: a constant, a bound helper parameter's value, or <see cref="Any"/>.</summary>
    private static string ValueOf(SemanticModel semantic, ExpressionSyntax expression, IReadOnlyDictionary<string, string> bindings)
    {
        if (ConstantOf(semantic, expression) is { } constant)
            return constant;
        return semantic.GetSymbolInfo(expression).Symbol is IParameterSymbol parameter && bindings.TryGetValue(parameter.Name, out string? bound)
            ? bound : Any;
    }

    /// <summary>The constant <paramref name="expression"/> is: an enum member's name, or a literal or constant's value.</summary>
    private static string? ConstantOf(SemanticModel semantic, ExpressionSyntax expression) =>
        semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member
            ? member.Name
            : semantic.GetConstantValue(expression) is { HasValue: true, Value: { } value }
                ? Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) : null;

    /// <summary>
    /// The table's entries, by the constant pose key each is keyed by when <paramref name="byPose"/>, else by their order:
    /// a family whose programs do not choose its frames needs only the frames.
    /// </summary>
    private Dictionary<string, string> TableEntries(INamedTypeSymbol family, ISymbol table, INamedTypeSymbol frameType, bool byPose)
    {
        ExpressionSyntax initializer = CoreModel.InitializerOf(table);
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        var empty = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (SyntaxNode node in initializer.DescendantNodes())
        {
            (ExpressionSyntax? key, ExpressionSyntax? value) = node switch
            {
                AssignmentExpressionSyntax { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var index] }, Right: var right } => (index.Expression, right),
                InitializerExpressionSyntax { RawKind: (int)SyntaxKind.ComplexElementInitializerExpression, Expressions: [var first, var second] } => (first, second),
                _ => ((ExpressionSyntax?)null, (ExpressionSyntax?)null),
            };
            if (key is null || value is null || semantic.GetSymbolInfo(value).Symbol is not IFieldSymbol frame
                || !SymbolEqualityComparer.Default.Equals(frame.ContainingType, model.EnemyProjectileFrameId))
                continue;
            if (!byPose)
            {
                entries.Add(entries.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), frame.Name);
                continue;
            }
            // A pose is a construction of the family's pose type.
            string? pose = key is BaseObjectCreationExpressionSyntax creation
                && semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
                && SymbolEqualityComparer.Default.Equals(constructor.ContainingType.OriginalDefinition, frameType)
                ? PoseOf(semantic, creation, constructor, empty)
                : frameType.SpecialType != SpecialType.None ? ConstantOf(semantic, key) : null;
            if (pose is null || pose.Split(',').Any(part => part.EndsWith("=" + Any, StringComparison.Ordinal)))
                throw new InvalidDataException($"{family.ToDisplayString()}'s frame table keys a frame by {key}, which is no constant pose.");
            if (!entries.TryAdd(pose, frame.Name))
                throw new InvalidDataException($"{family.ToDisplayString()}'s frame table lists pose {pose} twice.");
        }
        if (entries.Count == 0)
            throw new InvalidDataException($"{family.ToDisplayString()}'s frame table {table.ToDisplayString()} has no pose entries this generator can read.");
        return entries;
    }

    /// <summary>
    /// The frame table <paramref name="family"/>'s <c>Present</c> indexes, and whether it indexes it with the pose it is
    /// handed, so that each pose a segment shows is a table key; no table when its <c>Present</c> indexes none and names
    /// its frame outright (the pre-Phantoon room's blank).
    /// </summary>
    private (ISymbol? Table, bool Precise) TableOf(INamedTypeSymbol family)
    {
        IMethodSymbol present = DeclaredIn(family, "Present").OfType<IMethodSymbol>().FirstOrDefault()
            ?? throw new InvalidDataException($"{family.ToDisplayString()} declares no Present this generator can read.");
        foreach (SyntaxReference reference in present.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not MethodDeclarationSyntax declaration)
                continue;
            SemanticModel semantic = model.ModelOf(declaration.SyntaxTree);
            foreach (ElementAccessExpressionSyntax access in declaration.DescendantNodes().OfType<ElementAccessExpressionSyntax>())
            {
                ISymbol? table = semantic.GetSymbolInfo(access.Expression).Symbol;
                if (table is not (IPropertySymbol or IFieldSymbol) || table.Name != "ShownProjectileFrames")
                    continue;
                // A plain-valued pose (a number) is shown by value, not constructed, so its segments cannot be read pose by pose.
                bool precise = access.ArgumentList.Arguments is [var argument]
                    && semantic.GetSymbolInfo(argument.Expression).Symbol is IParameterSymbol { ContainingSymbol: IMethodSymbol owner, Type.SpecialType: SpecialType.None }
                    && SymbolEqualityComparer.Default.Equals(owner.OriginalDefinition, present.OriginalDefinition);
                return (table.OriginalDefinition, precise);
            }
        }
        return (null, false);
    }

    /// <summary>
    /// The static member <paramref name="family"/>'s <paramref name="property"/> (its <c>Segments</c> or <c>StartingAt</c>
    /// override) returns.
    /// </summary>
    private ISymbol NamedBy(INamedTypeSymbol family, string property)
    {
        IPropertySymbol declared = DeclaredIn(family, property).OfType<IPropertySymbol>().FirstOrDefault()
            ?? throw new InvalidDataException($"{family.ToDisplayString()} declares no {property} this generator can read.");
        foreach (SyntaxReference reference in declared.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not PropertyDeclarationSyntax { ExpressionBody.Expression: var body })
                continue;
            if (model.ModelOf(body.SyntaxTree).GetSymbolInfo(body).Symbol is (IPropertySymbol or IFieldSymbol) and { IsStatic: true } member)
                return member.OriginalDefinition;
        }
        throw new InvalidDataException($"{family.ToDisplayString()}'s {property} returns no static property or field this generator can read.");
    }

    /// <summary>The members named <paramref name="name"/> declared in source by <paramref name="family"/> or a base class of it.</summary>
    private IEnumerable<ISymbol> DeclaredIn(INamedTypeSymbol family, string name)
    {
        for (INamedTypeSymbol? type = family; type is not null && model.IsSource(type); type = type.BaseType)
        {
            foreach (ISymbol member in type.GetMembers(name))
            {
                if (!member.IsAbstract && member.DeclaringSyntaxReferences.Length > 0)
                    yield return member;
            }
        }
    }

    /// <summary>The <c>EnemyProjectileTypedAnimation</c> base <paramref name="family"/> derives from, with its type arguments.</summary>
    private INamedTypeSymbol TypedAnimationOf(INamedTypeSymbol family)
    {
        for (INamedTypeSymbol? type = family; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, model.TypedAnimation))
                return type;
        }
        throw new InvalidDataException($"{family.ToDisplayString()} does not derive from {model.TypedAnimation.Name}.");
    }

    /// <summary>
    /// The members of <paramref name="segmentType"/> Core names outside <paramref name="excluded"/> (the family's segments,
    /// animations and frame table) and the families' starts: code naming a program can start an object in it. Tests of a
    /// program (case labels, patterns, equality) start nothing, and neither does a table pose naming the program it belongs
    /// to: the Kago bug, started in Kraid's rocks' still pose, never shows the spat rock's burst its table's keys name.
    /// </summary>
    private HashSet<string> NamedOutside(INamedTypeSymbol family, INamedTypeSymbol segmentType, IReadOnlyCollection<SyntaxNode> excluded)
    {
        var named = new HashSet<string>(StringComparer.Ordinal);
        var members = new HashSet<string>(segmentType.GetMembers().OfType<IFieldSymbol>().Select(member => member.Name), StringComparer.Ordinal);
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (IdentifierNameSyntax name in tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                // Only a name spelled as one of the segment type's members can name one; the rest need no binding.
                if (!members.Contains(name.Identifier.Text))
                    continue;
                semantic ??= model.ModelOf(tree);
                if (semantic.GetSymbolInfo(name).Symbol is not IFieldSymbol field || !SymbolEqualityComparer.Default.Equals(field.ContainingType, segmentType))
                    continue;
                if (name.Ancestors().Any(node => excluded.Contains(node) || IsTest(node, name))
                    || name.Ancestors().OfType<InvocationExpressionSyntax>().Any(invocation => invocation.Expression is IdentifierNameSyntax { Identifier.Text: "nameof" })
                    || IsStartArgument(semantic, name, family)
                    || IsHandedProgram(semantic, name, family))
                    continue;
                named.Add(field.Name);
            }
        }
        return named;
    }

    /// <summary>
    /// The programs of <paramref name="family"/> the handlers of its cues (<paramref name="cueType"/>, registered with
    /// the methods that take them) start, through every Core method and construction they reach: its constructions there, and the
    /// typed-start kinds they name that start it. Null when one of them names a program no constant decides.
    /// </summary>
    private HashSet<string>? CueStarted(INamedTypeSymbol family, INamedTypeSymbol cueType)
    {
        if (!_cueStarts.TryGetValue(cueType, out Dictionary<INamedTypeSymbol, HashSet<string>?>? started))
        {
            started = HandlerStarts(CueHandlers(cueType));
            _cueStarts.Add(cueType, started);
        }
        return started.TryGetValue(family, out HashSet<string>? programs) ? programs : new HashSet<string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// The handlers of the cues <paramref name="cueType"/> names (#1760): every Core method that takes such a cue, as the
    /// projectile behaviours' <c>RunCue</c> implementations and the engine's shared-cue handler do.
    /// </summary>
    private IEnumerable<SyntaxNode> CueHandlers(INamedTypeSymbol cueType)
    {
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (MethodDeclarationSyntax method in tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                // Only a method declaring a parameter spelled with the cue type's name can take its cues.
                if (!method.ParameterList.Parameters.Any(parameter => parameter.Type?.ToString().Contains(cueType.Name, StringComparison.Ordinal) == true))
                    continue;
                semantic ??= model.ModelOf(tree);
                if (semantic.GetDeclaredSymbol(method) is IMethodSymbol symbol
                    && symbol.Parameters.Any(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, cueType)))
                    yield return method;
            }
        }
    }

    /// <summary>
    /// The programs of <paramref name="family"/> any frame-behaviour handler (a projectile behaviour's <c>Move</c> or a single-motion mover)
    /// starts, through every Core method and construction it reaches: a handler restarting a projectile restarts one of
    /// its own family's, as a Ridley afterburn's flight switches it to its final puff when it hits a wall (#1537), so,
    /// like its cues' starts, these are among the programs every start of the family runs. Null when one of them names a
    /// program no constant decides.
    /// </summary>
    private HashSet<string>? FrameBehaviourStarted(INamedTypeSymbol family)
    {
        _frameBehaviourStarts ??= HandlerStarts(FrameBehaviourHandlers());
        return _frameBehaviourStarts.TryGetValue(family, out HashSet<string>? programs) ? programs : new HashSet<string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Every frame-behaviour handler (#1760): each projectile behaviour's <c>Move</c> override, and the mover each
    /// single-motion behaviour is constructed with.
    /// </summary>
    private IEnumerable<SyntaxNode> FrameBehaviourHandlers()
    {
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
            {
                switch (node)
                {
                    // Only an override spelled Move can move a projectile.
                    case MethodDeclarationSyntax { Identifier.Text: "Move" } method when method.Modifiers.Any(SyntaxKind.OverrideKeyword):
                        semantic ??= model.ModelOf(tree);
                        if (semantic.GetDeclaredSymbol(method) is IMethodSymbol { OverriddenMethod.ContainingType.Name: "EnemyProjectileBehavior" or "SingleMotionProjectileBehavior" })
                            yield return method;
                        break;
                    case BaseObjectCreationExpressionSyntax { ArgumentList: { } arguments } creation:
                        semantic ??= model.ModelOf(tree);
                        if (MoverArgument(semantic.GetSymbolInfo(creation).Symbol, arguments) is { } created)
                            yield return created;
                        break;
                    case PrimaryConstructorBaseTypeSyntax baseType:
                        semantic ??= model.ModelOf(tree);
                        if (MoverArgument(semantic.GetSymbolInfo(baseType).Symbol, baseType.ArgumentList) is { } inherited)
                            yield return inherited;
                        break;
                }
            }
        }

        // The argument a single-motion behaviour's constructor takes as its mover, by name or by position.
        static ExpressionSyntax? MoverArgument(ISymbol? constructor, ArgumentListSyntax arguments)
        {
            if (constructor is not IMethodSymbol { MethodKind: MethodKind.Constructor, ContainingType.Name: "SingleMotionProjectileBehavior" } method)
                return null;
            int position = method.Parameters.IndexOf(method.Parameters.Single(parameter => parameter.Name == "move"));
            return arguments.Arguments.FirstOrDefault(argument => argument.NameColon?.Name.Identifier.Text == "move")?.Expression
                ?? (arguments.Arguments.Count > position && arguments.Arguments[position].NameColon is null ? arguments.Arguments[position].Expression : null);
        }
    }

    /// <summary>
    /// Every projectile program family the handlers <paramref name="handlers"/> start, through every Core method and
    /// construction they reach, with the programs they start it in; null for a family started in a program no constant
    /// decides.
    /// </summary>
    private Dictionary<INamedTypeSymbol, HashSet<string>?> HandlerStarts(IEnumerable<SyntaxNode> handlers)
    {
        var started = new Dictionary<INamedTypeSymbol, HashSet<string>?>(SymbolEqualityComparer.Default);
        var visited = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var pending = new Stack<SyntaxNode>(handlers);
        while (pending.TryPop(out SyntaxNode? root))
        {
            SemanticModel semantic = model.ModelOf(root.SyntaxTree);
            foreach (SyntaxNode node in root.DescendantNodesAndSelf())
            {
                switch (node)
                {
                    case BaseObjectCreationExpressionSyntax creation when semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol constructor:
                        INamedTypeSymbol created = constructor.ContainingType.OriginalDefinition;
                        if (model.FamilyOf(created) is { } family && SymbolEqualityComparer.Default.Equals(family, created))
                            Started(family, model.ProgramsOf(semantic, creation, family) ?? Handed(family, creation, semantic));
                        else
                            Enter(constructor);
                        break;
                    case SimpleNameSyntax name when semantic.GetSymbolInfo(name).Symbol is { } symbol:
                        if (symbol is IMethodSymbol method)
                            Enter(method);
                        else if (symbol is IFieldSymbol kind && SymbolEqualityComparer.Default.Equals(kind.ContainingType, model.RoomEnemyProjectileKind)
                                 && model.StartedFamily(kind) is { } typedFamily && !name.Ancestors().Any(ancestor => IsTest(ancestor, name)))
                            Started(typedFamily, model.StartedPrograms(kind));
                        break;
                }
            }
        }
        return started;

        // A program the constructing method was handed is what Core's calls of it pass (#1784): the handlers reaching it
        // reach every such call's program, as a statue particle's cue spawns its tail.
        IReadOnlySet<string>? Handed(INamedTypeSymbol family, BaseObjectCreationExpressionSyntax creation, SemanticModel semantic) =>
            creation.ArgumentList is { Arguments: [var program, ..] } && model.ProgramTypeOf(family) is { } programType
                ? new ProgramTracer(model, reaches: null).Programs(programType, program.Expression, semantic)
                : null;

        // A family started in programs no constant decides may be started in any of them.
        void Started(INamedTypeSymbol family, IReadOnlySet<string>? programs)
        {
            if (started.TryGetValue(family, out HashSet<string>? known) && known is null)
                return;
            if (programs is null)
                started[family] = null;
            else if (known is null)
                started[family] = new HashSet<string>(programs, StringComparer.Ordinal);
            else
                known.UnionWith(programs);
        }

        // Code the handlers run is read once, Core's own and outside the tables naming every kind.
        void Enter(IMethodSymbol method)
        {
            IMethodSymbol definition = (method.ReducedFrom ?? method).OriginalDefinition;
            if (!model.IsSource(definition) || definition.ContainingType is not { } owner || model.IsTable(owner) || !visited.Add(definition))
                return;
            foreach (SyntaxReference reference in definition.DeclaringSyntaxReferences)
                pending.Push(reference.GetSyntax());
        }
    }

    /// <summary>Whether <paramref name="node"/>, an ancestor of <paramref name="name"/>, only tests or annotates with it: an attribute, a case or pattern, an equality.</summary>
    internal static bool IsTest(SyntaxNode node, SyntaxNode name) => node switch
    {
        AttributeListSyntax or CaseSwitchLabelSyntax or CasePatternSwitchLabelSyntax => true,
        SwitchExpressionArmSyntax arm => arm.Pattern.Span.Contains(name.Span),
        IsPatternExpressionSyntax test => test.Pattern.Span.Contains(name.Span),
        BinaryExpressionSyntax binary => binary.IsKind(SyntaxKind.EqualsExpression) || binary.IsKind(SyntaxKind.NotEqualsExpression),
        _ => false,
    };

    /// <summary>Whether <paramref name="name"/> is the program argument of a construction of <paramref name="family"/>, a start the walks record.</summary>
    private static bool IsStartArgument(SemanticModel semantic, IdentifierNameSyntax name, INamedTypeSymbol family) =>
        ArgumentOf(name)?.Parent is ArgumentListSyntax { Parent: BaseObjectCreationExpressionSyntax creation }
        && semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
        && SymbolEqualityComparer.Default.Equals(constructor.ContainingType.OriginalDefinition, family);

    /// <summary>
    /// Whether <paramref name="name"/> is passed for a parameter that hands <paramref name="family"/> its program (#1784), as
    /// a spawner's caller names the program the spawner starts: a start the walks record by tracing what each call passes.
    /// </summary>
    private bool IsHandedProgram(SemanticModel semantic, IdentifierNameSyntax name, INamedTypeSymbol family) =>
        ArgumentOf(name) is { } argument && CoreModel.ParameterOf(semantic, argument) is { } parameter
        && model.ProgramParameters(family).Contains(parameter);

    /// <summary>The argument <paramref name="name"/> is, directly or as a choice of a conditional or switch; null when it is no argument.</summary>
    private static ArgumentSyntax? ArgumentOf(IdentifierNameSyntax name)
    {
        SyntaxNode node = name.Parent is MemberAccessExpressionSyntax access && access.Name == name ? access : name;
        // A program a conditional or switch chooses is the argument too.
        while (true)
        {
            switch (node.Parent)
            {
                case ConditionalExpressionSyntax conditional when conditional.WhenTrue == node || conditional.WhenFalse == node:
                    node = conditional;
                    continue;
                case ParenthesizedExpressionSyntax parenthesized:
                    node = parenthesized;
                    continue;
                case SwitchExpressionArmSyntax arm when arm.Expression == node && arm.Parent is SwitchExpressionSyntax choice:
                    node = choice;
                    continue;
            }
            break;
        }
        return node.Parent as ArgumentSyntax;
    }

    /// <summary>
    /// Finds, once, the switch sections that switch an object to the room's graphics (<c>Graphics = SpriteGraphics.SharedStandard</c>) and the
    /// enum members their labels name: the cues whose publication switches the object.
    /// </summary>
    private void FindRoomCues()
    {
        if (_roomCues is not null)
            return;
        var cues = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var sections = new HashSet<SwitchSectionSyntax>();
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SemanticModel semantic = model.ModelOf(tree);
            foreach (SwitchSectionSyntax section in tree.GetRoot().DescendantNodes().OfType<SwitchSectionSyntax>())
            {
                if (!section.Statements.SelectMany(statement => statement.DescendantNodesAndSelf()).OfType<AssignmentExpressionSyntax>()
                        .Any(assignment => IsRoomGraphicsAssignment(semantic, assignment)))
                    continue;
                sections.Add(section);
                foreach (SwitchLabelSyntax label in section.Labels)
                {
                    foreach (IdentifierNameSyntax named in label.DescendantNodes().OfType<IdentifierNameSyntax>())
                    {
                        if (semantic.GetSymbolInfo(named).Symbol is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } member)
                            cues.Add(member.OriginalDefinition);
                    }
                }
            }
        }
        _roomCueSections = sections;
        _roomCues = cues;
    }

    /// <summary>
    /// Whether <paramref name="assignment"/> sets an object's graphics to the room's: <c>X.Graphics = default</c> or
    /// <c>X.Graphics = SpriteGraphics.SharedStandard</c>.
    /// </summary>
    private bool IsRoomGraphicsAssignment(SemanticModel semantic, AssignmentExpressionSyntax assignment) =>
        assignment is { RawKind: (int)SyntaxKind.SimpleAssignmentExpression, Left: MemberAccessExpressionSyntax { Name.Identifier.Text: "Graphics" } }
        && (assignment.Right is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.DefaultLiteralExpression } or DefaultExpressionSyntax
            || semantic.GetSymbolInfo(assignment.Right).Symbol is IPropertySymbol { Name: "SharedStandard" } shared
               && SymbolEqualityComparer.Default.Equals(shared.ContainingType, model.SpriteGraphics))
        && semantic.GetTypeInfo(assignment.Left).Type is { } assigned
        && SymbolEqualityComparer.Default.Equals(assigned.OriginalDefinition, model.SpriteGraphics);

    /// <summary>What one beat or phase contributes.</summary>
    private enum ProgramItemKind
    {
        /// <summary>It shows a projectile frame it names.</summary>
        ShowFrame,
        /// <summary>It shows a pose, which the family's frame table presents.</summary>
        ShowPose,
        /// <summary>It publishes a cue that switches the object to the room's graphics.</summary>
        RoomGraphics,
        /// <summary>
        /// It names a segment: an animation's phase plays it, or a segment's cue continues the play in the animation
        /// starting there.
        /// </summary>
        Continue,
    }

    /// <summary>One thing a beat or phase contributes, in order.</summary>
    /// <param name="Kind">What it is.</param>
    /// <param name="Value">The frame or pose pattern shown, the cue published, or the segment named.</param>
    private readonly record struct ProgramItem(ProgramItemKind Kind, string Value);

    /// <summary>One family's table, segments and animations.</summary>
    /// <param name="Table">Each pose key's frame, for a family presenting poses.</param>
    /// <param name="TableFrames">Every frame the segments name or the table holds.</param>
    /// <param name="Nodes">Each segment's and animation's items, in order.</param>
    /// <param name="NamedOutside">The programs Core names outside the segments, animations and the family's starts.</param>
    /// <param name="Precise">Whether each frame a segment shows is read from it, so the programs choose the frames.</param>
    private sealed record FamilyProgramSet(IReadOnlyDictionary<string, string> Table, IReadOnlySet<string> TableFrames,
        Dictionary<AnimationNode, List<ProgramItem>> Nodes, IReadOnlySet<string> NamedOutside, bool Precise);
}
