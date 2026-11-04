namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>Writes EnemyDefinitionFrameDefinitions.cs: the frames each enemy definition can show.</summary>
internal static class EnemyDefinitionFramesFile
{
    /// <summary>The generated file for <paramref name="analysis"/>.</summary>
    public static GeneratedFile Create(string repository, CoreModel model, SpawnAnalysis analysis)
    {
        var lines = new List<string>();
        lines.AddRange(CoreModel.Usings(model.ArchitectureReview, model.EnemyDefinitionId, model.EnemyFrameId));
        lines.AddRange(
        [
            "",
            "namespace SuperMetroid.AssetExtraction.Enemies.Artwork;",
            "",
            "/// <summary>",
            "/// The frames each enemy definition can show (#1392): the frames its behaviour's code reaches, which its pre-coloured",
            $"/// sheets hold. Generated from the enemy behaviours by {GeneratedFile.GeneratorPath}; regenerate rather than edit.",
            "/// </summary>",
            "[ArchitectureReview(\"large\", \"Generated data: the two thousand frames the enemy definitions show; it holds no logic, "
                + "and splitting it by family would only scatter one table the sheet composer reads as a whole.\")]",
            "internal static class EnemyDefinitionFrameDefinitions",
            "{",
        ]);
        var arrayOf = new Dictionary<string, string>(StringComparer.Ordinal);
        var names = new HashSet<string>(StringComparer.Ordinal);
        int frameCount = 0;
        foreach (BehaviourSpawns spawns in analysis.Behaviours)
        {
            Behaviour behaviour = spawns.Behaviour;
            // Definitions showing the same frames share one array; a behaviour whose definitions all do keeps its family's name.
            var groups = behaviour.Definitions
                .GroupBy(name => string.Join(',', CoreModel.InDeclarationOrder(model.EnemyFrameId, spawns.EnemyFrames[name])), StringComparer.Ordinal)
                .ToList();
            foreach (IGrouping<string, string> group in groups)
            {
                List<string> frames = CoreModel.InDeclarationOrder(model.EnemyFrameId, spawns.EnemyFrames[group.First()]);
                string name = groups.Count == 1 ? behaviour.Family : $"{group.First()}Frames";
                if (!names.Add(name))
                    throw new InvalidDataException($"Two frame arrays would both be named {name}.");
                frameCount += frames.Count;
                lines.Add(groups.Count == 1
                    ? $"    /// <summary>The frames <c>{behaviour.Path}</c> shows.</summary>"
                    : $"    /// <summary>The frames <c>{behaviour.Path}</c> shows for {string.Join(", ", group.Select(d => $"<c>{d}</c>"))}.</summary>");
                if (frames.Count == 0)
                {
                    lines.Add($"    private static readonly EnemyFrameId[] {name} = [];");
                }
                else
                {
                    lines.Add($"    private static readonly EnemyFrameId[] {name} =");
                    lines.Add("    [");
                    lines.AddRange(frames.Select(frame => $"        EnemyFrameId.{frame},"));
                    lines.Add("    ];");
                }
                lines.Add("");
                foreach (string definition in group)
                    arrayOf.Add(definition, name);
            }
        }
        lines.Add("    /// <summary>The frames each enemy definition can show.</summary>");
        lines.Add("    internal static IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>> Frames { get; } =");
        lines.Add("        new Dictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>>");
        lines.Add("        {");
        foreach (BehaviourSpawns spawns in analysis.Behaviours)
            lines.AddRange(spawns.Behaviour.Definitions.Select(name => $"            [EnemyDefinitionId.{name}] = {arrayOf[name]},"));
        lines.Add("        };");
        lines.Add("}");

        int definitions = analysis.Behaviours.Sum(s => s.Behaviour.Definitions.Count);
        return new GeneratedFile(
            Path.Combine(GeneratedFile.OutputDirectory(repository), "EnemyDefinitionFrameDefinitions.cs"),
            string.Join('\n', lines) + "\n",
            "enemy definition frames current",
            $"wrote EnemyDefinitionFrameDefinitions.cs: {analysis.Behaviours.Count} families, {definitions} definitions, {frameCount} frames");
    }
}
