namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// How a behaviour whose definitions each run their own animations splits its frames (#1513): its code runs whichever
/// animations the slot's definition selects, so the walk alone would give every definition every one's frames; instead
/// each definition shows only the frames its own animations' segments name (#1623).
/// </summary>
/// <param name="Behaviour">The behaviour type's name.</param>
/// <param name="Tables">Each definition's frame-naming tables (its animations' segments), as <c>Type.Member</c>.</param>
internal sealed record DefinitionTableConfiguration(string Behaviour, IReadOnlyDictionary<string, string[]> Tables)
{
    /// <summary>
    /// The configured behaviours: the escaping Etecoon and Dachora each run their own animations, chosen by the slot's
    /// definition.
    /// </summary>
    public static IReadOnlyList<DefinitionTableConfiguration> All { get; } =
    [
        new("EscapeAnimalsBehavior", new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["EtecoonEscape"] = ["EscapeEtecoonAnimations.Segments"],
            ["DachoraEscape"] = ["EscapeDachoraAnimations.Segments"],
        }),
    ];
}
