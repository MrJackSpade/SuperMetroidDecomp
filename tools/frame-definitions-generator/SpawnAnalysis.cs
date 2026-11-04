using Microsoft.CodeAnalysis;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>What one behaviour shows and spawns.</summary>
/// <param name="Behaviour">The behaviour.</param>
/// <param name="EnemyFrames">The enemy frames each of its definitions shows, by definition name.</param>
/// <param name="ProjectileFrames">The projectile frames it spawns that draw from its own spawned-object sheet.</param>
/// <param name="SpriteObjects">The room sprite-object kinds it spawns that draw from its own spawned-object sheet.</param>
/// <param name="SharedProjectileFrames">The projectile frames it spawns that draw from the shared sheet of the sets it is in.</param>
/// <param name="SharedSpriteObjects">The room sprite-object kinds it spawns that draw from the shared sheet of the sets it is in.</param>
/// <param name="ForeignEntries">The other behaviours' members its code calls directly.</param>
internal sealed record BehaviourSpawns(
    Behaviour Behaviour,
    IReadOnlyDictionary<string, IReadOnlySet<string>> EnemyFrames,
    IReadOnlySet<string> ProjectileFrames,
    IReadOnlySet<string> SpriteObjects,
    IReadOnlySet<string> SharedProjectileFrames,
    IReadOnlySet<string> SharedSpriteObjects,
    IReadOnlyList<(Unit From, Unit To)> ForeignEntries);

