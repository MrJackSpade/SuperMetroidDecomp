using Microsoft.CodeAnalysis;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>What one behaviour shows and spawns.</summary>
/// <param name="Behaviour">The behaviour.</param>
/// <param name="EnemyFrames">The enemy frames its own code, and the families it spawns, show.</param>
/// <param name="ProjectileFrames">The projectile frames it spawns.</param>
/// <param name="SpriteObjects">The room sprite-object kinds it spawns.</param>
/// <param name="ForeignEntries">The other behaviours' members its code calls directly.</param>
internal sealed record BehaviourSpawns(
    Behaviour Behaviour,
    IReadOnlySet<string> EnemyFrames,
    IReadOnlySet<string> ProjectileFrames,
    IReadOnlySet<string> SpriteObjects,
    IReadOnlyList<(Unit From, Unit To)> ForeignEntries);

/// <summary>
/// Walks every projectile program family, every enemy behaviour and the code outside every behaviour, and resolves
/// what each shows and spawns, a spawned family contributing everything its own walk (and the families it spawns) name.
/// </summary>
internal sealed class SpawnAnalysis
{
    private readonly CoreModel _model;
    private readonly Dictionary<INamedTypeSymbol, Walk> _familyWalks = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, Collected> _familyClosures = new(SymbolEqualityComparer.Default);

    /// <summary>Creates the analysis over <paramref name="model"/>.</summary>
    private SpawnAnalysis(CoreModel model) => _model = model;

    /// <summary>What each behaviour shows and spawns, in behaviour order.</summary>
    public IReadOnlyList<BehaviourSpawns> Behaviours { get; private set; } = [];

    /// <summary>The projectile frames code outside every behaviour spawns.</summary>
    public IReadOnlySet<string> AnywhereProjectileFrames { get; private set; } = new HashSet<string>();

    /// <summary>The room sprite-object kinds code outside every behaviour spawns.</summary>
    public IReadOnlySet<string> AnywhereSpriteObjects { get; private set; } = new HashSet<string>();

    /// <summary>The units outside every behaviour whose own code names something they spawn.</summary>
    public IReadOnlyList<Unit> AnywhereSpawners { get; private set; } = [];

    /// <summary>Runs every walk.</summary>
    public static SpawnAnalysis Run(CoreModel model)
    {
        RequireMembers(model.EnemyFrameId, Walk.ExcludedEnemyFrames);
        RequireMembers(model.EnemyProjectileFrameId, Walk.ExcludedProjectileFrames);
        RequireMembers(model.RoomSpriteObjectKind, Walk.ExcludedSpriteObjects);
        var analysis = new SpawnAnalysis(model);
        var scanner = new UnitScanner(model);
        var reached = new HashSet<Unit>();

        foreach (INamedTypeSymbol family in model.Families)
        {
            var walk = new Walk(model, scanner, behaviour: null, family);
            foreach (INamedTypeSymbol type in model.Types)
            {
                if (SymbolEqualityComparer.Default.Equals(model.FamilyOf(type), family))
                    RootAll(walk, model, type);
            }
            walk.Run();
            reached.UnionWith(walk.Reached);
            analysis._familyWalks.Add(family, walk);
        }

        var behaviours = new List<BehaviourSpawns>();
        foreach (Behaviour behaviour in model.Behaviours)
        {
            var walk = new Walk(model, scanner, behaviour.Type, family: null);
            foreach (INamedTypeSymbol type in model.Types)
            {
                if (!model.IsTable(type) && model.FamilyOf(type) is null && model.OwnersOf(type) is { } owners && owners.Contains(behaviour.Type))
                    RootAll(walk, model, type);
            }
            walk.Run();
            reached.UnionWith(walk.Reached);
            Collected own = analysis.WithFamilies(walk.Own);
            Collected foreign = analysis.WithFamilies(walk.Foreign);
            behaviours.Add(new BehaviourSpawns(
                behaviour,
                own.EnemyFrames,
                Union(own.ProjectileFrames, foreign.ProjectileFrames),
                Union(own.SpriteObjects, foreign.SpriteObjects),
                walk.ForeignEntries));
        }
        analysis.Behaviours = behaviours;

        // The code outside every behaviour is every shared unit no behaviour or family reaches; it reaches shared code
        // the behaviours reach too, as the enemy deaths it runs for every enemy.
        var anywhere = new Walk(model, scanner, behaviour: null, family: null);
        foreach (INamedTypeSymbol type in model.Types)
        {
            if (model.IsTable(type) || model.FamilyOf(type) is not null || model.OwnersOf(type) is not null)
                continue;
            foreach (Unit unit in model.UnitsOf(type))
            {
                if (!reached.Contains(unit))
                    anywhere.Root(unit);
            }
        }
        anywhere.Run();
        Collected collected = analysis.WithFamilies(anywhere.Own);
        analysis.AnywhereProjectileFrames = collected.ProjectileFrames;
        analysis.AnywhereSpriteObjects = collected.SpriteObjects;
        analysis.AnywhereSpawners = [.. anywhere.Spawners.OrderBy(model.Describe, StringComparer.Ordinal)];
        return analysis;
    }

