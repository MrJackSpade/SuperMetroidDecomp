using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The installed characters one Torizo artwork swap shows.</summary>
/// <param name="Entry">The swap's <see cref="TorizoArtworkSwapDefinitions"/> entry.</param>
internal sealed record TorizoArtworkSwapCharacters(int Entry) : SpriteArtworkSource
{
    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"Torizo artwork swap {Entry}";
}
