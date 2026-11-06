namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// How a behaviour whose definitions each run their own program set and frame table splits its frames (#1513): its code
/// runs whichever table the slot's definition selects, so the walk alone would give every definition every table; instead
/// each definition shows only the frames of its own tables.
/// </summary>
/// <param name="Behaviour">The behaviour type's name.</param>
/// <param name="Tables">Each definition's frame tables, as <c>Type.Member</c>.</param>
internal sealed record DefinitionTableConfiguration(string Behaviour, IReadOnlyDictionary<string, string[]> Tables)
{
    /// <summary>
    /// The configured behaviours: the escaping Etecoon and Dachora each run their own program set over their own frame
    /// table, chosen by the slot's definition.
    /// </summary>
    public static IReadOnlyList<DefinitionTableConfiguration> All { get; } =
    [
        new("EscapeAnimalsBehavior", new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["EtecoonEscape"] = ["EscapeEtecoonFrameDefinitions.ShownFrames"],
            ["DachoraEscape"] = ["EscapeDachoraFrameDefinitions.ShownFrames"],
        }),
    ];
}
