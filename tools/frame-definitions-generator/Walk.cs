using Microsoft.CodeAnalysis;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>Whose code a reached unit runs as.</summary>
internal enum WalkContext
{
    /// <summary>The walk's own code: its behaviour or family, what they own, and shared code they run.</summary>
    Own,
    /// <summary>Another behaviour's code that the walk's own code calls directly, whose spawns are the caller's too.</summary>
    Foreign,
}

/// <summary>What the units reached in one context name.</summary>
internal sealed class Collected
{
    /// <summary>The EnemyFrameId member names.</summary>
    public HashSet<string> EnemyFrames { get; } = new(StringComparer.Ordinal);

    /// <summary>The EnemyProjectileFrameId member names.</summary>
    public HashSet<string> ProjectileFrames { get; } = new(StringComparer.Ordinal);

    /// <summary>The RoomSpriteObjectKind member names.</summary>
    public HashSet<string> SpriteObjects { get; } = new(StringComparer.Ordinal);

    /// <summary>The projectile program families constructed, or started through a typed-start kind, in any of their programs.</summary>
    public HashSet<INamedTypeSymbol> Families { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>
    /// The projectile program families started only in programs their starts name, with those programs (#1513); a family
    /// also in <see cref="Families"/> may be started in any of its programs.
    /// </summary>
    public Dictionary<INamedTypeSymbol, HashSet<string>> Programs { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>Every family started here, in any program or in named ones.</summary>
    public IEnumerable<INamedTypeSymbol> AllFamilies => Families.Concat(Programs.Keys).Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default);

    /// <summary>The programs <paramref name="family"/> is started in here, or null when it may be started in any.</summary>
    public IReadOnlySet<string>? ProgramsOf(INamedTypeSymbol family) =>
        Families.Contains(family) ? null : Programs.TryGetValue(family, out HashSet<string>? named) ? named
            : throw new InvalidOperationException($"{family.ToDisplayString()} is not started here.");

    /// <summary>Records <paramref name="family"/> started in <paramref name="programs"/>, or in any of its programs when null.</summary>
    public void AddFamily(INamedTypeSymbol family, IReadOnlySet<string>? programs)
    {
        if (programs is null)
        {
            Families.Add(family);
            return;
        }
        if (!Programs.TryGetValue(family, out HashSet<string>? named))
            Programs.Add(family, named = new HashSet<string>(StringComparer.Ordinal));
        named.UnionWith(programs);
    }

    /// <summary>Adds every family <paramref name="source"/> starts, with the programs it starts them in.</summary>
    public void AddFamiliesOf(Collected source)
    {
        Families.UnionWith(source.Families);
        foreach ((INamedTypeSymbol family, HashSet<string> programs) in source.Programs)
            AddFamily(family, programs);
    }
}

/// <summary>Which kind of sheet a destination is.</summary>
internal enum DestinationKind
{
    /// <summary>A behaviour's own spawned-object sheet.</summary>
    Own,
    /// <summary>The shared sheet of the graphics sets holding a behaviour's definitions.</summary>
    Shared,
    /// <summary>
    /// The shared sheet of every set whose rooms run a common spawner: code outside every behaviour, keyed by the spawner
    /// (<see cref="CoreModel.SpawnerKey"/>), which the import places in the rooms whose data makes that code run.
    /// </summary>
    Common,
    /// <summary>A sheet-specific owner's sheet.</summary>
    Sheet,
}

/// <summary>One sheet spawns are drawn from.</summary>
/// <param name="Kind">Which kind of sheet.</param>
/// <param name="Behaviour">For <see cref="DestinationKind.Own"/> and <see cref="DestinationKind.Shared"/>, whose.</param>
/// <param name="Sheet">For <see cref="DestinationKind.Sheet"/>, the owner property naming it.</param>
/// <param name="Spawner">For <see cref="DestinationKind.Common"/>, the common spawner whose code spawns there.</param>
internal readonly record struct Destination(DestinationKind Kind, INamedTypeSymbol? Behaviour = null, string? Sheet = null, string? Spawner = null)
{
    /// <inheritdoc/>
    public bool Equals(Destination other) =>
        Kind == other.Kind && SymbolEqualityComparer.Default.Equals(Behaviour, other.Behaviour) && Sheet == other.Sheet && Spawner == other.Spawner;

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(Kind, Behaviour is null ? 0 : SymbolEqualityComparer.Default.GetHashCode(Behaviour), Sheet, Spawner);
}

/// <summary>Graphics whose sheet only other code decides: copied within a type's objects, or a method's parameter.</summary>
/// <param name="Source">Inherited (keyed by the code's type) or Parameter (keyed by the method).</param>
/// <param name="Key">The type or method.</param>
internal readonly record struct PendingKey(GraphicsSource Source, ISymbol Key)
{
    /// <inheritdoc/>
    public bool Equals(PendingKey other) => Source == other.Source && SymbolEqualityComparer.Default.Equals(Key, other.Key);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Source, SymbolEqualityComparer.Default.GetHashCode(Key));
}

