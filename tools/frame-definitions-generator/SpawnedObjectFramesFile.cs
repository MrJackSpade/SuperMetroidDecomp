namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// Writes SpawnedObjectFrameDefinitions.cs: the projectile frames and room sprite-object kinds each enemy definition
/// spawns onto its own sheet and onto its sets' shared sheet, those a sheet-specific owner's sheet holds, and those code
/// outside every behaviour spawns, by the common spawner whose code spawns it.
/// </summary>
internal static class SpawnedObjectFramesFile
{
    /// <summary>The generated file for <paramref name="analysis"/>.</summary>
    public static GeneratedFile Create(string repository, CoreModel model, SpawnAnalysis analysis)
    {
        var spawning = analysis.Behaviours.Where(s => s.ProjectileFrames.Count > 0 || s.SpriteObjects.Count > 0).ToList();
        var sharing = analysis.Behaviours.Where(s => s.SharedProjectileFrames.Count > 0 || s.SharedSpriteObjects.Count > 0).ToList();
        var lines = new List<string>();
        lines.AddRange(CoreModel.Usings(model.ArchitectureReview, model.EnemyProjectileFrameId, model.RoomSpriteObjectKind, model.EnemyDefinitionId));
        lines.AddRange(
        [
            "",
            "namespace SuperMetroid.AssetExtraction.Enemies.Artwork;",
            "",
            "/// <summary>",
            "/// The projectile frames and room sprite-object kinds each enemy definition can spawn, and those each common spawner (code",
            "/// outside the enemy behaviours) spawns (#1392), which the pre-coloured spawned-object sheets hold, each on the sheet its spawn's graphics name (#1513).",
            $"/// Generated from the source by {GeneratedFile.GeneratorPath}; regenerate rather than edit.",
            "/// </summary>",
            "[ArchitectureReview(\"large\", \"Generated data: the projectile frames and sprite-object kinds each enemy definition spawns; "
                + "it holds no logic, and splitting it by family would only scatter one table the sheet composer reads as a whole.\")]",
            "internal static class SpawnedObjectFrameDefinitions",
            "{",
        ]);
        foreach (BehaviourSpawns spawns in spawning)
        {
            string family = spawns.Behaviour.Family;
            string path = spawns.Behaviour.Path;
            AddArray(lines, "EnemyProjectileFrameId", $"{family}Projectiles",
                CoreModel.InDeclarationOrder(model.EnemyProjectileFrameId, spawns.ProjectileFrames), $"The projectile frames <c>{path}</c> spawns.");
            AddArray(lines, "RoomSpriteObjectKind", $"{family}SpriteObjects",
                SortedKinds(spawns.SpriteObjects), $"The room sprite objects <c>{path}</c> spawns.");
        }
        foreach (BehaviourSpawns spawns in sharing)
        {
            string family = spawns.Behaviour.Family;
            string path = spawns.Behaviour.Path;
            AddArray(lines, "EnemyProjectileFrameId", $"{family}SharedProjectiles",
                CoreModel.InDeclarationOrder(model.EnemyProjectileFrameId, spawns.SharedProjectileFrames),
                $"The projectile frames <c>{path}</c> spawns onto its sets' shared sheet.");
            AddArray(lines, "RoomSpriteObjectKind", $"{family}SharedSpriteObjects",
                SortedKinds(spawns.SharedSpriteObjects), $"The room sprite objects <c>{path}</c> spawns onto its sets' shared sheet.");
        }
        foreach ((string sheet, Collected held) in analysis.BySheet.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            AddArray(lines, "EnemyProjectileFrameId", $"{sheet}SheetProjectiles",
                CoreModel.InDeclarationOrder(model.EnemyProjectileFrameId, held.ProjectileFrames),
                $"The projectile frames <see cref=\"SpawnedObjectOwner.{sheet}\"/>'s sheet holds.");
            AddArray(lines, "RoomSpriteObjectKind", $"{sheet}SheetSpriteObjects",
                SortedKinds(held.SpriteObjects), $"The room sprite objects <see cref=\"SpawnedObjectOwner.{sheet}\"/>'s sheet holds.");
        }
        var commonSpawners = analysis.Common.Keys.Order(StringComparer.Ordinal).ToList();
        foreach (string spawner in commonSpawners)
        {
            Collected held = analysis.Common[spawner];
            string name = Identifier(spawner);
            AddArray(lines, "EnemyProjectileFrameId", $"Common{name}Projectiles",
                CoreModel.InDeclarationOrder(model.EnemyProjectileFrameId, held.ProjectileFrames),
                $"The projectile frames <c>{spawner}</c>'s code spawns onto the shared sheet of the sets whose rooms run it.");
            AddArray(lines, "RoomSpriteObjectKind", $"Common{name}SpriteObjects",
                SortedKinds(held.SpriteObjects), $"The room sprite objects <c>{spawner}</c>'s code spawns onto the shared sheet of the sets whose rooms run it.");
        }
        lines.AddRange(
        [
            "    /// <summary>What each enemy definition can spawn onto its own spawned-object sheet.</summary>",
            "    internal static IReadOnlyDictionary<EnemyDefinitionId, SpawnedObjects> ByEnemy { get; } =",
            "        new Dictionary<EnemyDefinitionId, SpawnedObjects>",
            "        {",
        ]);
        foreach (BehaviourSpawns spawns in spawning)
        {
            string family = spawns.Behaviour.Family;
            lines.AddRange(spawns.Behaviour.Definitions.Select(name =>
                $"            [EnemyDefinitionId.{name}] = new({family}Projectiles, {family}SpriteObjects),"));
        }
        lines.AddRange(
        [
            "        };",
            "",
            "    /// <summary>What each enemy definition can spawn onto the shared sheet of the graphics sets holding it.</summary>",
            "    internal static IReadOnlyDictionary<EnemyDefinitionId, SpawnedObjects> SharedByEnemy { get; } =",
            "        new Dictionary<EnemyDefinitionId, SpawnedObjects>",
            "        {",
        ]);
        foreach (BehaviourSpawns spawns in sharing)
        {
            string family = spawns.Behaviour.Family;
            lines.AddRange(spawns.Behaviour.Definitions.Select(name =>
                $"            [EnemyDefinitionId.{name}] = new({family}SharedProjectiles, {family}SharedSpriteObjects),"));
        }
        lines.AddRange(
        [
            "        };",
            "",
            "    /// <summary>What each sheet-specific owner's sheet holds: only what is spawned with graphics naming that owner.</summary>",
            "    internal static IReadOnlyDictionary<SpawnedObjectOwner, SpawnedObjects> BySheet { get; } =",
            "        new Dictionary<SpawnedObjectOwner, SpawnedObjects>",
            "        {",
        ]);
        foreach (string sheet in analysis.BySheet.Keys.Order(StringComparer.Ordinal))
            lines.Add($"            [SpawnedObjectOwner.{sheet}] = new({sheet}SheetProjectiles, {sheet}SheetSpriteObjects),");
        lines.AddRange(
        [
            "        };",
            "",
            "    /// <summary>",
            "    /// What each common spawner, code outside the enemy behaviours, spawns onto the shared sheet, by the spawner the import",
            "    /// places in the rooms whose data runs it: a type's name, or <c>Type.Member</c> for a type whose members serve unrelated rooms.",
            "    /// </summary>",
            "    internal static IReadOnlyList<CommonSpawnedObjects> Common { get; } =",
            "    [",
        ]);
        foreach (string spawner in commonSpawners)
        {
            string name = Identifier(spawner);
            lines.Add($"        new(\"{spawner}\", new(Common{name}Projectiles, Common{name}SpriteObjects)),");
        }
        lines.AddRange(
        [
            "    ];",
            "}",
        ]);

        return new GeneratedFile(
            Path.Combine(GeneratedFile.OutputDirectory(repository), "SpawnedObjectFrameDefinitions.cs"),
            string.Join('\n', lines) + "\n",
            "spawned object frames current",
            $"wrote SpawnedObjectFrameDefinitions.cs: {spawning.Count} spawning behaviours, {sharing.Count} sharing behaviours, "
                + $"{analysis.BySheet.Count} owner sheets, {commonSpawners.Count} common spawners");
    }

    /// <summary>The identifier fragment a spawner's arrays are named by: its name without separators.</summary>
    private static string Identifier(string spawner) => spawner.Replace(".", "", StringComparison.Ordinal);

    /// <summary>Sprite-object kinds in ordinal name order.</summary>
    private static List<string> SortedKinds(IEnumerable<string> kinds) => [.. kinds.Order(StringComparer.Ordinal)];

    /// <summary>Adds one documented array of <paramref name="element"/> members.</summary>
    private static void AddArray(List<string> lines, string element, string name, List<string> values, string summary)
    {
        lines.Add($"    /// <summary>{summary}</summary>");
        if (values.Count == 0)
        {
            lines.Add($"    private static readonly {element}[] {name} = [];");
            lines.Add("");
            return;
        }
        lines.Add($"    private static readonly {element}[] {name} =");
        lines.Add("    [");
        lines.AddRange(values.Select(value => $"        {element}.{value},"));
        lines.Add("    ];");
        lines.Add("");
    }
}
