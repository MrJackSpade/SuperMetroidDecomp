using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Corpses.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The shades the import pre-colours the Shitroid's encounter in (#1512): the colours its room's entry fades the victim
/// Sidehopper and the Shitroid to, and each phase of the Shitroid's membrane pulse.
/// </summary>
public static class ShitroidShades
{
    /// <summary>The Shitroid's membrane at pulse phase <paramref name="pulse"/> (#2259).</summary>
    public static PictureShade Membrane(ShitroidMembranePulse pulse) => new("shitroid-membrane", pulse.Step);

    /// <summary>A draw in the colours the encounter's entry loads.</summary>
    public static PictureShading InTargetColours => TourianCorpseShades.InShitroidTargetColours;
}