/// <summary>One spawn call a walk reached: what it calls, the type of the code making it, and where its graphics draw.</summary>
/// <param name="Callee">The method called.</param>
/// <param name="SiteType">The type whose code makes the call.</param>
/// <param name="Destination">The sheet, when the call's graphics name one.</param>
/// <param name="Pending">The pending graphics, when other code decides them.</param>
internal sealed record CallRecord(IMethodSymbol Callee, INamedTypeSymbol SiteType, Destination? Destination, PendingKey? Pending);

/// <summary>Something code that spawns nothing itself names, waiting to learn whether a caller chain spawns it.</summary>
/// <param name="Unit">The code naming it.</param>
/// <param name="Add">Adds it to a collection.</param>
/// <param name="Family">
/// The projectile program family it starts, when it starts one (by construction or a typed-start kind); null for a frame or
/// sprite object named outright.
/// </param>
/// <param name="Member">The enum member naming it, when an enum member does; null for a family the code constructs.</param>
internal sealed record UnspawnedName(Unit Unit, Action<Collected> Add, INamedTypeSymbol? Family, IFieldSymbol? Member);

/// <summary>
/// One reachability walk over Core's call graph, from a behaviour, a projectile program family, or the code outside
/// every behaviour. It follows calls, constructions, property and indexer accesses, method groups, user-defined
/// operators and conversions, static data reads (each member its own initializer, with the type's static constructor),
/// and interface and virtual dispatch to every Core implementation. Tables naming every kind or frame are never
/// entered. A projectile program family is a leaf: constructing it records the family, whose own walk supplies its
/// frames. Another behaviour's code (the behaviour type, and the types only behaviour code constructs) is never reached
/// by dispatch; a behaviour walk enters it only by a direct reference, and only to collect what it spawns. What code
/// spawns goes to every sheet its graphics can name; graphics other code decides wait in <see cref="Pending"/>.
/// </summary>
internal sealed class Walk
{
    /// <summary>
    /// EnemyFrameId members that draw nothing and are on no sheet: the empty frame, and the Norfair lava jumper's
    /// first frame, which shows the shared empty spritemap.
    /// </summary>
    public static readonly string[] ExcludedEnemyFrames = ["Empty", "NorfairLavaJumperFollowerSecond"];

    /// <summary>
    /// EnemyProjectileFrameId members that draw nothing and are on no sheet: the blank frame, and the program frames Core's
    /// <c>EnemyProjectileEmptyFrameDefinitions.EmptySpritemapFrames</c> lists as showing the empty projectile spritemap.
    /// </summary>
    public static readonly string[] ExcludedProjectileFrames =
    [
        "Blank", "EnemyDeathRespawnTail", "EnemyDeathBigExplosion", "EnemyDeathMiniKraidExplosion", "FallingSparkHitFloorSecond", "FallingSparkHitFloorFourth",
        "FallingSparkHitFloorSixth", "FallingSparkHitFloorEighth", "FallingSparkHitFloorTenth", "PhantoonRainImpactFifth", "PhantoonCasualFallingFourth", "PhantoonCasualFallingSixth",
        "TorizoDroolFourFrameDelay", "TorizoDroolTwoFrameDelay", "GoldenTorizoEyeBeamFloorImpactLoopFirst", "TourianStatueEyeGlowTenth", "BotwoonHiddenUp",
    ];


