namespace SuperMetroid.Core.Assets;

/// <summary>
/// The sprites' artwork as a background shows it (#1463): what the sprite looks shown so far leave on the object page,
/// which the Ceres Ridley arena's backgrounds point at.
/// </summary>
public interface ISpriteObjectPage
{
    /// <summary>The object page the sprite looks shown so far leave.</summary>
    /// <exception cref="InvalidOperationException">No enemy sheets are installed.</exception>
    IBgCharacterArtwork ObjectPage();
}
