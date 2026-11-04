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

/// <summary>
/// One reachability walk over Core's call graph, from a behaviour, a projectile program family, or the code outside
/// every behaviour. It follows calls, constructions, property and indexer accesses, method groups, user-defined
/// operators and conversions, static data reads (each member its own initializer, with the type's static constructor),
/// and interface and virtual dispatch to every Core implementation. Tables naming every kind or frame are never
/// entered. A projectile program family is a leaf: constructing it records the family, whose own walk supplies its
/// frames. Another behaviour's code (the behaviour type, and the types only behaviour code constructs) is never reached
/// by dispatch; a behaviour walk enters it only by a direct reference, and only to collect what it spawns. What code
/// spawns with emptied, shared or another behaviour's graphics is routed where those graphics draw from. With a
/// definition context, the behaviour's entity dispatch is evaluated for that one definition.
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

    /// <summary>What the walk's own code names, spawning with its spawner's graphics.</summary>
    public Collected Own { get; } = new();

    /// <summary>What other behaviours' code the walk's own code calls directly names, spawning with its spawner's graphics.</summary>
    public Collected Foreign { get; } = new();

    /// <summary>What the walk's code spawns with graphics that draw elsewhere than its spawner's sheet, by where they draw.</summary>
    public Dictionary<SpawnTarget, Collected> Routed { get; } = [];

    /// <summary>Every unit reached, in either context.</summary>
    public IEnumerable<Unit> Reached => _own.Concat(_foreign);

    /// <summary>The units reached as the walk's own code.</summary>
    public IReadOnlySet<Unit> OwnUnits => _own;

    /// <summary>The reached units whose own code names a projectile frame, sprite object, started kind or family.</summary>
    public HashSet<Unit> Spawners { get; } = [];

    /// <summary>Each other behaviour's unit the walk's own code references directly, with the unit referencing it.</summary>
    public List<(Unit From, Unit To)> ForeignEntries { get; } = [];

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
            foreach (IFieldSymbol member in scan.EnumMembers)
                Record(unit, scan, member, context);
            foreach (Unit target in scan.Targets)
                Reach(target, context, viaDispatch: false, origin: unit);
        }
    }

    /// <summary>Reaches <paramref name="unit"/> from code running in <paramref name="context"/>, and every implementation it dispatches to.</summary>
    private void Reach(Unit unit, WalkContext context, bool viaDispatch, Unit? origin)
    {
        INamedTypeSymbol type = unit.Type;
        if (_model.IsTable(type))
            return;
        WalkContext entered = context;
        if (_model.FamilyOf(type) is { } family)
        {
            if (!SymbolEqualityComparer.Default.Equals(family, _family))
            {
                if (unit.IsConstructor && !viaDispatch)
                {
                    SpawnTarget target = origin is { } constructor ? ScanOf(constructor).UnitTarget : SpawnTarget.Default;
                    Collect(context, target).Families.Add(family);
                    if (origin is { } spawner)
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
        Collected into = Collect(context, scan.TargetOf(member));
        if (SymbolEqualityComparer.Default.Equals(type, _model.EnemyProjectileFrameId))
        {
            if (!ExcludedProjectileFrames.Contains(member.Name))
            {
                into.ProjectileFrames.Add(member.Name);
                Spawners.Add(unit);
            }
        }
        else if (SymbolEqualityComparer.Default.Equals(type, _model.RoomSpriteObjectKind))
        {
            if (!ExcludedSpriteObjects.Contains(member.Name))
            {
                into.SpriteObjects.Add(member.Name);
                Spawners.Add(unit);
            }
        }
        else if (SymbolEqualityComparer.Default.Equals(type, _model.RoomEnemyProjectileKind)
                 && _model.StartedFamily(member) is { } started)
        {
            into.Families.Add(started);
            Spawners.Add(unit);
        }
    }

    /// <summary>The collection what code in <paramref name="context"/> spawns toward <paramref name="target"/> goes to.</summary>
    private Collected Collect(WalkContext context, SpawnTarget target)
    {
        // Graphics naming this walk's own behaviour are its spawner's or shared sheet as they say, like its own code's.
        SpawnTarget normalized = target.Scope is { } scope && SymbolEqualityComparer.Default.Equals(scope, _behaviour)
            ? target with { Scope = null }
            : target;
        // A sheet-specific owner draws from its own sheet whoever spawns it.
        if (normalized.Sheet is not null)
            normalized = normalized with { Scope = null, Shared = false };
        if (normalized.Equals(SpawnTarget.Default))
            return context == WalkContext.Own ? Own : Foreign;
        if (!Routed.TryGetValue(normalized, out Collected? routed))
            Routed.Add(normalized, routed = new Collected());
        return routed;
    }
}