    private readonly CoreModel _model;
    private readonly UnitScanner _scanner;
    private readonly INamedTypeSymbol? _behaviour;
    private readonly INamedTypeSymbol? _family;
    private readonly DefinitionContext? _definition;
    private readonly HashSet<Unit> _own = [];
    private readonly HashSet<Unit> _foreign = [];
    private readonly HashSet<Unit> _recorded = [];
    private readonly Dictionary<Unit, HashSet<Unit>> _callers = [];
    private readonly Dictionary<Unit, Collected> _helperEvidence = [];
    private readonly List<(Unit Unit, SpawnCall Call)> _openSpriteObjectSpawns = [];
    private readonly List<(Unit Unit, string Kind)> _dataSpriteObjects = [];
    private readonly List<UnspawnedName> _unspawnedNames = [];
    private readonly Queue<(Unit Unit, WalkContext Context)> _pending = new();

    /// <summary>
    /// Creates a walk for <paramref name="behaviour"/>, for <paramref name="family"/>, or (both null) outside every
    /// behaviour; <paramref name="definition"/> evaluates the behaviour's dispatch for one of its definitions.
    /// </summary>
    public Walk(CoreModel model, UnitScanner scanner, INamedTypeSymbol? behaviour, INamedTypeSymbol? family, DefinitionContext? definition = null)
    {
        _model = model;
        _scanner = scanner;
        _behaviour = behaviour;
        _family = family;
        _definition = definition;
    }

    /// <summary>The behaviour the walk is for, or null for a family's walk or the walk outside every behaviour.</summary>
    public INamedTypeSymbol? Behaviour => _behaviour;

    /// <summary>The enemy frames the walk's own code names, and the families it constructs or starts (whose frames it shows too).</summary>
    public Collected Own { get; } = new();

    /// <summary>What the walk's code spawns, by the sheet it draws from.</summary>
    public Dictionary<Destination, Collected> Spawns { get; } = [];

    /// <summary>What the walk's code spawns with graphics other code decides, by what decides them.</summary>
    public Dictionary<PendingKey, Collected> Pending { get; } = [];

    /// <summary>Where each pending key was first met, for the self-check's report.</summary>
    public Dictionary<PendingKey, string> PendingSites { get; } = [];

    /// <summary>Every spawn call the walk reached.</summary>
    public List<CallRecord> Calls { get; } = [];

