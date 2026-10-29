namespace SuperMetroid.Core.Assets;

/// <summary>
/// The sprites' artwork as a background shows it (#1463, #1466): which looks the sprite looks shown so far leave on the
/// object page, which the Ceres Ridley arena's backgrounds point at.
/// </summary>
public interface ISpriteObjectPage
{
    /// <summary>Whether <paramref name="look"/> shows on the object page: it was shown and no later look has hidden it.</summary>
    /// <exception cref="InvalidOperationException">No enemy sheets are installed.</exception>
    bool Shows(SpriteArtworkSource look);
}
