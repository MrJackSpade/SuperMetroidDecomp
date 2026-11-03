using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The shades the Chozo statues' and the n00b tube's pictures are pre-coloured in (#1512): each statue's own colours,
/// which its initializer installs, and the glass tube's crack colours, which the tube room's crack initializer installs.
/// The import colours them from the installed Chozo-and-tube colours.
/// </summary>
internal static class ChozoAndTubeShades
{
    /// <summary>The Wrecked Ship statue's colours.</summary>
    internal static PictureShade WreckedShipStatue { get; } = new("chozo-statue-wrecked-ship", 0);

    /// <summary>The Lower Norfair statue's colours.</summary>
    internal static PictureShade LowerNorfairStatue { get; } = new("chozo-statue-lower-norfair", 0);

    /// <summary>The glass tube's crack colours, which its cracks, shards and released air show.</summary>
    internal static PictureShade TubeCracks { get; } = new("noob-tube-cracks", 0);
}
