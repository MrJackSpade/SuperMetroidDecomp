using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    private readonly Dictionary<(INamedTypeSymbol Family, string Programs), FamilyClosure> _familyClosures = [];

    /// <summary>Creates the analysis over <paramref name="model"/>.</summary>
    private SpawnAnalysis(CoreModel model) => _model = model;

    /// <summary>What each behaviour shows and spawns, in behaviour order.</summary>
    public IReadOnlyList<BehaviourSpawns> Behaviours { get; private set; } = [];

    /// <summary>
    /// What each common spawner (code outside every behaviour, by <see cref="CoreModel.SpawnerKey"/>) spawns onto the shared
    /// sheet of the sets whose rooms run it, with the families it spawns; spawners that spawn nothing drawn are left out.
    /// </summary>
    public IReadOnlyDictionary<string, Collected> Common { get; private set; } = new Dictionary<string, Collected>();

    /// <summary>For each common spawner, the units its spawns were met in and the units outside it reaching them.</summary>
    public IReadOnlyDictionary<string, (IReadOnlyList<Unit> Units, IReadOnlyList<Unit> ReachedFrom)> CommonSites { get; private set; } =
        new Dictionary<string, (IReadOnlyList<Unit>, IReadOnlyList<Unit>)>();

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
        RequireEmptyFrames(model);
        RequireSamusHitBursts(model);
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
            IReadOnlySet<string> familyFrames = analysis.WithFamilies(walk.Own, switchedToRoom: null).EnemyFrames;
            DefinitionFrameSplit frames = behaviour.Definitions.Count == 1
                ? new DefinitionFrameSplit(new Dictionary<string, IReadOnlySet<string>> { [behaviour.Definitions[0]] = familyFrames }, [])
                : new DefinitionFrames(model, scanner, behaviour, walk).Resolve(familyFrames);
            // A behaviour whose definitions each select their own frame tables keeps each table on its own definitions.
            if (DefinitionTableConfiguration.All.SingleOrDefault(configuration => configuration.Behaviour == behaviour.Type.Name) is { } tables)
                frames = frames with { Frames = new DefinitionTables(model, tables).Apply(behaviour, frames.Frames) };
            // A behaviour whose parts share one frame table splits it by what each part's own programs show.
            if (PartFrameConfiguration.All.SingleOrDefault(configuration => configuration.Behaviour == behaviour.Type.Name) is { } parts)
                frames = frames with { Frames = new PartFrames(model, parts).Apply(behaviour, frames.Frames) };
            walks.Add((behaviour, walk, frames));
        }
        if (PartFrameConfiguration.All.FirstOrDefault(configuration => model.Behaviours.All(b => b.Type.Name != configuration.Behaviour)) is { } unknown)
            throw new InvalidDataException($"The part frame configuration names {unknown.Behaviour}, which is no enemy behaviour.");
        if (DefinitionTableConfiguration.All.FirstOrDefault(configuration => model.Behaviours.All(b => b.Type.Name != configuration.Behaviour)) is { } unknownTables)
            throw new InvalidDataException($"The definition table configuration names {unknownTables.Behaviour}, which is no enemy behaviour.");

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
                spawnedFamilies.UnionWith(spawned.AllFamilies);
        }
        foreach (INamedTypeSymbol family in spawnedFamilies.ToList())
            spawnedFamilies.UnionWith(analysis.Closure(family, programs: null, []).Shown.AllFamilies);
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
                        || !spawns.AllFamilies.All(family => held.AllFamilies.Contains<INamedTypeSymbol>(family, SymbolEqualityComparer.Default)))
                    {
                        unreadable.Add($"{walk.PendingSites[key]}: spawns with graphics {Describe(key)} are missing from {destination}");
                    }
                }
            }
        }
        // A spawn whose graphics name no sheet this generator can read would draw from a sheet that lacks it.
        if (unreadable.Count > 0)
            throw new InvalidDataException("Spawn graphics the generator cannot place on a sheet:\n" + string.Join('\n', unreadable));
        analysis.BySheet = sheets.Sheets.ToDictionary(pair => pair.Key, pair => analysis.WithFamilies(pair.Value, switchedToRoom: null), StringComparer.Ordinal);

        var behaviours = new List<BehaviourSpawns>();
        foreach ((Behaviour behaviour, Walk walk, DefinitionFrameSplit frames) in walks)
        {
            // What a behaviour spawns with its own graphics and shows after a cue switched it to the room's draws from its
            // sets' shared sheet (#1513).
            var switched = new Collected();
            Collected own = analysis.WithFamilies(sheets.Of(new Destination(DestinationKind.Own, behaviour.Type)), switched);
            var sharedSource = new Collected();
            Merge(sharedSource, sheets.Of(new Destination(DestinationKind.Shared, behaviour.Type)));
            sharedSource.ProjectileFrames.UnionWith(switched.ProjectileFrames);
            Collected sharedSpawns = analysis.WithFamilies(sharedSource, switchedToRoom: null);
            behaviours.Add(new BehaviourSpawns(behaviour, frames.Frames, own.ProjectileFrames, own.SpriteObjects,
                sharedSpawns.ProjectileFrames, sharedSpawns.SpriteObjects, walk.ForeignEntries, frames.Notes));
        }
        analysis.Behaviours = behaviours;
        var common = new Dictionary<string, Collected>(StringComparer.Ordinal);
        foreach ((string spawner, Collected spawns) in sheets.Common)
        {
            Collected held = analysis.WithFamilies(spawns, switchedToRoom: null);
            if (held.ProjectileFrames.Count > 0 || held.SpriteObjects.Count > 0)
                common.Add(spawner, held);
        }
        analysis.Common = common;
        analysis.CommonSites = common.Keys.ToDictionary(spawner => spawner, spawner =>
        {
            // A spawner's units are met only in the walk outside the behaviours, the one walk whose spawns resolve to it.
            var units = new HashSet<Unit>(spawnWalks.SelectMany(walk => walk.SpawnerUnits.GetValueOrDefault(spawner) ?? []));
            IReadOnlyList<Unit> from = [.. units.SelectMany(unit => anywhere.CallersOf(unit))
                .Where(caller => !units.Contains(caller))
                .Distinct().OrderBy(model.Describe, StringComparer.Ordinal)];
            IReadOnlyList<Unit> sites = [.. units.OrderBy(model.Describe, StringComparer.Ordinal)];
            return (sites, from);
        }, StringComparer.Ordinal);
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
        output.WriteLine($"code outside the behaviours naming spawns: {string.Join(", ", AnywhereSpawners.Select(_model.Describe))}");
        foreach ((string spawner, (IReadOnlyList<Unit> units, IReadOnlyList<Unit> from)) in CommonSites.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            Collected held = Common[spawner];
            output.WriteLine($"common spawner {spawner}: {held.ProjectileFrames.Count} projectile frames, {held.SpriteObjects.Count} sprite objects; "
                + $"spawns in {string.Join(", ", units.Select(_model.Describe))}; reached from {string.Join(", ", from.Select(_model.Describe))}");
        }
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

    /// <summary>
    /// Requires the projectile frames the generator excludes, the blank frame aside, to be exactly those Core's
    /// <c>EnemyProjectileEmptyFrameDefinitions.EmptyFrames</c> lists as drawing nothing, so a frame Core draws
    /// nothing for is on no sheet and a frame Core draws is on its sheet.
    /// </summary>
    /// <exception cref="InvalidDataException">The catalog is missing or lists other frames.</exception>
    private static void RequireEmptyFrames(CoreModel model)
    {
        INamedTypeSymbol[] catalogs = [.. model.Types.Where(type => type.Name == "EnemyProjectileEmptyFrameDefinitions")];
        if (catalogs is not [var catalog] || catalog.GetMembers("EmptyFrames") is not [IFieldSymbol field])
            throw new InvalidDataException("Core declares no single EnemyProjectileEmptyFrameDefinitions.EmptyFrames.");
        var listed = new SortedSet<string>(StringComparer.Ordinal);
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
        var excluded = new SortedSet<string>(Walk.ExcludedProjectileFrames, StringComparer.Ordinal);
        excluded.Remove("Blank");
        if (!listed.SetEquals(excluded))
        {
            throw new InvalidDataException("The generator's excluded projectile frames (" + string.Join(", ", excluded)
                + ") are not the frames EnemyProjectileEmptyFrameDefinitions.EmptyFrames lists (" + string.Join(", ", listed) + ").");
        }
    }

    /// <summary>
    /// Requires the sprite-object kinds the generator excludes to be exactly those Core's
    /// <c>RoomSpriteObjectVisualDefinitions.SamusHitBursts</c> lists as Samus's weapon effects (#1713), so a kind her
    /// projectile sheets picture is on no enemy sheet and every other kind is on its sheet.
    /// </summary>
    /// <exception cref="InvalidDataException">The catalog is missing or lists other kinds.</exception>
    private static void RequireSamusHitBursts(CoreModel model)
    {
        INamedTypeSymbol[] catalogs = [.. model.Types.Where(type => type.Name == "RoomSpriteObjectVisualDefinitions")];
        if (catalogs is not [var catalog] || catalog.GetMembers("SamusHitBursts") is not [IPropertySymbol property])
            throw new InvalidDataException("Core declares no single RoomSpriteObjectVisualDefinitions.SamusHitBursts.");
        var listed = new SortedSet<string>(StringComparer.Ordinal);
        foreach (SyntaxReference reference in property.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not PropertyDeclarationSyntax { Initializer.Value: var value })
                continue;
            SemanticModel semantic = model.ModelOf(value.SyntaxTree);
            foreach (ExpressionSyntax expression in value.DescendantNodes().OfType<ExpressionSyntax>())
            {
                if (semantic.GetSymbolInfo(expression).Symbol is IFieldSymbol member
                    && SymbolEqualityComparer.Default.Equals(member.ContainingType, model.RoomSpriteObjectKind))
                    listed.Add(member.Name);
            }
        }
        var excluded = new SortedSet<string>(Walk.ExcludedSpriteObjects, StringComparer.Ordinal);
        if (!listed.SetEquals(excluded))
        {
            throw new InvalidDataException("The generator's excluded sprite objects (" + string.Join(", ", excluded)
                + ") are not the kinds RoomSpriteObjectVisualDefinitions.SamusHitBursts lists (" + string.Join(", ", listed) + ").");
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

    /// <summary>
    /// <paramref name="collected"/> with everything the families it constructs or starts name added, each family's frames
    /// those of the programs it is started in (#1513). The frames those programs show once a cue has switched an object to
    /// the room's graphics go to <paramref name="switchedToRoom"/> when it is given (a behaviour's own sheet, whose
    /// switched objects draw from the shared sheet), and stay with the rest otherwise.
    /// </summary>
    private Collected WithFamilies(Collected collected, Collected? switchedToRoom)
    {
        var result = new Collected();
        Merge(result, collected);
        var expected = new List<(INamedTypeSymbol Family, FamilyClosure Closure)>();
        foreach (INamedTypeSymbol family in collected.AllFamilies.ToList())
        {
            FamilyClosure closure = Closure(family, collected.ProgramsOf(family), []);
            expected.Add((family, closure));
            Merge(result, closure.Shown);
            (switchedToRoom ?? result).ProjectileFrames.UnionWith(closure.Room);
        }
        // The self-check: a sheet holding a family holds every frame its programs can show under the sheet's graphics (its
        // walk's frames and the families it spawns in turn), so a family routed to a sheet without its later frames fails
        // here rather than when it draws.
        var incomplete = new List<string>();
        foreach ((INamedTypeSymbol family, FamilyClosure closure) in expected)
        {
            if (!closure.Shown.ProjectileFrames.IsSubsetOf(result.ProjectileFrames) || !closure.Shown.SpriteObjects.IsSubsetOf(result.SpriteObjects))
                incomplete.Add($"{family.ToDisplayString()} is routed to a sheet missing some of its frames");
        }
        if (incomplete.Count > 0)
            throw new InvalidDataException("Projectile families incomplete on a sheet:\n" + string.Join('\n', incomplete.Distinct()));
        return result;
    }

    /// <summary>
    /// What <paramref name="family"/> shows when started in <paramref name="programs"/> (or any of its programs, when null),
    /// with the families it spawns in turn: everything its walk names except the frames of its table its programs do not
    /// show (#1513), and apart the frames its programs show only once a cue has switched the object to the room's graphics.
    /// </summary>
    private FamilyClosure Closure(INamedTypeSymbol family, IReadOnlySet<string>? programs, HashSet<INamedTypeSymbol> visiting)
    {
        (INamedTypeSymbol, string) key = (family, programs is null ? "*" : string.Join(',', programs.Order(StringComparer.Ordinal)));
        if (_familyClosures.TryGetValue(key, out FamilyClosure? cached))
            return cached;
        var shown = new Collected();
        var room = new HashSet<string>(StringComparer.Ordinal);
        if (!visiting.Add(family))
            return new FamilyClosure(shown, room);
        Walk walk = _familyWalks[family];
        ProjectileProgramFrames programFrames = _model.ProgramFrames;
        IReadOnlySet<string> table = programFrames.TableFrames(family);
        ProgramFrameSplit split = programFrames.Split(family, programs);
        // A family walk enters no behaviour code, so everything it names is its own, whatever graphics it spawns with; of
        // its table, only what its programs show.
        Merge(shown, walk.Own);
        foreach (Collected spawned in walk.AllSpawns)
            Merge(shown, spawned);
        shown.ProjectileFrames.ExceptWith(table);
        shown.ProjectileFrames.UnionWith(split.Spawned.Where(frame => !Walk.ExcludedProjectileFrames.Contains(frame)));
        room.UnionWith(split.Room.Where(frame => !Walk.ExcludedProjectileFrames.Contains(frame)));
        foreach (INamedTypeSymbol spawned in shown.AllFamilies.ToList())
        {
            FamilyClosure nested = Closure(spawned, shown.ProgramsOf(spawned), visiting);
            Merge(shown, nested.Shown);
            room.UnionWith(nested.Room);
        }
        visiting.Remove(family);
        var result = new FamilyClosure(shown, room);
        if (visiting.Count == 0)
            _familyClosures.Add(key, result);
        return result;
    }

    /// <summary>What a family started in some programs shows.</summary>
    /// <param name="Shown">Everything it names and shows under the graphics it was spawned with, with the families it spawns.</param>
    /// <param name="Room">The frames it shows only once a cue has switched it to the room's graphics.</param>
    private sealed record FamilyClosure(Collected Shown, IReadOnlySet<string> Room);

    /// <summary>Adds everything <paramref name="source"/> names to <paramref name="target"/>.</summary>
    private static void Merge(Collected target, Collected source)
    {
        target.EnemyFrames.UnionWith(source.EnemyFrames);
        target.ProjectileFrames.UnionWith(source.ProjectileFrames);
        target.SpriteObjects.UnionWith(source.SpriteObjects);
        target.AddFamiliesOf(source);
    }
}