/// <summary>
/// Walks every projectile program family, every enemy behaviour (and each definition of a behaviour serving several)
/// and the code outside every behaviour, and resolves what each shows and spawns, and from which sheet: a spawned family
/// contributes everything its own walk (and the families it spawns) name.
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

    /// <summary>The projectile frames code outside every behaviour's scope spawns, which every set's shared sheet holds.</summary>
    public IReadOnlySet<string> AnywhereProjectileFrames { get; private set; } = new HashSet<string>();

    /// <summary>The room sprite-object kinds code outside every behaviour's scope spawns, which every set's shared sheet holds.</summary>
    public IReadOnlySet<string> AnywhereSpriteObjects { get; private set; } = new HashSet<string>();

    /// <summary>
    /// The projectile frames and sprite-object kinds each sheet-specific owner's sheet holds (as Mother Brain's tank glass),
    /// by the SpawnedObjectOwner property naming it: what is spawned with graphics naming that owner.
    /// </summary>
    public IReadOnlyDictionary<string, Collected> BySheet { get; private set; } = new Dictionary<string, Collected>();

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

        var walks = new List<(Behaviour Behaviour, Walk Walk, IReadOnlyDictionary<string, IReadOnlySet<string>> Frames)>();
        foreach (Behaviour behaviour in model.Behaviours)
        {
            var walk = new Walk(model, scanner, behaviour.Type, family: null);
            RootBehaviour(walk, model, behaviour);
            walk.Run();
            reached.UnionWith(walk.Reached);
            IReadOnlySet<string> familyFrames = analysis.WithFamilies(walk.Own).EnemyFrames;
            IReadOnlyDictionary<string, IReadOnlySet<string>> frames = behaviour.Definitions.Count == 1
                ? new Dictionary<string, IReadOnlySet<string>> { [behaviour.Definitions[0]] = familyFrames }
                : new DefinitionFrames(model, scanner, behaviour, walk).Resolve(familyFrames);
            walks.Add((behaviour, walk, frames));
        }

        // The code outside every behaviour is every shared unit no behaviour or family reaches; it reaches shared code
        // the behaviours reach too, as the enemy deaths it runs for every enemy. A static data member of a type the walks
        // read belongs to its readers: one no walk reads is no root of its own.
        var touched = new HashSet<INamedTypeSymbol>(reached.Select(unit => unit.Type), SymbolEqualityComparer.Default);
        var anywhere = new Walk(model, scanner, behaviour: null, family: null);
        foreach (INamedTypeSymbol type in model.Types)
        {
            if (model.IsTable(type) || model.FamilyOf(type) is not null || model.OwnersOf(type) is not null)
                continue;
            foreach (Unit unit in model.UnitsOf(type))
            {
                if (!reached.Contains(unit) && !(touched.Contains(type) && IsStaticData(unit)))
                    anywhere.Root(unit);
            }
        }
        anywhere.Run();

        // Each walk's spawns go where their graphics draw from: its spawner's sheet, the shared sheet of the sets a
        // behaviour whose graphics it uses is in, or (outside every behaviour, emptied or shared) every set's shared sheet.
        var owned = new Dictionary<INamedTypeSymbol, Collected>(SymbolEqualityComparer.Default);
        var shared = new Dictionary<INamedTypeSymbol, Collected>(SymbolEqualityComparer.Default);
        var everywhere = new Collected();
        var sheets = new Dictionary<string, Collected>(StringComparer.Ordinal);
        foreach ((Behaviour behaviour, Walk walk, _) in walks)
        {
            Merge(Of(owned, behaviour.Type), walk.Own);
            Merge(Of(owned, behaviour.Type), walk.Foreign);
            Route(walk, behaviour.Type, owned, shared, everywhere, sheets);
        }
        Merge(everywhere, anywhere.Own);
        Route(anywhere, scope: null, owned, shared, everywhere, sheets);
        analysis.BySheet = sheets.ToDictionary(pair => pair.Key, pair => analysis.WithFamilies(pair.Value), StringComparer.Ordinal);

        var behaviours = new List<BehaviourSpawns>();
        foreach ((Behaviour behaviour, Walk walk, IReadOnlyDictionary<string, IReadOnlySet<string>> frames) in walks)
        {
            Collected own = analysis.WithFamilies(Of(owned, behaviour.Type));
            Collected sharedSpawns = analysis.WithFamilies(Of(shared, behaviour.Type));
            behaviours.Add(new BehaviourSpawns(behaviour, frames, own.ProjectileFrames, own.SpriteObjects,
                sharedSpawns.ProjectileFrames, sharedSpawns.SpriteObjects, walk.ForeignEntries));
        }
        analysis.Behaviours = behaviours;
        Collected anywhereSpawns = analysis.WithFamilies(everywhere);
        analysis.AnywhereProjectileFrames = anywhereSpawns.ProjectileFrames;
        analysis.AnywhereSpriteObjects = anywhereSpawns.SpriteObjects;
        analysis.AnywhereSpawners = [.. anywhere.Spawners.OrderBy(model.Describe, StringComparer.Ordinal)];
        return analysis;
    }

    /// <summary>Prints what the walks found that a reviewer should see: cross-behaviour calls and the code spawning anywhere.</summary>
    public void Report(TextWriter output)
    {
        foreach (BehaviourSpawns spawns in Behaviours)
        {
            foreach ((Unit origin, Unit target) in spawns.ForeignEntries.Where(entry => entry.To.Symbol is IMethodSymbol))
                output.WriteLine($"{spawns.Behaviour.Family} spawns through {target} (from {origin})");
            IReadOnlySet<string> first = spawns.EnemyFrames.Values.First();
            if (spawns.EnemyFrames.Values.Any(frames => !frames.SetEquals(first)))
            {
                foreach ((string definition, IReadOnlySet<string> frames) in spawns.EnemyFrames)
                    output.WriteLine($"{spawns.Behaviour.Family}: {definition} shows {frames.Count} frames");
            }
        }
        output.WriteLine($"anywhere code: {string.Join(", ", AnywhereSpawners.Select(_model.Describe))}");
    }

    /// <summary>Roots a behaviour walk at the behaviour and every type only its code constructs.</summary>
    internal static void RootBehaviour(Walk walk, CoreModel model, Behaviour behaviour)
    {
        foreach (INamedTypeSymbol type in model.Types)
        {
            if (!model.IsTable(type) && model.FamilyOf(type) is null && model.OwnersOf(type) is { } owners && owners.Contains(behaviour.Type))
                RootAll(walk, model, type);
        }
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

    /// <summary>Whether <paramref name="unit"/> is a static field or data property: static data its readers own.</summary>
    private static bool IsStaticData(Unit unit) => unit is { Kind: UnitKind.Member }
        && unit.Symbol switch
        {
            IFieldSymbol field => field.IsStatic,
            IPropertySymbol property => property.IsStatic && CoreModel.IsDataProperty(property),
            _ => false,
        };

    /// <summary>Roots every unit of <paramref name="type"/>'s code.</summary>
    private static void RootAll(Walk walk, CoreModel model, INamedTypeSymbol type)
    {
        foreach (Unit unit in model.UnitsOf(type))
            walk.Root(unit);
    }

    /// <summary>Sends what <paramref name="walk"/> spawns with graphics drawing elsewhere to where they draw.</summary>
    private static void Route(Walk walk, INamedTypeSymbol? scope, Dictionary<INamedTypeSymbol, Collected> owned,
        Dictionary<INamedTypeSymbol, Collected> shared, Collected everywhere, Dictionary<string, Collected> sheets)
    {
        foreach ((SpawnTarget target, Collected spawns) in walk.Routed)
        {
            if (target.Sheet is { } sheet)
            {
                if (!sheets.TryGetValue(sheet, out Collected? held))
                    sheets.Add(sheet, held = new Collected());
                Merge(held, spawns);
                continue;
            }
            INamedTypeSymbol? to = target.Scope ?? scope;
            if (to is null)
                Merge(everywhere, spawns);
            else
                Merge(Of(target.Shared ? shared : owned, to), spawns);
        }
    }

    /// <summary>The collection for <paramref name="behaviour"/>, created empty when it has none.</summary>
    private static Collected Of(Dictionary<INamedTypeSymbol, Collected> collections, INamedTypeSymbol behaviour)
    {
        if (!collections.TryGetValue(behaviour, out Collected? collected))
            collections.Add(behaviour, collected = new Collected());
        return collected;
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
        // A family walk enters no behaviour code, so everything it names is its own, whatever graphics it spawns with.
        Merge(result, walk.Own);
        foreach (Collected routed in walk.Routed.Values)
            Merge(result, routed);
        foreach (INamedTypeSymbol spawned in result.Families.ToList())
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
}