    /// <summary>
    /// The sheets graphics are reassigned to by code that spawns nothing itself (a cue or frame-behaviour handler switching
    /// an object's graphics), keyed by the behaviour whose walk runs it, or outside every behaviour by the code's type: any
    /// object that behaviour's (or type's) code spawns may draw from them.
    /// </summary>
    public Dictionary<INamedTypeSymbol, HashSet<Destination>> Reassigned { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>The same reassignments where the graphics are a method's parameter.</summary>
    public Dictionary<INamedTypeSymbol, HashSet<PendingKey>> ReassignedPending { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>Everything each type's code spawns, whatever its graphics.</summary>
    public Dictionary<INamedTypeSymbol, Collected> EvidenceByType { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>Each spawn whose graphics this generator cannot read, for the self-check.</summary>
    public List<string> Unreadable { get; } = [];

    /// <summary>Every unit reached, in either context.</summary>
    public IEnumerable<Unit> Reached => _own.Concat(_foreign);

    /// <summary>The units reached as the walk's own code.</summary>
    public IReadOnlySet<Unit> OwnUnits => _own;

    /// <summary>The reached units whose own code names a projectile frame, sprite object, started kind or family.</summary>
    public HashSet<Unit> Spawners { get; } = [];

    /// <summary>Each other behaviour's unit the walk's own code references directly, with the unit referencing it.</summary>
    public List<(Unit From, Unit To)> ForeignEntries { get; } = [];

    /// <summary>Every collection the walk spawned into, wherever it draws.</summary>
    public IEnumerable<Collected> AllSpawns => Spawns.Values.Concat(Pending.Values);

    /// <summary>The scan of <paramref name="unit"/> as this walk reads it.</summary>
    public UnitScan ScanOf(Unit unit) => _scanner.Scan(unit, _definition);

    /// <summary>Starts the walk at <paramref name="unit"/>, as the walk's own code.</summary>
    public void Root(Unit unit) => Reach(unit, WalkContext.Own, viaDispatch: false, origin: null);

    /// <summary>Follows every reference from the roots until nothing new is reached.</summary>
    public void Run()
    {
        while (_pending.Count > 0)
        {
            (Unit unit, WalkContext context) = _pending.Dequeue();
            UnitScan scan = ScanOf(unit);
            if (_recorded.Add(unit))
            {
                RecordCalls(unit, scan);
                _openSpriteObjectSpawns.AddRange(scan.SpawnCalls.Where(call => call.OpenSpriteObjectKind).Select(call => (unit, call)));
            }
            foreach (IFieldSymbol member in scan.EnumMembers)
                Record(unit, scan, member, context);
            foreach (Unit target in scan.Targets)
                Reach(target, context, viaDispatch: false, origin: unit);
        }
        RouteDataSpriteObjects();
        // Helper evidence first, so a family a helper starts on what its callers spawn is on those spawns' collections
        // when the names no spawning caller reaches look for where the walk spawns their families.
        RouteHelperEvidence();
        RouteUnspawnedNames();
    }

    /// <summary>
    /// Sends what code that spawns nothing itself names (a family it starts, a projectile frame or kind it names) to the
    /// spawner's own sheet only when no caller chain in the walk spawns it: a helper that starts a family on the object its
    /// callers spawn, as the pickup a drop's spawn starts, draws where those callers' graphics say, which
    /// <see cref="RouteHelperEvidence"/> sends it to; code no spawning caller reaches (a handler switching the program of an
    /// object the spawner's code spawned) changes an object drawing where a behaviour's code spawns that family, and a
    /// family it spawns nowhere, or a frame or kind named outright, on the spawner's sheet.
    /// </summary>
    private void RouteUnspawnedNames()
    {
        var sinks = new Dictionary<Unit, HashSet<Collected>>();
        // What the walk's own spawns hold before any unspawned name joins them.
        List<(Collected Collected, HashSet<INamedTypeSymbol> Families)> spawned = _behaviour is not null && _family is null
            ? [.. AllSpawns.Select(collected => (collected, collected.AllFamilies.ToHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default)))]
            : [];
        foreach (UnspawnedName named in _unspawnedNames)
        {
            if (SpawningCallersOf(named.Unit, sinks, []).Count > 0)
                continue;
            // A behaviour's code handing a kind to the helper that allocates and starts the object (the entrance statue's
            // decorations, the statue particles' cue handler) spawns it where that helper's spawn graphics say, never on the
            // behaviour's own sheet by default: the statue scene's objects spawn with the shared sheet's graphics (#1534).
            if (_behaviour is not null && _family is null && named.Member is { } member && HandedSpawns(named.Unit, member) is { Count: > 0 } handed)
            {
                foreach (Collected collected in handed)
                    named.Add(collected);
                continue;
            }
            // A behaviour's handler starting a program on an object of a family its code spawns (a touch, shot, frame
            // behaviour or step restarting it) changes an object drawing where that code spawned it: Mother Brain's
            // turrets, onion rings, tubes and drool, spawned with the room's graphics, draw from the shared sheet, never
            // her own (#1513).
            Collected[] spawnedThere = named.Family is { } family
                ? [.. spawned.Where(spawn => spawn.Families.Contains(family)).Select(spawn => spawn.Collected)]
                : [];
            if (spawnedThere.Length > 0)
            {
                foreach (Collected collected in spawnedThere)
                    named.Add(collected);
                continue;
            }
            foreach (SpawnTarget target in ScanOf(named.Unit).UnitTargets)
            {
                if (Collect(named.Unit, target) is { } collected)
                    named.Add(collected);
            }
        }
    }

    /// <summary>
    /// The collections the calls <paramref name="unit"/> hands <paramref name="member"/> to spawn into: each called method
    /// whose own code spawns, by every sheet its spawn calls name. A called method that spawns nothing itself but passes
    /// what it was handed on, directly or through an interface's implementations, is followed to the methods it forwards
    /// to: Mother Brain's bomb hands its afterburn kind to her room actors, which pass it to Ridley's projectile code, whose
    /// spawn names the room's shared sheet (#1537).
    /// </summary>
    private List<Collected> HandedSpawns(Unit unit, IFieldSymbol member)
    {
        var collections = new List<Collected>();
        if (!ScanOf(unit).HandedTo.TryGetValue(member, out HashSet<Unit>? callees))
            return collections;
        var visited = new HashSet<Unit>();
        var pending = new Queue<Unit>(callees);
        while (pending.TryDequeue(out Unit callee))
        {
            if (!visited.Add(callee))
                continue;
            foreach (ISymbol implementation in _model.Implementations(callee.Symbol))
                pending.Enqueue(Unit.Member(implementation));
            UnitScan scan = ScanOf(callee);
            if (scan.SpawnCalls.Count == 0)
            {
                foreach (Unit forwarded in scan.ForwardsTo)
                    pending.Enqueue(forwarded);
                continue;
            }
            foreach (SpawnTarget target in scan.UnitTargets)
            {
                if (Collect(callee, target) is { } collected && !collections.Contains(collected))
                    collections.Add(collected);
            }
        }
        return collections;
    }

    /// <summary>
    /// Sends each room sprite-object kind named by code that spawns nothing (a cue or frame table carrying the kind) to the
    /// sheets of the walk's spawn calls whose kind is not a constant, since only such a call spawns a kind other code
    /// chose; a walk without one routes them as other helper evidence.
    /// </summary>
    private void RouteDataSpriteObjects()
    {
        bool common = _behaviour is null && _family is null;
        foreach ((Unit unit, string kind) in _dataSpriteObjects)
        {
            if (common)
            {
                // Outside every behaviour each spawner's objects exist only in its own rooms, so a kind carried by data goes
                // to the open spawns whose code reads that data (the escape explosions' table to the escape explosion), not
                // to every open spawn the walk met.
                HashSet<Unit> readers = AncestorsOf(unit);
                var reading = _openSpriteObjectSpawns.Where(open => readers.Contains(open.Unit)).ToList();
                if (reading.Count == 0)
                    Unreadable.Add($"{_model.Describe(unit)}: sprite object {kind} is carried by data no open spawn outside the behaviours reads");
                foreach ((Unit spawning, SpawnCall call) in reading)
                {
                    foreach (SpawnTarget target in call.Targets)
                        Collect(spawning, target)?.SpriteObjects.Add(kind);
                }
                continue;
            }
            if (_openSpriteObjectSpawns.Count == 0)
            {
                foreach (SpawnTarget target in ScanOf(unit).UnitTargets)
                    Collect(unit, target)?.SpriteObjects.Add(kind);
                Of(_helperEvidence, unit).SpriteObjects.Add(kind);
                continue;
            }
            foreach ((Unit spawning, SpawnCall call) in _openSpriteObjectSpawns)
            {
                foreach (SpawnTarget target in call.Targets)
                    Collect(spawning, target)?.SpriteObjects.Add(kind);
            }
        }
    }

    /// <summary>Every unit that reaches <paramref name="unit"/> in this walk, directly or through others.</summary>
    private HashSet<Unit> AncestorsOf(Unit unit)
    {
        var ancestors = new HashSet<Unit>();
        var pending = new Stack<Unit>([unit]);
        while (pending.Count > 0)
        {
            foreach (Unit caller in CallersOf(pending.Pop()))
            {
                if (ancestors.Add(caller))
                    pending.Push(caller);
            }
        }
        return ancestors;
    }

    /// <summary>
    /// Sends what code that spawns nothing itself names (a helper choosing a kind, a frame table) to every sheet its
    /// callers' spawns name too: the helper's value is what they spawn, as a laser's kind chosen by its definition's
    /// helper is spawned by the robot's spawn call and drawn under the graphics that call and its reassignments give.
    /// </summary>
    private void RouteHelperEvidence()
    {
        var sinks = new Dictionary<Unit, HashSet<Collected>>();
        foreach ((Unit helper, Collected evidence) in _helperEvidence)
        {
            foreach (Collected collected in SpawningCallersOf(helper, sinks, []))
                Merge(collected, evidence);
        }
    }

    /// <summary>
    /// The collections the nearest spawning callers of <paramref name="unit"/> spawn into, climbing through callers that
    /// spawn nothing themselves; memoized per unit.
    /// </summary>
    private HashSet<Collected> SpawningCallersOf(Unit unit, Dictionary<Unit, HashSet<Collected>> sinks, HashSet<Unit> visiting)
    {
        if (sinks.TryGetValue(unit, out HashSet<Collected>? known))
            return known;
        var collections = new HashSet<Collected>(ReferenceEqualityComparer.Instance);
        if (!visiting.Add(unit))
            return collections;
        foreach (Unit caller in _callers.GetValueOrDefault(unit) ?? [])
        {
            UnitScan scan = ScanOf(caller);
            if (scan.SpawnCalls.Count == 0 && scan.Reassignments.Count == 0)
            {
                collections.UnionWith(SpawningCallersOf(caller, sinks, visiting));
                continue;
            }
            foreach (SpawnTarget target in scan.UnitTargets)
            {
                if (Collect(caller, target) is { } collected)
                    collections.Add(collected);
            }
        }
        visiting.Remove(unit);
        // Every unit's answer is kept, so no caller chain is climbed twice; inside a cycle a unit misses only what its
        // still-open ancestors' own callers spawn into, which those ancestors receive themselves.
        sinks.TryAdd(unit, collections);
        return collections;
    }

    /// <summary>Adds everything <paramref name="source"/> names to <paramref name="target"/>.</summary>
    private static void Merge(Collected target, Collected source)
    {
        target.ProjectileFrames.UnionWith(source.ProjectileFrames);
        target.SpriteObjects.UnionWith(source.SpriteObjects);
        target.AddFamiliesOf(source);
    }

    /// <summary>Reaches <paramref name="unit"/> from code running in <paramref name="context"/>, and every implementation it dispatches to.</summary>
    private void Reach(Unit unit, WalkContext context, bool viaDispatch, Unit? origin)
    {
        INamedTypeSymbol type = unit.Type;
        if (_model.IsTable(type))
            return;
        if (origin is { } calling)
        {
            if (!_callers.TryGetValue(unit, out HashSet<Unit>? callers))
                _callers.Add(unit, callers = []);
            callers.Add(calling);
        }
        WalkContext entered = context;
        if (_model.FamilyOf(type) is { } family)
        {
            if (!SymbolEqualityComparer.Default.Equals(family, _family))
            {
                // Code outside a family that refers to it directly (constructs it, or calls a factory or member of it, as
                // Botwoon's ForSegment and Spit build his segments' and spit's animations) has its objects in play: their
                // frames are what that code spawns. Dispatch into a family's overrides is the engine running whichever
                // family it holds, which names none.
                if (!viaDispatch && origin is { } spawner)
                {
                    UnitScan scan = ScanOf(spawner);
                    // The programs the code starts the family in (#1513): those its constructions name, or any when it
                    // reaches the family otherwise or names a program no constant decides.
                    IReadOnlySet<string>? programs = scan.FamilyPrograms.TryGetValue(family, out HashSet<string>? named) ? named : null;
                    Evidence(spawner).AddFamily(family, programs);
                    if (scan.SpawnCalls.Count == 0 && scan.Reassignments.Count == 0)
                    {
                        Of(_helperEvidence, spawner).AddFamily(family, programs);
                        _unspawnedNames.Add(new UnspawnedName(spawner, collected => collected.AddFamily(family, programs), family, Member: null));
                    }
                    else
                    {
                        foreach (SpawnTarget target in scan.UnitTargets)
                            Collect(spawner, target)?.AddFamily(family, programs);
                    }
                    if (context == WalkContext.Own)
                        Own.AddFamily(family, programs);
                    Spawners.Add(spawner);
                }
                return;
            }
        }
        else if (_model.IsForeign(type, _behaviour))
        {
            if (viaDispatch || _behaviour is null)
                return;
            entered = WalkContext.Foreign;
        }

        if (entered == WalkContext.Own)
        {
            if (!_own.Add(unit))
                return;
        }
        else
        {
            if (_own.Contains(unit) || !_foreign.Add(unit))
                return;
            if (context == WalkContext.Own && origin is { } source)
                ForeignEntries.Add((source, unit));
        }
        _pending.Enqueue((unit, entered));
        if (unit.Kind == UnitKind.Member)
        {
            foreach (ISymbol implementation in _model.Implementations(unit.Symbol))
                Reach(Unit.Member(implementation), entered, viaDispatch: true, origin: unit);
        }
    }

    /// <summary>Records <paramref name="unit"/>'s spawn calls, and the graphics it reassigns when it spawns nothing itself.</summary>
    private void RecordCalls(Unit unit, UnitScan scan)
    {
        foreach (SpawnCall call in scan.SpawnCalls)
        {
            if (call.Targets.Count == 0)
                Unreadable.Add($"{_model.Describe(unit)}: graphics {call.Graphics} name no sheet");
            // Graphics copied from a spawned object draw where that object does, never where its free slot's initializer says.
            if (call.Copied && call.Targets.Any(t => t.Source != GraphicsSource.Inherited))
                Unreadable.Add($"{_model.Describe(unit)}: graphics {call.Graphics} are copied from a spawned object but were read as its initializer");
            foreach (SpawnTarget target in call.Targets)
            {
                (Destination? destination, PendingKey? pending) = Resolve(unit, target, call.Graphics);
                Calls.Add(new CallRecord(call.Callee, unit.Type, destination, pending));
            }
        }
        if (scan.SpawnCalls.Count > 0)
            return;
        foreach ((IReadOnlyList<SpawnTarget> targets, string graphics) in scan.Reassignments)
        {
            foreach (SpawnTarget target in targets)
            {
                // Graphics copied from another of the type's objects name no sheet the type's spawns do not.
                if (target.Source == GraphicsSource.Inherited)
                    continue;
                (Destination? destination, PendingKey? pending) = Resolve(unit, target, graphics);
                INamedTypeSymbol switching = _behaviour ?? unit.Type;
                if (destination is { } sheet)
                    Of(Reassigned, switching).Add(sheet);
                if (pending is { } decided)
                    Of(ReassignedPending, switching).Add(decided);
            }
        }
    }

    /// <summary>Records an enum member <paramref name="unit"/>'s code names.</summary>
    private void Record(Unit unit, UnitScan scan, IFieldSymbol member, WalkContext context)
    {
        INamedTypeSymbol type = member.ContainingType;
        if (SymbolEqualityComparer.Default.Equals(type, _model.EnemyFrameId))
        {
            // The frames a definition shows are the ones its own code sets; another behaviour's code shows them on its own enemies.
            if (context == WalkContext.Own && !ExcludedEnemyFrames.Contains(member.Name))
                Own.EnemyFrames.Add(member.Name);
            return;
        }
        Action<Collected>? add = null;
        INamedTypeSymbol? startedFamily = null;
        if (SymbolEqualityComparer.Default.Equals(type, _model.EnemyProjectileFrameId))
        {
            if (!ExcludedProjectileFrames.Contains(member.Name))
                add = collected => collected.ProjectileFrames.Add(member.Name);
        }
        else if (SymbolEqualityComparer.Default.Equals(type, _model.RoomSpriteObjectKind))
        {
            if (scan.SpawnCalls.Count == 0 && scan.Reassignments.Count == 0)
            {
                Spawners.Add(unit);
                Evidence(unit).SpriteObjects.Add(member.Name);
                _dataSpriteObjects.Add((unit, member.Name));
                return;
            }
            add = collected => collected.SpriteObjects.Add(member.Name);
        }
        else if (SymbolEqualityComparer.Default.Equals(type, _model.RoomEnemyProjectileKind) && _model.StartedFamily(member) is { } started)
        {
            // A typed start names the program its kind starts the family in, when its construction names one (#1513).
            IReadOnlySet<string>? programs = _model.StartedPrograms(member);
            add = collected => collected.AddFamily(started, programs);
            startedFamily = started;
            if (context == WalkContext.Own)
                Own.AddFamily(started, programs);
        }
        if (add is null)
            return;
        Spawners.Add(unit);
        add(Evidence(unit));
        if (scan.SpawnCalls.Count == 0 && scan.Reassignments.Count == 0)
        {
            add(Of(_helperEvidence, unit));
            _unspawnedNames.Add(new UnspawnedName(unit, add, startedFamily, member.OriginalDefinition));
            return;
        }
        foreach (SpawnTarget target in scan.TargetsOf(member))
        {
            if (Collect(unit, target) is { } collected)
                add(collected);
        }
    }

    /// <summary>The collection what <paramref name="unit"/> spawns toward <paramref name="target"/> goes to.</summary>
    private Collected? Collect(Unit unit, SpawnTarget target)
    {
        (Destination? destination, PendingKey? pending) = Resolve(unit, target, graphics: "(its spawns)");
        if (destination is { } sheet)
            return Of(Spawns, sheet);
        if (pending is { } decided)
        {
            PendingSites.TryAdd(decided, _model.Describe(unit));
            return Of(Pending, decided);
        }
        return null;
    }

    /// <summary>The sheet <paramref name="target"/> names in this walk's scope, or what decides it; neither for unreadable graphics.</summary>
    private (Destination?, PendingKey?) Resolve(Unit unit, SpawnTarget target, string graphics)
    {
        INamedTypeSymbol? scope = target.Scope ?? _behaviour;
        switch (target.Source)
        {
            case GraphicsSource.Spawner:
                return (scope is null ? CommonOf(unit) : new Destination(DestinationKind.Own, scope), null);
            case GraphicsSource.Shared:
                return (scope is null ? CommonOf(unit) : new Destination(DestinationKind.Shared, scope), null);
            case GraphicsSource.Sheet:
                return (new Destination(DestinationKind.Sheet, Sheet: target.Sheet), null);
            case GraphicsSource.Inherited:
                return (null, new PendingKey(GraphicsSource.Inherited, unit.Type));
            case GraphicsSource.Parameter when target.Origin is { } method:
                return (null, new PendingKey(GraphicsSource.Parameter, method));
            default:
                Unreadable.Add($"{_model.Describe(unit)}: graphics {graphics} name no sheet this generator can read");
                return (null, null);
        }
    }

    /// <summary>
    /// The common sheet of the spawner whose code <paramref name="unit"/> is, recording the unit under that spawner for the
    /// report of where each spawner's code is reached from.
    /// </summary>
    private Destination CommonOf(Unit unit)
    {
        // A family walk's spawns are all its family's, whatever their graphics: they join the family's closure.
        if (_family is not null)
            return new Destination(DestinationKind.Common, Spawner: _family.ToDisplayString());
        string spawner = _model.SpawnerKey(unit);
        if (!SpawnerUnits.TryGetValue(spawner, out HashSet<Unit>? units))
            SpawnerUnits.Add(spawner, units = []);
        units.Add(unit);
        return new Destination(DestinationKind.Common, Spawner: spawner);
    }

    /// <summary>The units each common spawner's spawns were met in.</summary>
    public Dictionary<string, HashSet<Unit>> SpawnerUnits { get; } = new(StringComparer.Ordinal);

    /// <summary>The units reaching <paramref name="unit"/> directly in this walk.</summary>
    public IReadOnlySet<Unit> CallersOf(Unit unit) => _callers.GetValueOrDefault(unit) ?? [];

    /// <summary>Everything <paramref name="unit"/>'s type's code spawns.</summary>
    private Collected Evidence(Unit unit) => Of(EvidenceByType, unit.Type);

    /// <summary>The value for <paramref name="key"/>, created when absent.</summary>
    private static TValue Of<TKey, TValue>(Dictionary<TKey, TValue> values, TKey key) where TKey : notnull where TValue : new()
    {
        if (!values.TryGetValue(key, out TValue? value))
            values.Add(key, value = new TValue());
        return value;
    }
}
