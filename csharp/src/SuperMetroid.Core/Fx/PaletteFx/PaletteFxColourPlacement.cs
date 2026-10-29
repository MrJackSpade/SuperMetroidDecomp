using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Which owner's colours a palette-FX colour group is, and the owner's colour its first written colour is (#1464): a
/// colour of a background or sprite set, or of Samus's suit or beams. Only
/// <see cref="PaletteFxColourPlacementDefinitions"/> names placements.
/// </summary>
internal abstract record PaletteFxColourPlacement
{
    /// <summary>The owner's colours from the group's first written colour to the owner's last.</summary>
    internal abstract ColourRun From(IPaletteFxColourOwners owners);
}
