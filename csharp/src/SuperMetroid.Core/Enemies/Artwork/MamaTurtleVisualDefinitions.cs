using SuperMetroid.Core.Enemies.MamaTurtles;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Editable Mama/Baby Turtle OAM compositions selected by the compiled tatori
/// programs. Movement, shell callbacks, durations and collision remain in code.
/// </summary>
internal static class MamaTurtleVisualDefinitions
{
    /// <summary>Tatori instruction and OAM bank $A2.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>Twenty-nine selected Spritemap_BabyTurtle/MamaTurtle records at $A2:94D9..983F.</summary>
    internal const int FrameCount = 29;

    /// <summary>
    /// Exports each distinct OAM composition the turtles' programs show once, in the order the programs first show
    /// it, requiring all 29 native frames.
    /// </summary>
    internal static EnemySpritemapDefinition[] Frames()
    {
        var frames = new List<EnemySpritemapDefinition>(FrameCount);
        var seen = new HashSet<ushort>();
        foreach (ushort pointer in MamaTurtleAnimations.Spritemaps(Enum.GetValues<MamaTurtleProgram>()))
        {
            // Crawling, shell and spinning programs reuse native compositions.
            // Export each actual selection once, leaving repetitions in the programs.
            if (seen.Add(pointer))
                frames.Add(new(Bank, pointer, $"tatori_{frames.Count:D2}"));
        }
        if (frames.Count != FrameCount)
            throw new InvalidDataException(
                $"Tatori programs select {frames.Count} distinct OAM frames, expected {FrameCount}.");
        return [.. frames];
    }
}
