using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>A sprite look and the sprite characters it shows over, from its first (#1463, #1508).</summary>
/// <param name="Look">The look.</param>
/// <param name="FirstCharacter">The sprite character it shows from.</param>
/// <param name="CharacterCount">The characters it shows.</param>
public sealed record PlacedLook(SpriteArtworkSource Look, int FirstCharacter, int CharacterCount)
{
    /// <summary>The look.</summary>
    public SpriteArtworkSource Look { get; } = Ensure.NotNull(Look);

    /// <summary>The sprite character it shows from.</summary>
    public int FirstCharacter { get; } = Ensure.AtLeastZero(FirstCharacter);

    /// <summary>The characters it shows.</summary>
    public int CharacterCount { get; } = Ensure.BetweenInclusive(1, int.MaxValue, CharacterCount);

    /// <summary>Whether this look shows over every character <paramref name="other"/> shows over.</summary>
    public bool Covers(PlacedLook other) =>
        FirstCharacter <= Ensure.NotNull(other).FirstCharacter && other.FirstCharacter + other.CharacterCount <= FirstCharacter + CharacterCount;
}
