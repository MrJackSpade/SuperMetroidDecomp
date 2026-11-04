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
/// <param name="FrameNotes">For a behaviour serving several definitions, each unit whose frames every definition shows.</param>
internal sealed record BehaviourSpawns(
    Behaviour Behaviour,
    IReadOnlyDictionary<string, IReadOnlySet<string>> EnemyFrames,
    IReadOnlySet<string> ProjectileFrames,
    IReadOnlySet<string> SpriteObjects,
    IReadOnlySet<string> SharedProjectileFrames,
    IReadOnlySet<string> SharedSpriteObjects,
    IReadOnlyList<(Unit From, Unit To)> ForeignEntries,
    IReadOnlyList<string> FrameNotes);

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

        var walks = new List<(Behaviour Behaviour, Walk Walk, DefinitionFrameSplit Frames)>();
        foreach (Behaviour behaviour in model.Behaviours)
        {
            var walk = new Walk(model, scanner, behaviour.Type, family: null);
            RootBehaviour(walk, model, behaviour);
            walk.Run();
            reached.UnionWith(walk.Reached);
            IReadOnlySet<string> familyFrames = analysis.WithFamilies(walk.Own).EnemyFrames;
            DefinitionFrameSplit frames = behaviour.Definitions.Count == 1
                ? new DefinitionFrameSplit(new Dictionary<string, IReadOnlySet<string>> { [behaviour.Definitions[0]] = familyFrames }, [])
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

        // Each walk's spawns go to every sheet their graphics can name; then the graphics other code decides are resolved,
        // and graphics a type's handlers switch its objects to add that type's spawns to those sheets too.
        var sheets = new SheetCollections();
        var spawnWalks = walks.Select(w => w.Walk).Append(anywhere).ToList();
        foreach (Walk walk in spawnWalks)
        {
            foreach ((Destination destination, Collected spawns) in walk.Spawns)
                Merge(sheets.Of(destination), spawns);
        }
        var unreadable = new SortedSet<string>(spawnWalks.Concat(analysis._familyWalks.Values).SelectMany(w => w.Unreadable), StringComparer.Ordinal);
        var resolver = new PendingResolver(spawnWalks);
        foreach (Walk walk in spawnWalks)
        {
            foreach ((PendingKey key, Collected spawns) in walk.Pending)
            {
                var destinations = new HashSet<Destination>(resolver.Resolve(key));
                // Graphics copied from the walk's own objects may also be those its behaviour's handlers switch them to.
                if (walk.Behaviour is { } owner && destinations.Count > 0)
                    destinations.UnionWith(resolver.ReassignedFor(owner));
                if (destinations.Count == 0)
                    unreadable.Add($"{walk.PendingSites[key]}: graphics {Describe(key)} reach no sheet: no code spawning with a readable sheet decides them");
                foreach (Destination destination in destinations)
                    Merge(sheets.Of(destination), spawns);
            }
        }
        foreach ((INamedTypeSymbol scope, IReadOnlySet<Destination> destinations) in resolver.Reassignments())
        {
            bool linked = false;
            foreach (Walk walk in spawnWalks)
            {
                // A behaviour's handlers switch the graphics of whatever its walk spawns; outside every behaviour, of what
                // the switching code's type spawns.
                IEnumerable<Collected> evidence = SymbolEqualityComparer.Default.Equals(walk.Behaviour, scope)
                    ? walk.EvidenceByType.Values
                    : walk.EvidenceByType.TryGetValue(scope, out Collected? typed) ? [typed] : [];
                foreach (Collected spawned in evidence)
                {
                    linked = true;
                    foreach (Destination destination in destinations)
                        Merge(sheets.Of(destination), spawned);
                }
            }
            if (!linked)
                unreadable.Add($"{scope.ToDisplayString()}: graphics its handlers switch to ({string.Join(", ", destinations)}) reach no spawn the generator can link");
        }
        // The self-check: a family code constructs (by constructor or its own factory) is spawned by some code a walk
        // reaches, so no family's frames are missing from every sheet because its construction went unseen.
        var spawnedFamilies = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (Walk walk in spawnWalks)
        {
            foreach (Collected spawned in walk.EvidenceByType.Values)
                spawnedFamilies.UnionWith(spawned.Families);
        }
        foreach (INamedTypeSymbol family in spawnedFamilies.ToList())
            spawnedFamilies.UnionWith(analysis.Closure(family, new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default)).Families);
        foreach (INamedTypeSymbol family in model.ConstructedFamilies.Where(f => !spawnedFamilies.Contains(f)))
            unreadable.Add($"{family.ToDisplayString()} is constructed in code no walk records as spawning it");
        // The self-check: every spawn with copied or handed-on graphics is on every sheet the code deciding them names (a
        // falling spark's trail on the sheet of whoever spawned the spark), and no spawn's graphics are unreadable.
        foreach (Walk walk in spawnWalks)
        {
            foreach ((PendingKey key, Collected spawns) in walk.Pending)
            {
                foreach (Destination destination in resolver.Resolve(key))
                {
                    Collected held = sheets.Of(destination);
                    if (!spawns.ProjectileFrames.IsSubsetOf(held.ProjectileFrames) || !spawns.SpriteObjects.IsSubsetOf(held.SpriteObjects)
                        || !spawns.Families.IsSubsetOf(held.Families))
                    {
                        unreadable.Add($"{walk.PendingSites[key]}: spawns with graphics {Describe(key)} are missing from {destination}");
                    }
                }
            }
        }
        // A spawn whose graphics name no sheet this generator can read would draw from a sheet that lacks it.
        if (unreadable.Count > 0)
            throw new InvalidDataException("Spawn graphics the generator cannot place on a sheet:\n" + string.Join('\n', unreadable));
        analysis.BySheet = sheets.Sheets.ToDictionary(pair => pair.Key, pair => analysis.WithFamilies(pair.Value), StringComparer.Ordinal);
        Collected everywhere = sheets.Anywhere;

        var behaviours = new List<BehaviourSpawns>();
        foreach ((Behaviour behaviour, Walk walk, DefinitionFrameSplit frames) in walks)
        {
            Collected own = analysis.WithFamilies(sheets.Of(new Destination(DestinationKind.Own, behaviour.Type)));
            Collected sharedSpawns = analysis.WithFamilies(sheets.Of(new Destination(DestinationKind.Shared, behaviour.Type)));
            behaviours.Add(new BehaviourSpawns(behaviour, frames.Frames, own.ProjectileFrames, own.SpriteObjects,
                sharedSpawns.ProjectileFrames, sharedSpawns.SpriteObjects, walk.ForeignEntries, frames.Notes));
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
            if (spawns.EnemyFrames.Count > 1)
            {
                foreach ((string definition, IReadOnlySet<string> frames) in spawns.EnemyFrames)
                    output.WriteLine($"{spawns.Behaviour.Family}: {definition} shows {frames.Count} frames");
                foreach (string note in spawns.FrameNotes)
                    output.WriteLine($"{spawns.Behaviour.Family}: {note}");
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

    /// <summary>Names pending graphics for the self-check's report.</summary>
    private static string Describe(PendingKey key) => key.Source == GraphicsSource.Inherited
        ? $"copied from objects {key.Key.ToDisplayString()}'s code spawns"
        : $"handed to {key.Key.ToDisplayString()}";

    /// <summary><paramref name="collected"/> with everything the families it constructs or starts name added.</summary>
    private Collected WithFamilies(Collected collected)
    {
        var result = new Collected();
        Merge(result, collected);
        foreach (INamedTypeSymbol family in collected.Families)
            Merge(result, Closure(family, new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default)));
        // The self-check: a sheet holding a family holds every frame its programs can show (its walk's frames and the
        // families it spawns in turn), so a family routed to a sheet without its later frames fails here rather than when
        // it draws.
        var incomplete = new List<string>();
        foreach (INamedTypeSymbol family in result.Families)
        {
            Collected closure = Closure(family, new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default));
            if (!closure.ProjectileFrames.IsSubsetOf(result.ProjectileFrames) || !closure.SpriteObjects.IsSubsetOf(result.SpriteObjects))
                incomplete.Add($"{family.ToDisplayString()} is routed to a sheet missing some of its frames");
        }
        if (incomplete.Count > 0)
            throw new InvalidDataException("Projectile families incomplete on a sheet:\n" + string.Join('\n', incomplete.Distinct()));
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
        foreach (Collected spawned in walk.AllSpawns)
            Merge(result, spawned);
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
