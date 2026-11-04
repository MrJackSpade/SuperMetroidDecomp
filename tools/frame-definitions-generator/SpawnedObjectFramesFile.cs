namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// Writes SpawnedObjectFrameDefinitions.cs: the projectile frames and room sprite-object kinds each enemy definition
/// can spawn, and those code outside every behaviour spawns in any room.
/// </summary>
internal static class SpawnedObjectFramesFile
{
    /// <summary>The generated file for <paramref name="analysis"/>.</summary>
    public static GeneratedFile Create(string repository, CoreModel model, SpawnAnalysis analysis)
    {
        var spawning = analysis.Behaviours.Where(s => s.ProjectileFrames.Count > 0 || s.SpriteObjects.Count > 0).ToList();
        var lines = new List<string>();
        lines.AddRange(CoreModel.Usings(model.ArchitectureReview, model.EnemyProjectileFrameId, model.RoomSpriteObjectKind, model.EnemyDefinitionId));
        lines.AddRange(
        [
            "",
            "namespace SuperMetroid.AssetExtraction.Enemies.Artwork;",
            "",
            "/// <summary>",
            "/// The projectile frames and room sprite-object kinds each enemy definition can spawn, and those any room can show",
            "/// (#1392), which the pre-coloured spawned-object sheets hold. Generated from the source by",
            $"/// {GeneratedFile.GeneratorPath}; regenerate rather than edit.",
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
        List<string> anywhereFrames = CoreModel.InDeclarationOrder(model.EnemyProjectileFrameId, analysis.AnywhereProjectileFrames);
        List<string> anywhereKinds = SortedKinds(analysis.AnywhereSpriteObjects);
        AddArray(lines, "EnemyProjectileFrameId", "AnywhereProjectiles", anywhereFrames,
            "The projectile frames code outside the enemy behaviours spawns in any room.");
        AddArray(lines, "RoomSpriteObjectKind", "AnywhereSpriteObjects", anywhereKinds,
            "The room sprite objects code outside the enemy behaviours spawns in any room.");
        lines.AddRange(
        [
            "    /// <summary>What each enemy definition can spawn.</summary>",
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
            "    /// <summary>What code outside the enemy behaviours spawns, in any room.</summary>",
            "    internal static SpawnedObjects Anywhere { get; } = new(AnywhereProjectiles, AnywhereSpriteObjects);",
            "}",
        ]);

        return new GeneratedFile(
            Path.Combine(GeneratedFile.OutputDirectory(repository), "SpawnedObjectFrameDefinitions.cs"),
            string.Join('\n', lines) + "\n",
            "spawned object frames current",
            $"wrote SpawnedObjectFrameDefinitions.cs: {spawning.Count} spawning behaviours, {anywhereFrames.Count} anywhere frames, "
                + $"{anywhereKinds.Count} anywhere kinds");
    }

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
