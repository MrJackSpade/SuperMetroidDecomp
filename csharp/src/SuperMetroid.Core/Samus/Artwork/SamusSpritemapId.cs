namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// One Samus spritemap composition: its place in the installed composition list, which keeps the cartridge's record
/// order, so a run's next composition is the next one stored.
/// </summary>
/// <param name="Ordinal">The composition's place in the installed list.</param>
public readonly record struct SamusSpritemapId(int Ordinal)
{
    /// <summary>The composition <paramref name="count"/> places after this one in record order.</summary>
    public SamusSpritemapId After(int count) => new(Ordinal + count);
}
