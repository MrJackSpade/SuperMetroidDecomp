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
/// operators and conversions, static data reads (which run the type's static initialization), and interface and
/// virtual dispatch to every Core implementation. Tables naming every kind or frame are never entered. A projectile
/// program family is a leaf: constructing it records the family, whose own walk supplies its frames. Another
/// behaviour's code (the behaviour type, and the types only behaviour code constructs) is never reached by dispatch;
/// a behaviour walk enters it only by a direct reference, and only to collect what it spawns.
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
    private readonly HashSet<Unit> _own = [];
    private readonly HashSet<Unit> _foreign = [];
    private readonly Queue<(Unit Unit, WalkContext Context)> _pending = new();

    /// <summary>Creates a walk for <paramref name="behaviour"/>, for <paramref name="family"/>, or (both null) outside every behaviour.</summary>
    public Walk(CoreModel model, UnitScanner scanner, INamedTypeSymbol? behaviour, INamedTypeSymbol? family)
    {
        _model = model;
        _scanner = scanner;
        _behaviour = behaviour;
        _family = family;
    }

    /// <summary>What the walk's own code names.</summary>
    public Collected Own { get; } = new();

    /// <summary>What other behaviours' code the walk's own code calls directly names.</summary>
    public Collected Foreign { get; } = new();

    /// <summary>Every unit reached, in either context.</summary>
    public IEnumerable<Unit> Reached => _own.Concat(_foreign);

    /// <summary>The reached units whose own code names a projectile frame, sprite object, started kind or family.</summary>
    public HashSet<Unit> Spawners { get; } = [];

    /// <summary>Each other behaviour's unit the walk's own code references directly, with the unit referencing it.</summary>
    public List<(Unit From, Unit To)> ForeignEntries { get; } = [];

    /// <summary>Starts the walk at <paramref name="unit"/>, as the walk's own code.</summary>
    public void Root(Unit unit) => Reach(unit, WalkContext.Own, viaDispatch: false, origin: null);

    /// <summary>Follows every reference from the roots until nothing new is reached.</summary>
    public void Run()
    {
        while (_pending.Count > 0)
        {
            (Unit unit, WalkContext context) = _pending.Dequeue();
            UnitScan scan = _scanner.Scan(unit);
            foreach (IFieldSymbol member in scan.EnumMembers)
                Record(unit, member, context);
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
                    Collect(context).Families.Add(family);
                    if (origin is { } constructor)
                        Spawners.Add(constructor);
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
    private void Record(Unit unit, IFieldSymbol member, WalkContext context)
    {
        Collected into = Collect(context);
        INamedTypeSymbol type = member.ContainingType;
        if (SymbolEqualityComparer.Default.Equals(type, _model.EnemyFrameId))
        {
            // The frames a definition shows are the ones its own code sets; another behaviour's code shows them on its own enemies.
            if (context == WalkContext.Own && !ExcludedEnemyFrames.Contains(member.Name))
                into.EnemyFrames.Add(member.Name);
        }
        else if (SymbolEqualityComparer.Default.Equals(type, _model.EnemyProjectileFrameId))
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

    /// <summary>The collection for <paramref name="context"/>.</summary>
    private Collected Collect(WalkContext context) => context == WalkContext.Own ? Own : Foreign;
}