    /// <summary>Prints what the walks found that a reviewer should see: cross-behaviour calls and the code spawning anywhere.</summary>
    public void Report(TextWriter output)
    {
        foreach (BehaviourSpawns spawns in Behaviours)
        {
            foreach ((Unit origin, Unit target) in spawns.ForeignEntries)
                output.WriteLine($"{spawns.Behaviour.Family} spawns through {target} (from {origin})");
        }
        output.WriteLine($"anywhere code: {string.Join(", ", AnywhereSpawners.Select(_model.Describe))}");
    }

    /// <summary>Requires <paramref name="type"/> to declare every excluded member, so a rename cannot silently stop the exclusion.</summary>
    private static void RequireMembers(INamedTypeSymbol type, IEnumerable<string> names)
    {
        foreach (string name in names)
        {
            if (type.GetMembers(name) is not [IFieldSymbol])
                throw new InvalidDataException($"{type.Name} no longer declares {name}, which the generator excludes.");
        }
    }

    /// <summary>Roots every unit of <paramref name="type"/>'s code.</summary>
    private static void RootAll(Walk walk, CoreModel model, INamedTypeSymbol type)
    {
        foreach (Unit unit in model.UnitsOf(type))
            walk.Root(unit);
    }

    /// <summary><paramref name="collected"/> with everything the families it constructs or starts name added.</summary>
    private Collected WithFamilies(Collected collected)
    {
        var result = new Collected();
        Merge(result, collected);
        foreach (INamedTypeSymbol family in collected.Families)
            Merge(result, Closure(family, new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default)));
        return result;
    }

    /// <summary>Everything <paramref name="family"/>'s walk names, with the families it spawns in turn.</summary>
    private Collected Closure(INamedTypeSymbol family, HashSet<INamedTypeSymbol> visiting)
    {
        if (_familyClosures.TryGetValue(family, out Collected? cached))
            return cached;
        var result = new Collected();
        if (!visiting.Add(family))
            return result;
        Walk walk = _familyWalks[family];
        // A family walk enters no behaviour code, so everything it names is its own.
        Merge(result, walk.Own);
        foreach (INamedTypeSymbol spawned in walk.Own.Families)
            Merge(result, Closure(spawned, visiting));
        visiting.Remove(family);
        if (visiting.Count == 0)
            _familyClosures.Add(family, result);
        return result;
    }

    /// <summary>Adds everything <paramref name="source"/> names to <paramref name="target"/>.</summary>
    private static void Merge(Collected target, Collected source)
    {
        target.EnemyFrames.UnionWith(source.EnemyFrames);
        target.ProjectileFrames.UnionWith(source.ProjectileFrames);
        target.SpriteObjects.UnionWith(source.SpriteObjects);
        target.Families.UnionWith(source.Families);
    }

    /// <summary>The union of two name sets.</summary>
    private static HashSet<string> Union(IEnumerable<string> first, IEnumerable<string> second)
    {
        var union = new HashSet<string>(first, StringComparer.Ordinal);
        union.UnionWith(second);
        return union;
    }
}
