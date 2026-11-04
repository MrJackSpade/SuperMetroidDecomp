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

    /// <summary>The projectile program families constructed, or started through a typed-start kind.</summary>
    public HashSet<INamedTypeSymbol> Families { get; } = new(SymbolEqualityComparer.Default);
}

/// <summary>Which kind of sheet a destination is.</summary>
internal enum DestinationKind
{
    /// <summary>A behaviour's own spawned-object sheet.</summary>
    Own,
    /// <summary>The shared sheet of the graphics sets holding a behaviour's definitions.</summary>
    Shared,
    /// <summary>Every set's shared sheet.</summary>
    Anywhere,
    /// <summary>A sheet-specific owner's sheet.</summary>
    Sheet,
}

/// <summary>One sheet spawns are drawn from.</summary>
/// <param name="Kind">Which kind of sheet.</param>
/// <param name="Behaviour">For <see cref="DestinationKind.Own"/> and <see cref="DestinationKind.Shared"/>, whose.</param>
/// <param name="Sheet">For <see cref="DestinationKind.Sheet"/>, the owner property naming it.</param>
internal readonly record struct Destination(DestinationKind Kind, INamedTypeSymbol? Behaviour = null, string? Sheet = null)
{
    /// <inheritdoc/>
    public bool Equals(Destination other) =>
        Kind == other.Kind && SymbolEqualityComparer.Default.Equals(Behaviour, other.Behaviour) && Sheet == other.Sheet;

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(Kind, Behaviour is null ? 0 : SymbolEqualityComparer.Default.GetHashCode(Behaviour), Sheet);
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
    /// <summary>EnemyFrameId members that draw nothing and are compiled, not installed.</summary>
    public static readonly string[] ExcludedEnemyFrames = ["Empty", "EmptyExtended"];

    /// <summary>EnemyProjectileFrameId members that draw nothing.</summary>
    public static readonly string[] ExcludedProjectileFrames = ["Blank"];

    /// <summary>RoomSpriteObjectKind members that are no object.</summary>
    public static readonly string[] ExcludedSpriteObjects = ["None"];

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
                RecordCalls(unit, scan);
            foreach (IFieldSymbol member in scan.EnumMembers)
                Record(unit, scan, member, context);
            foreach (Unit target in scan.Targets)
                Reach(target, context, viaDispatch: false, origin: unit);
        }
        RouteHelperEvidence();
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
        target.Families.UnionWith(source.Families);
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
                if (unit.IsConstructor && !viaDispatch && origin is { } spawner)
                {
                    UnitScan scan = ScanOf(spawner);
                    foreach (SpawnTarget target in scan.UnitTargets)
                        Collect(spawner, target)?.Families.Add(family);
                    Evidence(spawner).Families.Add(family);
                    if (scan.SpawnCalls.Count == 0 && scan.Reassignments.Count == 0)
                        Of(_helperEvidence, spawner).Families.Add(family);
                    if (context == WalkContext.Own)
                        Own.Families.Add(family);
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
        if (SymbolEqualityComparer.Default.Equals(type, _model.EnemyProjectileFrameId))
        {
            if (!ExcludedProjectileFrames.Contains(member.Name))
                add = collected => collected.ProjectileFrames.Add(member.Name);
        }
        else if (SymbolEqualityComparer.Default.Equals(type, _model.RoomSpriteObjectKind))
        {
            if (!ExcludedSpriteObjects.Contains(member.Name))
                add = collected => collected.SpriteObjects.Add(member.Name);
        }
        else if (SymbolEqualityComparer.Default.Equals(type, _model.RoomEnemyProjectileKind) && _model.StartedFamily(member) is { } started)
        {
            add = collected => collected.Families.Add(started);
            if (context == WalkContext.Own)
                Own.Families.Add(started);
        }
        if (add is null)
            return;
        Spawners.Add(unit);
        add(Evidence(unit));
        if (scan.SpawnCalls.Count == 0 && scan.Reassignments.Count == 0)
            add(Of(_helperEvidence, unit));
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
                return (scope is null ? new Destination(DestinationKind.Anywhere) : new Destination(DestinationKind.Own, scope), null);
            case GraphicsSource.Shared:
                return (scope is null ? new Destination(DestinationKind.Anywhere) : new Destination(DestinationKind.Shared, scope), null);
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
