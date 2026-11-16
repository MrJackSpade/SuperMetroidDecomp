using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>
/// The shades the import pre-colours the Torizos' sheets in (#1512, #2236): the orb, flame and statue-shading colours both
/// load, each settled colouring of a Torizo's body and belly, and the glowing belly colours in each colouring a fade runs
/// toward; a fade between them is a typed fade.
/// </summary>
public static class TorizoShades
{
    /// <summary>The orb, flame and statue-shading colours both Torizos load.</summary>
    public static PictureShade Shared { get; } = new("torizo-shared", 0);

    /// <summary>A Torizo's body and belly showing <paramref name="look"/>.</summary>
    public static PictureShade Body(TorizoLook look) => new($"torizo-{look.Key}", 0);

    /// <summary>
    /// The colours his belly's glow recolours as <paramref name="look"/> colours them (#2236), which a fade over the glow
    /// runs toward.
    /// </summary>
    public static PictureShade GlowingBelly(TorizoLook look) => new($"torizo-glowing-belly-{look.Key}", 0);
}
