namespace SuperMetroid.Core.Assets;

/// <summary>
/// Which sprite looks replace which (#1463): the installed artwork knows where each look lands, so it alone says whether
/// showing one leaves nothing of another visible.
/// </summary>
public interface ISpriteArtworkOverlap
{
    /// <summary>Whether showing <paramref name="later"/> leaves nothing <paramref name="earlier"/> showed visible.</summary>
    /// <exception cref="InvalidDataException">The installed artwork does not know where one of the looks lands.</exception>
    bool Hides(SpriteArtworkSource later, SpriteArtworkSource earlier);
}
