namespace SuperMetroid.Core.Assets;

/// <summary>
/// Verification access: where a colour set run's colours lie in the cartridge's colour memory, for checks that compare
/// a run with a native routine's colour index (#1405). The background sets are its first half and the sprite sets its
/// second.
/// </summary>
internal static class ColourSetRunPositions
{
    /// <summary>The cartridge colour index of <paramref name="run"/>'s first colour.</summary>
    internal static int First(IColourSetRun run) => run switch
    {
        BackgroundColourRun background => background.Set * ColourSets.SetColours + background.Colour,
        SpriteColourRun sprite => ColourSets.SetCount * ColourSets.SetColours + sprite.Set * ColourSets.SetColours + sprite.Colour,
        _ => throw new ArgumentOutOfRangeException(nameof(run), run, "Unknown colour set run."),
    };

    /// <summary>The number of colours in <paramref name="run"/>.</summary>
    internal static int Count(IColourSetRun run) => run switch
    {
        BackgroundColourRun background => background.Count,
        SpriteColourRun sprite => sprite.Count,
        _ => throw new ArgumentOutOfRangeException(nameof(run), run, "Unknown colour set run."),
    };
}
