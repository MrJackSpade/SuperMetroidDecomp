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
/// The frames shown on an object its handlers spawned with the room's graphics, and the room pictures Core's
/// <c>EnemyProjectileRoomPictureDefinitions.RoomPictures</c> lists (#2260), which draw from the shared sheet whatever
/// graphics the object was spawned with.
/// </param>
internal sealed record ProgramFrameSplit(IReadOnlySet<string> Spawned, IReadOnlySet<string> Room);

/// <summary>
/// Reads, from SuperMetroid.Core's source, what each projectile animation family's animations show (#1513, #1600): its
/// segments (the beats its <c>Segments</c> names), its animations by the segment each starts at (its <c>StartingAt</c>),
/// the frame table (the <c>ShownProjectileFrames</c> its <c>Present</c> looks a pose up in) of a family whose beats show
/// poses rather than projectile frames, and the room pictures Core draws from the shared sheet (#2260). A family's
/// programs are the segments its animations start at. An animation shows what the segments its phases play show; a
/// segment shows the projectile frames its beats name, or the table entries of the poses they construct, inlining the
/// segments' own beat helpers with their constant arguments and the pose sets they are handed, whose frames are read from
/// the pose set's construction (#2472); a pose argument no constant decides (a loop's phase) matches
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
    /// <summary>The name of Core's catalog of the room pictures (#2260), which name the frames that draw from the shared sheet.</summary>
    internal const string RoomPictureCatalog = "EnemyProjectileRoomPictureDefinitions";

    private HashSet<string>? _roomPictures;
    private readonly Dictionary<INamedTypeSymbol, HandlerStartSet> _cueStarts = new(SymbolEqualityComparer.Default);
    private HandlerStartSet? _motionHandlerStarts;

    /// <summary>The frames <paramref name="family"/>'s segments or table hold, which its programs choose among.</summary>
    public IReadOnlySet<string> TableFrames(INamedTypeSymbol family) => Of(family).TableFrames;

    /// <summary>
    /// The frames <paramref name="family"/> shows when started in <paramref name="programs"/>, or in any of its programs
    /// when none are named.
    /// </summary>
    /// <exception cref="InvalidDataException">The family's animations or table cannot be read, or name a segment or pose they lack.</exception>
    public ProgramFrameSplit Split(INamedTypeSymbol family, IReadOnlySet<string>? programs) => Shown(family, Of(family), programs, RoomPictures());

    /// <summary>
    /// The frames <paramref name="set"/>, <paramref name="family"/>'s, shows when started in <paramref name="programs"/>, or
    /// in any; each of the <paramref name="roomPictures"/> it shows draws from the shared sheet.
    /// </summary>
    private static ProgramFrameSplit Shown(INamedTypeSymbol family, FamilyProgramSet set, IReadOnlySet<string>? programs, IReadOnlySet<string> roomPictures)
    {
        var spawned = new HashSet<string>(StringComparer.Ordinal);
        var room = new HashSet<string>(StringComparer.Ordinal);
        void Add(string frame, bool roomGraphics) => (roomGraphics || roomPictures.Contains(frame) ? room : spawned).Add(frame);
        if (!set.Precise)
        {
            foreach (string frame in set.TableFrames)
                Add(frame, false);
            return new ProgramFrameSplit(spawned, room);
        }
        var visited = new HashSet<(AnimationNode Node, bool Switched)>();
        var pending = new Queue<(AnimationNode Node, bool Switched)>();
        IEnumerable<string> entries = programs ?? set.Nodes.Keys.Where(node => node.IsStart).Select(node => node.Name);
        foreach (string entry in entries.Concat(set.NamedOutside))
            pending.Enqueue((AnimationNode.Start(entry), false));
        // What the family's handlers start on objects they spawn with the room's graphics draws from the shared sheet.
        foreach (string entry in set.RoomStarted)
            pending.Enqueue((AnimationNode.Start(entry), true));
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
                    case ProgramItemKind.Continue:
                        // An animation plays its phases' segments; a segment naming a segment continues the play in the
                        // animation starting there.
                        pending.Enqueue((next.Node.IsStart ? AnimationNode.Segment(item.Value) : AnimationNode.Start(item.Value), switched));
                        break;
                    case ProgramItemKind.Cue:
                        // A cue whose handler continues the play, or whose handler starts a motion under which the motion
                        // handlers restart the object, runs those programs on the object publishing it, drawing where it
                        // draws (#1881).
                        foreach (string program in (set.CueStarts.GetValueOrDefault(item.Value) ?? []).Concat(set.MotionStarts.GetValueOrDefault(item.Value) ?? []))
                            pending.Enqueue((AnimationNode.Start(program), switched));
                        break;
                    case ProgramItemKind.ShowFrame or ProgramItemKind.ShowPoseSetFrame:
                        Add(item.Value, switched);
                        break;
                    case ProgramItemKind.ShowPose:
                        string[] shown = [.. set.Table.Where(entry => Matches(item.Value, entry.Key)).Select(entry => entry.Value)];
                        if (shown.Length == 0)
                            throw new InvalidDataException($"{family.ToDisplayString()}'s segment {next.Node.Name} shows pose {item.Value}, which its frame table lacks.");
                        foreach (string frame in shown)
                            Add(frame, switched);
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
        INamedTypeSymbol animation = TimelineAnimationOf(family);
        var segmentType = (INamedTypeSymbol)animation.TypeArguments[0];
        var frameType = (INamedTypeSymbol)animation.TypeArguments[1];
        // A family whose beats name projectile frames, outright or as a frame record naming them, shows them as they are; any
        // other presents each pose it shows.
        bool direct = SymbolEqualityComparer.Default.Equals(frameType, model.EnemyProjectileFrameId) || IsFrameRecord(frameType);
        ISymbol segments = NamedBy(family, "Segments");
        // A family whose owners start every animation by its own name (#2604: Botwoon's spit, which plays its one flight)
        // has no table of the animations starting at each segment; its animations are read from its animation data.
        ISymbol? startingAt = StartTableOf(family, segments, segmentType);
        INamedTypeSymbol animationData = (startingAt ?? segments).ContainingType;
        List<ExpressionSyntax> segmentInitializers = model.InitializersWithSpreads(segments);
        ExpressionSyntax? startInitializer = startingAt is null ? null : CoreModel.InitializerOf(startingAt);
        AssignmentExpressionSyntax[] segmentEntries = [.. segmentInitializers.SelectMany(initializer => KeyedEntries(initializer, segmentType))];
        AssignmentExpressionSyntax[] startEntries = startInitializer is null ? [] : [.. KeyedEntries(startInitializer, segmentType)];
        (ExpressionSyntax Played, string Name)[] builtStarts = startInitializer is null ? [] : [.. BuilderStarts(startInitializer, segmentType)];
        // The animations its owners start by their own identity rather than at a segment (#2044: each n00b-tube shard's,
        // keyed by the shard) are starts as well, named by their key's type and member.
        (ExpressionSyntax Initializer, INamedTypeSymbol Key)[] ownerKeyed = [.. OwnerKeyedAnimations(animationData, startingAt, segmentType)];
        (AssignmentExpressionSyntax Entry, string Name)[] ownerEntries = [.. ownerKeyed.SelectMany(owned =>
            KeyedEntries(owned.Initializer, owned.Key).Select(entry => (entry, $"{owned.Key.Name}.{KeyOf(entry)}")))];
        // So are the animations its owners start by the animation's own name (#2061: the Tourian statue's descent dust),
        // named by their member.
        (ExpressionSyntax Initializer, string Name)[] ownerNamed = [.. OwnerNamedAnimations(animationData, segmentType)];
        if (startingAt is null && ownerKeyed.Length == 0 && ownerNamed.Length == 0)
        {
            throw new InvalidDataException(
                $"{family.ToDisplayString()} names no table of the animations starting at each {segmentType.Name}, and no animation by name, this generator can read.");
        }
        (ISymbol? table, bool indexedByPose) = direct ? (null, false) : TableOf(family);
        // The programs choose the frames only when each segment and animation is an authored [Segment] = ... entry of a
        // named segment, and each beat names the projectile frame it shows or constructs the pose it presents (#1513). A
        // family whose poses are plain values (a number, a segment or pose member, a single pose), whose segments are not
        // named members (Botwoon's, Mother Brain's turret), or whose segments or animations are computed from its members
        // (Fake Kraid's projectiles) shows every frame it holds.
        bool precise = segmentType.TypeKind == TypeKind.Enum && segmentEntries.Length > 0
            && startEntries.Length + builtStarts.Length + ownerEntries.Length + ownerNamed.Length > 0
            && (direct || (table is not null && indexedByPose && IsConstructedPose(frameType)));
        // A family whose Present names its frame outright, with no table, shows only the frames its code names.
        Dictionary<string, string> entries = table is null ? new Dictionary<string, string>(StringComparer.Ordinal)
            : TableEntries(family, table, frameType, precise);
        HashSet<string> tableFrames = direct
            ? ProjectileFramesNamed([.. segmentInitializers, .. SharedRunsIn(segmentInitializers)])
            : new HashSet<string>(entries.Values, StringComparer.Ordinal);
        if (!precise)
        {
            return new FamilyProgramSet(entries, tableFrames, [], new HashSet<string>(StringComparer.Ordinal), new HashSet<string>(StringComparer.Ordinal), Precise: false,
                new Dictionary<string, HashSet<string>>(), new Dictionary<string, HashSet<string>>());
        }
        var nodes = new Dictionary<AnimationNode, List<ProgramItem>>();
        INamedTypeSymbol helpers = segments.ContainingType;
        foreach (AssignmentExpressionSyntax entry in segmentEntries)
            Add(AnimationNode.Segment(KeyOf(entry)), entry.Right);
        foreach (AssignmentExpressionSyntax entry in startEntries)
            Add(AnimationNode.Start(KeyOf(entry)), entry.Right);
        foreach ((ExpressionSyntax played, string name) in builtStarts)
            Add(AnimationNode.Start(name), played);
        foreach ((AssignmentExpressionSyntax entry, string name) in ownerEntries)
            Add(AnimationNode.Start(name), entry.Right);
        foreach ((ExpressionSyntax initializer, string name) in ownerNamed)
            Add(AnimationNode.Start(name), initializer);
        // The frames a segment's beat helpers show are the family's too (#2472): those a helper's body names (a Torizo
        // Super Missile's spin pointing up) and those it reads from the pose set it is handed (a Torizo projectile side's
        // sonic boom poses), named where the pose set is constructed rather than in the segments.
        if (direct)
        {
            tableFrames.UnionWith(nodes.Values.SelectMany(items => items)
                .Where(item => item.Kind is ProgramItemKind.ShowFrame or ProgramItemKind.ShowPoseSetFrame).Select(item => item.Value));
        }
        // An object's own cues can start it, or more of its family, in other programs (an afterburn spawning the next), so
        // whatever its cue handlers start is among the programs every start of the family runs: on the object itself, or on
        // one it spawns, which draws where the object does. One it spawns with the room's graphics draws from the room's
        // shared sheet whatever sheet the object draws from (#1854): Spore Spawn's spawners, on his own sheet, release spores
        // drawn from the shared one. The family's frame table
        // names segments only as parts of the poses it looks up (Kraid's rocks), which starts nothing. The runs of steps
        // segments share are part of the segments.
        var excluded = new List<SyntaxNode>(segmentInitializers) { startInitializer };
        excluded.AddRange(ownerKeyed.Select(owned => owned.Initializer));
        excluded.AddRange(ownerNamed.Select(owned => owned.Initializer));
        excluded.AddRange(SharedRunsIn(segmentInitializers));
        // The family's own helpers that build an animation over the segment they are handed (#2557: a n00b-tube shard's
        // flight) name segments only for the starts that call them, which read them through the call.
        excluded.AddRange(helpers.GetMembers().OfType<IMethodSymbol>()
            .Where(method => method.ReturnType is INamedTypeSymbol { Name: "Animation", TypeArguments: [var played] }
                && SymbolEqualityComparer.Default.Equals(played, segmentType))
            .SelectMany(method => method.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax())));
        if (table is not null)
            excluded.Add(CoreModel.InitializerOf(table));
        HashSet<string> starts = [.. nodes.Keys.Where(node => node.IsStart).Select(node => node.Name)];
        // A program a cue handler's case continues the play in is run by the segments publishing that cue, and one a start
        // reads from a table or a method's returns (#1881) by that start, which the walks trace: neither is a program every
        // start of the family runs.
        var cueType = (INamedTypeSymbol)animation.TypeArguments[2];
        (Dictionary<string, HashSet<string>> cueStarts, HashSet<SyntaxNode> cueNamed) = CueSectionStarts(cueType, segmentType);
        HashSet<SyntaxNode> traced = TracedValues(family, segmentType);
        HashSet<string> entered = NamedOutside(family, segmentType, excluded, name => cueNamed.Contains(name) || IsTracedValue(name, traced));
        // The restarts motion handlers make under a motion only this family's cue cases start run where a segment publishes
        // such a cue.
        Dictionary<string, HashSet<string>> motionStarts = model.KindRestarts.CueMotionRestarts(family)
            .ToDictionary(pair => pair.Key, pair => pair.Value is null ? starts : new HashSet<string>(pair.Value, StringComparer.Ordinal), StringComparer.Ordinal);
        HandlerStartSet cues = CueStarts(cueType);
        // So can the motions its kinds run (an afterburn hitting a wall switching to its final puff, #1537), except a restart
        // under a motion only some kinds start with, which the walks add where they name those kinds (#1854).
        HandlerStartSet motions = MotionHandlerStarts();
        entered.UnionWith(cues.Own.Of(family) ?? starts);
        entered.UnionWith(motions.Own.Of(family) ?? starts);
        HashSet<string> room = [.. cues.Room.Of(family) ?? starts, .. motions.Room.Of(family) ?? starts];
        var set = new FamilyProgramSet(entries, tableFrames, nodes, entered, room, Precise: true, cueStarts, motionStarts);
        // The self-check: every frame the segments or table hold is shown by some program, so no beat the reader missed
        // leaves a frame the family draws off its sheets.
        ProgramFrameSplit all = Shown(family, set, programs: null, RoomPictures());
        string[] unshown = [.. tableFrames.Where(frame => !all.Spawned.Contains(frame) && !all.Room.Contains(frame)).Order(StringComparer.Ordinal)];
        if (unshown.Length > 0)
            throw new InvalidDataException($"{family.ToDisplayString()}'s frames include frames none of its programs shows as this generator reads them: {string.Join(", ", unshown)}");
        return set;

        string KeyOf(AssignmentExpressionSyntax entry) =>
            ((IFieldSymbol)model.ModelOf(entry.SyntaxTree).GetSymbolInfo(((ImplicitElementAccessSyntax)entry.Left).ArgumentList.Arguments[0].Expression).Symbol!).Name;

        void Add(AnimationNode node, ExpressionSyntax played)
        {
            var items = new List<ProgramItem>();
            Visit(played, model.ModelOf(played.SyntaxTree), new Dictionary<string, string>(StringComparer.Ordinal), NoPoseSets, items, segmentType, frameType,
                direct, helpers, depth: 0);
            if (!nodes.TryAdd(node, items))
                throw new InvalidDataException($"{family.ToDisplayString()} lists {(node.IsStart ? "the animation starting at" : "segment")} {node.Name} twice.");
        }
    }

    /// <summary>
    /// The initializers of the dictionaries of a family's animations its owners start by their own identity (#2044): each
    /// static property or field <paramref name="animationData"/> declares, other than <paramref name="startingAt"/>, whose
    /// value maps an enum other than <paramref name="segmentType"/> to animations over it, with that enum.
    /// </summary>
    private static IEnumerable<(ExpressionSyntax Initializer, INamedTypeSymbol Key)> OwnerKeyedAnimations(
        INamedTypeSymbol animationData, ISymbol? startingAt, INamedTypeSymbol segmentType)
    {
        foreach (ISymbol member in animationData.GetMembers())
        {
            ITypeSymbol? type = member switch
            {
                IPropertySymbol { IsStatic: true } property => property.Type,
                // A property's backing field is the property's, read through it.
                IFieldSymbol { IsStatic: true, IsImplicitlyDeclared: false } field => field.Type,
                _ => null,
            };
            if (SymbolEqualityComparer.Default.Equals(member, startingAt)
                || type is not INamedTypeSymbol { TypeArguments: [INamedTypeSymbol { TypeKind: TypeKind.Enum } key, INamedTypeSymbol { Name: "Animation", TypeArguments: [var played] }] }
                || SymbolEqualityComparer.Default.Equals(key, segmentType) || !SymbolEqualityComparer.Default.Equals(played, segmentType))
                continue;
            yield return (CoreModel.InitializerOf(member), key);
        }
    }

    /// <summary>
    /// The initializers of a family's animations its owners start by the animation's own name (#2061): each static property
    /// or field <paramref name="animationData"/> declares whose value is an animation over <paramref name="segmentType"/>,
    /// with its name.
    /// </summary>
    private static IEnumerable<(ExpressionSyntax Initializer, string Name)> OwnerNamedAnimations(INamedTypeSymbol animationData, INamedTypeSymbol segmentType)
    {
        foreach (ISymbol member in animationData.GetMembers())
        {
            ITypeSymbol? type = member switch
            {
                IPropertySymbol { IsStatic: true } property => property.Type,
                // A property's backing field is the property's, read through it.
                IFieldSymbol { IsStatic: true, IsImplicitlyDeclared: false } field => field.Type,
                _ => null,
            };
            if (type is not INamedTypeSymbol { Name: "Animation", TypeArguments: [var played] } || !SymbolEqualityComparer.Default.Equals(played, segmentType))
                continue;
            yield return (CoreModel.InitializerOf(member), member.Name);
        }
    }

    /// <summary>
    /// The starts a <c>StartingAt</c> built by the segments' own builders names (#2557): each segment handed to
    /// <c>Starts</c>, <c>EachLooping</c>, <c>StartsEnding</c> or <c>EachEnding</c> starts an animation that plays it whole,
    /// read as the segment it plays.
    /// </summary>
    private IEnumerable<(ExpressionSyntax Played, string Name)> BuilderStarts(ExpressionSyntax initializer, INamedTypeSymbol segmentType)
    {
        SemanticModel semantic = model.ModelOf(initializer.SyntaxTree);
        foreach (InvocationExpressionSyntax call in initializer.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            if (semantic.GetSymbolInfo(call).Symbol is not IMethodSymbol
                {
                    ContainingType.Name: "AnimationSegments" or "AnimationSegmentPlay", Name: "Starts" or "EachLooping" or "StartsEnding" or "EachEnding",
                })
            {
                continue;
            }
            foreach (ArgumentSyntax argument in call.ArgumentList.Arguments)
            {
                if (semantic.GetSymbolInfo(argument.Expression).Symbol is IFieldSymbol field && SymbolEqualityComparer.Default.Equals(field.ContainingType, segmentType))
                    yield return (argument.Expression, field.Name);
            }
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

    /// <summary>
    /// Whether <paramref name="frameType"/> is a frame record naming the projectile frames its beat shows (#1839): every
    /// parameter of each of its constructors taking any is a projectile frame, or an optional one, as the n00b tube's frame
    /// and its shard's mirror image, which a mirroring flicker shows on alternate updates. Its beats show every frame they name.
    /// </summary>
    private bool IsFrameRecord(INamedTypeSymbol frameType)
    {
        IMethodSymbol[] constructors = [.. frameType.InstanceConstructors.Where(constructor => constructor.Parameters.Length > 0)];
        return constructors.Length > 0 && constructors.All(constructor => constructor.Parameters.All(parameter =>
            parameter.Type is INamedTypeSymbol type
            && SymbolEqualityComparer.Default.Equals(
                type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ? type.TypeArguments[0] : type, model.EnemyProjectileFrameId)));
    }

    /// <summary>Whether <paramref name="frameType"/> is a pose its programs construct from arguments: neither a plain value nor a lone pose.</summary>
    private static bool IsConstructedPose(INamedTypeSymbol frameType) =>
        frameType.SpecialType == SpecialType.None && frameType.TypeKind != TypeKind.Enum
        && frameType.InstanceConstructors.Any(constructor => constructor.Parameters.Length > 0);

    /// <summary>
    /// Records, in source order, what <paramref name="node"/> contributes to a segment or animation: the projectile frames it
    /// names or the poses its key constructions show, the room-graphics cues it publishes, and the segments it names,
    /// inlining calls of the segments' own beat helpers (<paramref name="helpers"/>) with their arguments bound: constants in
    /// <paramref name="bindings"/>, and in <paramref name="poseSets"/> the constructions of the pose sets handed to them (#2472).
    /// </summary>
    private void Visit(SyntaxNode node, SemanticModel semantic, IReadOnlyDictionary<string, string> bindings,
        IReadOnlyDictionary<string, PoseSetConstruction> poseSets, List<ProgramItem> items,
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
                Visit(shared, model.ModelOf(shared.SyntaxTree), bindings, poseSets, items, segmentType, frameType, direct, helpers, depth);
                return;
            case InvocationExpressionSyntax invocation
                when semantic.GetSymbolInfo(invocation).Symbol is IMethodSymbol helper
                     && IsBeatHelper(helper, helpers):
                if (depth > 8)
                    throw new InvalidDataException($"{helpers.ToDisplayString()}'s beat helpers nest deeper than this generator inlines: {invocation}");
                var bound = new Dictionary<string, string>(StringComparer.Ordinal);
                var boundPoseSets = new Dictionary<string, PoseSetConstruction>(StringComparer.Ordinal);
                for (int index = 0; index < invocation.ArgumentList.Arguments.Count; index++)
                {
                    ArgumentSyntax argument = invocation.ArgumentList.Arguments[index];
                    IParameterSymbol? parameter = argument.NameColon is { } named
                        ? helper.Parameters.FirstOrDefault(candidate => candidate.Name == named.Name.Identifier.Text)
                        : index < helper.Parameters.Length ? helper.Parameters[index] : null;
                    // A segment handed to a helper is read where the helper's body uses it: as part of the pose it shows,
                    // which continues nowhere, or in a cue naming where play goes on.
                    if (parameter is null || !SymbolEqualityComparer.Default.Equals(parameter.Type, segmentType))
                        Visit(argument, semantic, bindings, poseSets, items, segmentType, frameType, direct, helpers, depth);
                    if (parameter is null)
                        continue;
                    bound[parameter.Name] = ValueOf(semantic, argument.Expression, bindings);
                    // A pose set handed to a helper is read where the helper's body reads its poses.
                    if (PoseSetOf(semantic, argument.Expression, poseSets) is { } poseSet)
                        boundPoseSets[parameter.Name] = poseSet;
                }
                VisitBody(helper, bound, boundPoseSets, items, segmentType, frameType, direct, helpers, depth + 1);
                return;
            // A beat helper handed on as a method group (the hand beam's Stage in .SelectMany(Stage)) runs with arguments no
            // constant decides, so its parameters match every value.
            case IdentifierNameSyntax group
                when (semantic.GetSymbolInfo(group).Symbol ?? semantic.GetSymbolInfo(group).CandidateSymbols.FirstOrDefault()) is IMethodSymbol handedOn
                     && IsBeatHelper(handedOn, helpers):
                if (depth > 8)
                    throw new InvalidDataException($"{helpers.ToDisplayString()}'s beat helpers nest deeper than this generator inlines: {group}");
                VisitBody(handedOn, new Dictionary<string, string>(StringComparer.Ordinal), NoPoseSets, items, segmentType, frameType, direct, helpers, depth + 1);
                return;
            // A projectile frame read from a pose set is the frame the pose set's construction passes for that pose (#2472:
            // a Torizo projectile side's sonic boom, read through the side a helper is handed).
            case MemberAccessExpressionSyntax access
                when direct && semantic.GetSymbolInfo(access).Symbol is IPropertySymbol { IsStatic: false } pose
                     && SymbolEqualityComparer.Default.Equals(pose.Type, model.EnemyProjectileFrameId)
                     && PoseSetOf(semantic, access.Expression, poseSets) is { } poseSetRead:
                ExpressionSyntax passed = ConstructionArgument(poseSetRead, pose.Name)
                    ?? throw new InvalidDataException($"The pose set {access.Expression} is constructed without its {pose.Name}: {poseSetRead.Creation}");
                foreach (IdentifierNameSyntax name in passed.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
                {
                    if (poseSetRead.Semantic.GetSymbolInfo(name).Symbol is IFieldSymbol frame
                        && SymbolEqualityComparer.Default.Equals(frame.ContainingType, model.EnemyProjectileFrameId))
                        items.Add(new ProgramItem(ProgramItemKind.ShowPoseSetFrame, frame.Name));
                }
                return;
            case IdentifierNameSyntax name when semantic.GetSymbolInfo(name).Symbol is IFieldSymbol field:
                // Any other enum member may name a cue whose handler's case continues the play.
                bool cue = field.ContainingType.TypeKind == TypeKind.Enum;
                if (SymbolEqualityComparer.Default.Equals(field.ContainingType, segmentType))
                {
                    items.Add(new ProgramItem(ProgramItemKind.Continue, field.Name));
                    cue = false;
                }
                else if (direct && SymbolEqualityComparer.Default.Equals(field.ContainingType, model.EnemyProjectileFrameId))
                {
                    items.Add(new ProgramItem(ProgramItemKind.ShowFrame, field.Name));
                    cue = false;
                }
                if (cue)
                    items.Add(new ProgramItem(ProgramItemKind.Cue, CueKey(field.OriginalDefinition)));
                return;
            // A helper's segment parameter outside a pose names the segment its call was handed.
            case IdentifierNameSyntax name
                when semantic.GetSymbolInfo(name).Symbol is IParameterSymbol parameter && SymbolEqualityComparer.Default.Equals(parameter.Type, segmentType)
                     && bindings.TryGetValue(parameter.Name, out string? segment) && segment != Any:
                items.Add(new ProgramItem(ProgramItemKind.Continue, segment));
                return;
        }
        foreach (SyntaxNode child in node.ChildNodes())
            Visit(child, semantic, bindings, poseSets, items, segmentType, frameType, direct, helpers, depth);
    }

    /// <summary>No pose sets bound: an initializer read outside any helper.</summary>
    private static readonly IReadOnlyDictionary<string, PoseSetConstruction> NoPoseSets = new Dictionary<string, PoseSetConstruction>(StringComparer.Ordinal);

    /// <summary>
    /// The construction of the pose set <paramref name="expression"/> stands for (#2472): a helper parameter bound to one in
    /// <paramref name="poseSets"/>, or a static member of Core's whose initializer constructs it (<c>TorizoProjectileSide.Left</c>);
    /// null for any other expression.
    /// </summary>
    private PoseSetConstruction? PoseSetOf(SemanticModel semantic, ExpressionSyntax expression, IReadOnlyDictionary<string, PoseSetConstruction> poseSets)
    {
        switch (semantic.GetSymbolInfo(expression).Symbol)
        {
            case IParameterSymbol parameter:
                return poseSets.GetValueOrDefault(parameter.Name);
            case IPropertySymbol { IsStatic: true, Type: INamedTypeSymbol { IsRecord: true } } or IFieldSymbol { IsStatic: true, Type: INamedTypeSymbol { IsRecord: true } }:
                ISymbol member = semantic.GetSymbolInfo(expression).Symbol!.OriginalDefinition;
                if (!model.IsSource(member))
                    return null;
                foreach (SyntaxReference reference in member.DeclaringSyntaxReferences)
                {
                    SyntaxNode declaration = reference.GetSyntax();
                    ExpressionSyntax? value = declaration switch
                    {
                        PropertyDeclarationSyntax { Initializer.Value: var initializer } => initializer,
                        VariableDeclaratorSyntax { Initializer.Value: var initializer } => initializer,
                        _ => null,
                    };
                    if (value is BaseObjectCreationExpressionSyntax creation)
                        return new PoseSetConstruction(model.ModelOf(creation.SyntaxTree), creation);
                }
                return null;
            default:
                return null;
        }
    }

    /// <summary>The argument <paramref name="poseSet"/>'s construction passes for its pose <paramref name="pose"/>, by position or by name.</summary>
    private static ExpressionSyntax? ConstructionArgument(PoseSetConstruction poseSet, string pose)
    {
        if (poseSet.Semantic.GetSymbolInfo(poseSet.Creation).Symbol is not IMethodSymbol { MethodKind: MethodKind.Constructor } constructor
            || poseSet.Creation.ArgumentList is not { } arguments)
            return null;
        for (int index = 0; index < arguments.Arguments.Count; index++)
        {
            ArgumentSyntax argument = arguments.Arguments[index];
            string? parameter = argument.NameColon is { } named ? named.Name.Identifier.Text
                : index < constructor.Parameters.Length ? constructor.Parameters[index].Name : null;
            if (parameter == pose)
                return argument.Expression;
        }
        return null;
    }

    /// <summary>A pose set's construction, read in its own semantic model (#2472): the record a beat helper reads its poses from.</summary>
    /// <param name="Semantic">The semantic model of the construction's syntax tree.</param>
    /// <param name="Creation">The construction.</param>
    private sealed record PoseSetConstruction(SemanticModel Semantic, BaseObjectCreationExpressionSyntax Creation);

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

    /// <summary>
    /// Visits beat helper <paramref name="helper"/>'s body with its parameters bound as <paramref name="bound"/> and
    /// <paramref name="poseSets"/> say.
    /// </summary>
    private void VisitBody(IMethodSymbol helper, IReadOnlyDictionary<string, string> bound, IReadOnlyDictionary<string, PoseSetConstruction> poseSets,
        List<ProgramItem> items, INamedTypeSymbol segmentType, INamedTypeSymbol frameType, bool direct, INamedTypeSymbol helpers, int depth)
    {
        foreach (SyntaxReference reference in helper.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not MethodDeclarationSyntax declaration)
                continue;
            SyntaxNode? body = (SyntaxNode?)declaration.ExpressionBody ?? declaration.Body;
            if (body is not null)
                Visit(body, model.ModelOf(declaration.SyntaxTree), bound, poseSets, items, segmentType, frameType, direct, helpers, depth);
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

    /// <summary>The <c>EnemyProjectileTimelineAnimation</c> base <paramref name="family"/> derives from, with its type arguments.</summary>
    private INamedTypeSymbol TimelineAnimationOf(INamedTypeSymbol family)
    {
        for (INamedTypeSymbol? type = family; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, model.TimelineAnimation))
                return type;
        }
        throw new InvalidDataException($"{family.ToDisplayString()} does not derive from {model.TimelineAnimation.Name}.");
    }

    /// <summary>
    /// The members of <paramref name="segmentType"/> Core names outside <paramref name="excluded"/> (the family's segments,
    /// animations and frame table) and the families' starts: code naming a program can start an object in it. Tests of a
    /// program (case labels, patterns, equality) start nothing, and neither does a table pose naming the program it belongs
    /// to: the Kago bug, started in Kraid's rocks' still pose, never shows the spat rock's burst its table's keys name.
    /// </summary>
    private HashSet<string> NamedOutside(INamedTypeSymbol family, INamedTypeSymbol segmentType, IReadOnlyCollection<SyntaxNode> excluded,
        Func<IdentifierNameSyntax, bool> accounted)
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
                    || IsHandedProgram(semantic, name, family)
                    || accounted(name))
                    continue;
                named.Add(field.Name);
            }
        }
        return named;
    }

    /// <summary>
    /// The programs each cue's handler case continues the play in (#1881), by <see cref="CueKey"/>, and the program names
    /// those cases hold: in each handler of <paramref name="cueType"/>'s cues, a case of a switch over the cue whose labels
    /// name enum members (a <c>Signal</c> of a cue kind) runs only for those cues, so the programs it names are entered by
    /// the segments publishing them. A case naming no enum member leaves its programs to every start.
    /// </summary>
    private (Dictionary<string, HashSet<string>> Starts, HashSet<SyntaxNode> Named) CueSectionStarts(INamedTypeSymbol cueType, INamedTypeSymbol segmentType)
    {
        var starts = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var named = new HashSet<SyntaxNode>();
        var constantReads = new Dictionary<IFieldSymbol, HashSet<IdentifierNameSyntax>>(SymbolEqualityComparer.Default);
        foreach (SyntaxNode handler in CueHandlers(cueType))
        {
            SemanticModel semantic = model.ModelOf(handler.SyntaxTree);
            foreach (SwitchSectionSyntax section in handler.DescendantNodes().OfType<SwitchSectionSyntax>())
            {
                if (section.Parent is not SwitchStatementSyntax { Expression: var governing }
                    || semantic.GetTypeInfo(governing).Type is not { } governed
                    || !SymbolEqualityComparer.Default.Equals(governed.OriginalDefinition, cueType))
                    continue;
                string[] cues = [.. section.Labels.SelectMany(label => label.DescendantNodes().OfType<IdentifierNameSyntax>())
                    .Select(name => semantic.GetSymbolInfo(name).Symbol).OfType<IFieldSymbol>()
                    .Where(member => member.ContainingType.TypeKind == TypeKind.Enum).Select(member => CueKey(member.OriginalDefinition)).Distinct()];
                if (cues.Length == 0)
                    continue;
                Section(cues, section.Statements.SelectMany(statement => statement.DescendantNodesAndSelf()), semantic);
            }
        }
        // An identity table of cases indexed with the cue, or its kind, runs each entry for its key's cue alone (#2553).
        foreach (IdentityTableEntry entry in model.Dispatch.CueEntries(cueType))
            Section([CueKey(entry.Key)], entry.Value.DescendantNodesAndSelf(), entry.Semantic);
        // A constant only these cases read names its program for them alone: its own initializer starts nothing elsewhere.
        foreach ((IFieldSymbol constant, HashSet<IdentifierNameSyntax> reads) in constantReads)
        {
            if (References(constant).All(reads.Contains) && NamedProgram(constant, segmentType) is { } initializer)
                named.Add(initializer.Name);
        }
        return (starts, named);

        // The programs the code of a case running only for the cues it names (a switch section or an identity table's entry)
        // names are entered by the segments publishing those cues.
        void Section(string[] cues, IEnumerable<SyntaxNode> code, SemanticModel semantic)
        {
            foreach (IdentifierNameSyntax name in code.OfType<IdentifierNameSyntax>())
            {
                if (name.Ancestors().Any(node => IsTest(node, name)) || semantic.GetSymbolInfo(name).Symbol is not IFieldSymbol field)
                    continue;
                // A constant naming a program is that program (#2167: Torizo's death explosion continues in DeathSmoke).
                IFieldSymbol? program = SymbolEqualityComparer.Default.Equals(field.ContainingType, segmentType) ? field
                    : NamedProgram(field, segmentType) is { } constant ? constant.Program : null;
                if (program is null)
                    continue;
                named.Add(name);
                if (!SymbolEqualityComparer.Default.Equals(field, program))
                {
                    if (!constantReads.TryGetValue(field.OriginalDefinition, out HashSet<IdentifierNameSyntax>? reads))
                        constantReads.Add(field.OriginalDefinition, reads = []);
                    reads.Add(name);
                }
                foreach (string cue in cues)
                {
                    if (!starts.TryGetValue(cue, out HashSet<string>? programs))
                        starts.Add(cue, programs = new HashSet<string>(StringComparer.Ordinal));
                    programs.Add(program.Name);
                }
            }
        }
    }

    /// <summary>
    /// The program the constant <paramref name="constant"/> names, a member of <paramref name="segmentType"/> its initializer
    /// names outright, with that name; null for any other field.
    /// </summary>
    private (IFieldSymbol Program, IdentifierNameSyntax Name)? NamedProgram(IFieldSymbol constant, INamedTypeSymbol segmentType)
    {
        if (constant is not { IsConst: true } || !SymbolEqualityComparer.Default.Equals(constant.Type, segmentType) || !model.IsSource(constant)
            || constant.OriginalDefinition.DeclaringSyntaxReferences is not [var reference]
            || reference.GetSyntax() is not VariableDeclaratorSyntax { Initializer.Value: var value })
            return null;
        IdentifierNameSyntax? name = value switch
        {
            IdentifierNameSyntax identifier => identifier,
            MemberAccessExpressionSyntax { Name: IdentifierNameSyntax member } => member,
            _ => null,
        };
        return name is not null && model.ModelOf(value.SyntaxTree).GetSymbolInfo(name).Symbol is IFieldSymbol program
            && SymbolEqualityComparer.Default.Equals(program.ContainingType, segmentType)
            ? (program, name)
            : null;
    }

    /// <summary>Every name in Core reading <paramref name="field"/>.</summary>
    private IEnumerable<IdentifierNameSyntax> References(IFieldSymbol field)
    {
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SemanticModel? semantic = null;
            foreach (IdentifierNameSyntax name in tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (name.Identifier.Text != field.Name)
                    continue;
                semantic ??= model.ModelOf(tree);
                if (SymbolEqualityComparer.Default.Equals(semantic.GetSymbolInfo(name).Symbol?.OriginalDefinition, field.OriginalDefinition))
                    yield return name;
            }
        }
    }

    /// <summary>
    /// The values the starts of <paramref name="family"/> read their programs from in tables and methods' returns (#1881):
    /// for each construction of the family whose program the tracer follows through a table or a method, the entries and
    /// returns it read, as Bomb Torizo's statue fragments read the table of the sixteen fragment programs.
    /// </summary>
    private HashSet<SyntaxNode> TracedValues(INamedTypeSymbol family, INamedTypeSymbol segmentType)
    {
        var tracer = new ProgramTracer(model, reaches: null);
        var values = new HashSet<SyntaxNode>();
        foreach (IMethodSymbol constructor in family.OriginalDefinition.InstanceConstructors)
        {
            if (model.ProgramParameterOf(constructor, family) is not { } program)
                continue;
            foreach (CallSite site in model.CallSites(constructor))
            {
                if (site.ArgumentFor(program) is { } argument)
                    values.UnionWith(tracer.ValuesReadFor(segmentType, argument, site.Semantic));
            }
        }
        return values;
    }

    /// <summary>
    /// Whether <paramref name="name"/> is in one of <paramref name="values"/>, or is the key a dictionary initializer gives one
    /// of them under: a table's key looks a value up and starts nothing itself.
    /// </summary>
    private static bool IsTracedValue(IdentifierNameSyntax name, HashSet<SyntaxNode> values) =>
        values.Count > 0
        && (name.AncestorsAndSelf().Any(values.Contains)
            || name.Ancestors().OfType<AssignmentExpressionSyntax>().Any(entry => entry.Left is ImplicitElementAccessSyntax && values.Contains(entry.Right))
            || name.Ancestors().OfType<InitializerExpressionSyntax>().Any(entry =>
                entry.IsKind(SyntaxKind.ComplexElementInitializerExpression) && entry.Expressions is [_, var value] && values.Contains(value)));

    /// <summary>
    /// The animation data of <paramref name="family"/>: its segments' initializers with the dictionaries and shared runs
    /// they spread in, and its animations' initializer; empty when the family names no segments or animations this
    /// generator can read. A cue constructed there is published by the family's segments.
    /// </summary>
    public IReadOnlyList<ExpressionSyntax> AnimationData(INamedTypeSymbol family)
    {
        if (model.ProgramTypeOf(family) is not { } programType || ReadableNamedBy(family, "Segments") is not { } segments)
            return [];
        ISymbol? startingAt = StartTableOf(family, segments, (INamedTypeSymbol)TimelineAnimationOf(family).TypeArguments[0]);
        INamedTypeSymbol animationData = (startingAt ?? segments).ContainingType;
        ExpressionSyntax[] named = [.. OwnerNamedAnimations(animationData, programType).Select(owned => owned.Initializer)];
        if (startingAt is null && named.Length == 0)
            return [];
        List<ExpressionSyntax> initializers = model.InitializersWithSpreads(segments);
        return [.. initializers, .. SharedRunsIn(initializers), .. startingAt is null ? [] : new[] { CoreModel.InitializerOf(startingAt) },
            .. OwnerKeyedAnimations(animationData, startingAt, programType).Select(owned => owned.Initializer),
            .. named];
    }

    /// <summary>
    /// The type declaring <paramref name="family"/>'s animation data (the static segments its <c>Segments</c> returns), when it
    /// declares any this generator reads.
    /// </summary>
    public INamedTypeSymbol? AnimationDataType(INamedTypeSymbol family) => AnimationSegments(family)?.ContainingType.OriginalDefinition;

    /// <summary>The static segments <paramref name="family"/>'s <c>Segments</c> returns, when it declares any this generator reads.</summary>
    public ISymbol? AnimationSegments(INamedTypeSymbol family) => ReadableNamedBy(family, "Segments");

    /// <summary>
    /// The static table of the animations starting at each <paramref name="segmentType"/> member: the one the family declares
    /// or the one beside its <paramref name="segments"/>, whose type is a dictionary from the segment to its
    /// animation (each family names the segment it starts at and passes the animation from that table, so the table is
    /// found by its type, not through an override). Null when neither declares exactly one.
    /// </summary>
    private ISymbol? StartTableOf(INamedTypeSymbol family, ISymbol segments, INamedTypeSymbol segmentType)
    {
        ISymbol[] tables = [.. new[] { family.OriginalDefinition, segments.ContainingType.OriginalDefinition }
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .SelectMany(type => type.GetMembers())
            .Where(member => member.IsStatic && member.DeclaringSyntaxReferences.Length > 0 && IsStartTable(member switch
            {
                IPropertySymbol property => property.Type,
                IFieldSymbol field => field.Type,
                _ => null,
            }, segmentType))
            .Select(member => member.OriginalDefinition)];
        return tables.Length == 1 ? tables[0] : null;
    }

    /// <summary>Whether <paramref name="type"/> is a dictionary or identity table (#2553) from <paramref name="segmentType"/> to an animation of it.</summary>
    private static bool IsStartTable(ITypeSymbol? type, INamedTypeSymbol segmentType) =>
        type is INamedTypeSymbol { Name: "IReadOnlyDictionary" or "Dictionary" or "IdentityTable", TypeArguments: [var key, INamedTypeSymbol { Name: "Animation", TypeArguments: [var played] }] }
        && SymbolEqualityComparer.Default.Equals(key, segmentType) && SymbolEqualityComparer.Default.Equals(played, segmentType);

    /// <summary>The static member <paramref name="family"/>'s <paramref name="property"/> returns, when it declares one this generator reads.</summary>
    private ISymbol? ReadableNamedBy(INamedTypeSymbol family, string property)
    {
        foreach (IPropertySymbol declared in DeclaredIn(family, property).OfType<IPropertySymbol>())
        {
            foreach (SyntaxReference reference in declared.DeclaringSyntaxReferences)
            {
                if (reference.GetSyntax() is PropertyDeclarationSyntax { ExpressionBody.Expression: var body }
                    && model.ModelOf(body.SyntaxTree).GetSymbolInfo(body).Symbol is (IPropertySymbol or IFieldSymbol) and { IsStatic: true } member)
                    return member.OriginalDefinition;
            }
        }
        return null;
    }

    /// <summary>
    /// The programs the handlers of the cues <paramref name="cueType"/> names (registered with the methods that take them)
    /// start, through every Core method and construction they reach: the constructions there, and the typed-start kinds
    /// they name.
    /// </summary>
    private HandlerStartSet CueStarts(INamedTypeSymbol cueType)
    {
        if (!_cueStarts.TryGetValue(cueType, out HandlerStartSet? started))
        {
            started = HandlerStarts(CueHandlers(cueType));
            _cueStarts.Add(cueType, started);
        }
        return started;
    }

    /// <summary>
    /// The handlers of the cues <paramref name="cueType"/> names (#1760): every Core method that takes such a cue, as the
    /// projectile behaviours' <c>RunCue</c> implementations and the engine's shared-cue handler do.
    /// </summary>
    internal IEnumerable<SyntaxNode> CueHandlers(INamedTypeSymbol cueType)
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
    /// The programs any motion handler (a projectile behaviour's <c>Move</c> or a single-motion mover) starts, through every
    /// Core method and construction it reaches: a handler restarting a projectile restarts one of its own family's, as a
    /// Ridley afterburn's flight switches it to its final puff when it hits a wall (#1537), so, like its cues' starts, these
    /// are among the programs every start of the family runs.
    /// </summary>
    private HandlerStartSet MotionHandlerStarts() => _motionHandlerStarts ??= HandlerStarts(MotionHandlers());

    /// <summary>
    /// Every motion handler (#1760, #1843): each projectile behaviour's <c>Move</c> override, and the mover each
    /// single-motion behaviour is constructed with.
    /// </summary>
    private IEnumerable<SyntaxNode> MotionHandlers()
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
                        if (semantic.GetDeclaredSymbol(method) is IMethodSymbol { OverriddenMethod.ContainingType.Name: "EnemyProjectileBehavior" or "EnemyProjectileMotionBehavior" or "SingleMotionProjectileBehavior" })
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
    /// construction they reach, with the programs they start it in. A restart under a motion only some kinds' definitions
    /// give is those kinds' (#1854), not a program every start of its family runs. A start on an object the code spawns with
    /// the room's graphics is a room start, with the restarts the kinds it spawns run: the object draws from the room's shared
    /// sheet, whatever sheet the object running the handler draws from.
    /// </summary>
    private HandlerStartSet HandlerStarts(IEnumerable<SyntaxNode> handlers)
    {
        var own = new ProgramStarts();
        var room = new ProgramStarts();
        var spawned = new SpawnedStarts(model);
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
                        {
                            if (!model.KindRestarts.IsRestart(creation))
                                Started(spawned.Of(semantic, creation), family, model.ProgramsOf(semantic, creation, family) ?? Handed(family, creation, semantic));
                        }
                        else
                        {
                            Enter(constructor);
                        }
                        break;
                    case SimpleNameSyntax name when semantic.GetSymbolInfo(name).Symbol is { } symbol:
                        if (symbol is IMethodSymbol method)
                            Enter(method);
                        else if (symbol is IPropertySymbol or IFieldSymbol && HoldsCode(symbol))
                            EnterMember(symbol);
                        else if (symbol is IFieldSymbol kind && SymbolEqualityComparer.Default.Equals(kind.ContainingType, model.RoomEnemyProjectileKind)
                                 && model.StartedFamily(kind) is { } typedFamily && !name.Ancestors().Any(ancestor => IsTest(ancestor, name)))
                            Started(spawned.Of(semantic, name), typedFamily, model.StartedPrograms(kind));
                        break;
                }
            }
        }
        return new HandlerStartSet(own, room);

        // A program the constructing method was handed is what Core's calls of it pass (#1784): the handlers reaching it
        // reach every such call's program, as a statue particle's cue spawns its tail.
        IReadOnlySet<string>? Handed(INamedTypeSymbol family, BaseObjectCreationExpressionSyntax creation, SemanticModel semantic) =>
            creation.ArgumentList is { Arguments: [var program, ..] } && model.ProgramTypeOf(family) is { } programType
                ? new ProgramTracer(model, reaches: null).Programs(programType, program.Expression, semantic)
                : null;

        // A start on an object spawned with the room's graphics is a room start, and so is every restart its kinds run.
        void Started(StartedObject started, INamedTypeSymbol family, IReadOnlySet<string>? programs)
        {
            if (!started.IsRoomSpawn)
            {
                own.Add(family, programs);
                return;
            }
            room.Add(family, programs);
            foreach (KindRestart restart in model.KindRestarts.OfSpawn(started, family))
                room.Add(restart.Family, restart.Programs);
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

        // So is the code a property or field holds (#2553): the cases an identity table or a cue's case record runs.
        void EnterMember(ISymbol member)
        {
            ISymbol definition = member.OriginalDefinition;
            if (definition.ContainingType is not { } owner || model.IsTable(owner) || !visited.Add(definition))
                return;
            foreach (SyntaxReference reference in definition.DeclaringSyntaxReferences)
                pending.Push(reference.GetSyntax());
        }

        // A Core property or field whose declaration holds a lambda holds code its readers run.
        bool HoldsCode(ISymbol member) =>
            model.IsSource(member.OriginalDefinition)
            && member.OriginalDefinition.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax().DescendantNodes().OfType<AnonymousFunctionExpressionSyntax>().Any());
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

    /// <summary>
    /// Whether <paramref name="name"/> is the program argument of a construction of <paramref name="family"/>, a start the
    /// walks record: passed to it outright, or to the lookup of the animation it is handed (#2061: <c>StartingWith(segment)</c>).
    /// </summary>
    private static bool IsStartArgument(SemanticModel semantic, IdentifierNameSyntax name, INamedTypeSymbol family) =>
        ArgumentOf(name)?.Parent is ArgumentListSyntax { Parent: var holder }
        && (holder is InvocationExpressionSyntax { Parent: ArgumentSyntax { Parent: ArgumentListSyntax { Parent: BaseObjectCreationExpressionSyntax looked } } }
            ? looked : holder) is BaseObjectCreationExpressionSyntax creation
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
    internal static ArgumentSyntax? ArgumentOf(IdentifierNameSyntax name)
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
    /// The room pictures Core's <c>EnemyProjectileRoomPictureDefinitions.RoomPictures</c> lists, read once (#2260): the
    /// projectile frames drawn from the room's shared sheet in their standard colours whoever spawned the projectile.
    /// </summary>
    /// <exception cref="InvalidDataException">Core declares no single such catalog.</exception>
    private HashSet<string> RoomPictures()
    {
        if (_roomPictures is not null)
            return _roomPictures;
        INamedTypeSymbol[] catalogs = [.. model.Types.Where(type => type.Name == RoomPictureCatalog)];
        if (catalogs is not [var catalog] || catalog.GetMembers("RoomPictures") is not [IFieldSymbol field])
            throw new InvalidDataException("Core declares no single EnemyProjectileRoomPictureDefinitions.RoomPictures.");
        var listed = new HashSet<string>(StringComparer.Ordinal);
        foreach (SyntaxReference reference in field.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not VariableDeclaratorSyntax { Initializer.Value: var value })
                continue;
            SemanticModel semantic = model.ModelOf(value.SyntaxTree);
            foreach (ExpressionSyntax expression in value.DescendantNodes().OfType<ExpressionSyntax>())
            {
                if (semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol member
                    && SymbolEqualityComparer.Default.Equals(member.ContainingType, model.EnemyProjectileFrameId))
                    listed.Add(member.Name);
            }
        }
        return _roomPictures = listed;
    }

    /// <summary>What one beat or phase contributes.</summary>
    private enum ProgramItemKind
    {
        /// <summary>It shows a projectile frame it names.</summary>
        ShowFrame,
        /// <summary>It shows a projectile frame it reads from a pose set, named where the pose set is constructed (#2472).</summary>
        ShowPoseSetFrame,
        /// <summary>It shows a pose, which the family's frame table presents.</summary>
        ShowPose,
        /// <summary>
        /// It names a segment: an animation's phase plays it, or a segment's cue continues the play in the animation
        /// starting there.
        /// </summary>
        Continue,
        /// <summary>
        /// It names a cue member (<see cref="CueKey"/>), whose handler's case may continue the play elsewhere or start a motion
        /// the motion handlers restart the object under.
        /// </summary>
        Cue,
    }

    /// <summary>The key a cue enum member is looked up by: its type and name.</summary>
    internal static string CueKey(IFieldSymbol member) => member.ContainingType.ToDisplayString() + "." + member.Name;

    /// <summary>One thing a beat or phase contributes, in order.</summary>
    /// <param name="Kind">What it is.</param>
    /// <param name="Value">The frame or pose pattern shown, the cue published, or the segment named.</param>
    private readonly record struct ProgramItem(ProgramItemKind Kind, string Value);

    /// <summary>One family's table, segments and animations.</summary>
    /// <param name="Table">Each pose key's frame, for a family presenting poses.</param>
    /// <param name="TableFrames">Every frame the segments name or the table holds.</param>
    /// <param name="Nodes">Each segment's and animation's items, in order.</param>
    /// <param name="NamedOutside">
    /// The programs every start of the family may run besides its own: those Core names outside the segments, animations and
    /// the family's starts, and those its handlers start on the objects they move or spawn to draw where those objects do.
    /// </param>
    /// <param name="RoomStarted">The programs its handlers start on objects they spawn with the room's graphics (#1854).</param>
    /// <param name="Precise">Whether each frame a segment shows is read from it, so the programs choose the frames.</param>
    /// <param name="CueStarts">
    /// By cue (<see cref="CueKey"/>), the programs its handler's case continues the play in (#1881): a segment publishing the
    /// cue runs them, as a Golden Torizo egg's hatch cue continues it in its hatched loop.
    /// </param>
    /// <param name="MotionStarts">
    /// By cue (<see cref="CueKey"/>), the programs motion handlers restart the object in under a motion that cue's handler
    /// case starts, when only cue cases of this family's segments start it (#1881, #1843): a segment publishing the cue runs
    /// them, as a thrown super missile's flight restarts it in its impact.
    /// </param>
    private sealed record FamilyProgramSet(IReadOnlyDictionary<string, string> Table, IReadOnlySet<string> TableFrames,
        Dictionary<AnimationNode, List<ProgramItem>> Nodes, IReadOnlySet<string> NamedOutside, IReadOnlySet<string> RoomStarted, bool Precise,
        IReadOnlyDictionary<string, HashSet<string>> CueStarts, IReadOnlyDictionary<string, HashSet<string>> MotionStarts);

    /// <summary>The programs handlers start, by family: those drawing where the handlers' objects do, and room starts.</summary>
    /// <param name="Own">The programs started on the objects the handlers move, or on objects drawing where those do.</param>
    /// <param name="Room">The programs started on objects the handlers spawn with the room's graphics, and the restarts those run.</param>
    private sealed record HandlerStartSet(ProgramStarts Own, ProgramStarts Room);

    /// <summary>Programs started, by family; a family started in a program no constant decides may be started in any.</summary>
    private sealed class ProgramStarts
    {
        private readonly Dictionary<INamedTypeSymbol, HashSet<string>?> _started = new(SymbolEqualityComparer.Default);

        /// <summary>Records <paramref name="family"/> started in <paramref name="programs"/>, or in any of its programs when null.</summary>
        public void Add(INamedTypeSymbol family, IReadOnlySet<string>? programs)
        {
            if (_started.TryGetValue(family, out HashSet<string>? known) && known is null)
                return;
            if (programs is null)
                _started[family] = null;
            else if (known is null)
                _started[family] = new HashSet<string>(programs, StringComparer.Ordinal);
            else
                known.UnionWith(programs);
        }

        /// <summary>The programs <paramref name="family"/> is started in; empty when it is not, null when it may be any.</summary>
        public IReadOnlySet<string>? Of(INamedTypeSymbol family) =>
            _started.TryGetValue(family, out HashSet<string>? programs) ? programs : new HashSet<string>(StringComparer.Ordinal);
    }
}
