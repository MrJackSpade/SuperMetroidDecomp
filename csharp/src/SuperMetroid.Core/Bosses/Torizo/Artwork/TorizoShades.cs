using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// The shades the import pre-colours the Torizos' sheets in (#1512): the orb, flame and statue-shading colours both load,
/// each settled colouring of a Torizo's body and belly, and each counter value of a fade from one to another.
/// </summary>
public static class TorizoShades
{
    /// <summary>The orb, flame and statue-shading colours both Torizos load.</summary>
    public static PictureShade Shared { get; } = new("torizo-shared", 0);

    /// <summary>A Torizo's body and belly showing <paramref name="look"/>.</summary>
    public static PictureShade Body(TorizoLook look) => new($"torizo-{look.Key}", 0);

    /// <summary>
    /// A Torizo's body and belly after the step of counter value <paramref name="value"/> of a fade from
    /// <paramref name="from"/> toward <paramref name="to"/>.
    /// </summary>
    public static PictureShade Fading(TorizoLook from, TorizoLook to, int value) => new($"torizo-{from.Key}-to-{to.Key}", value);
}
