using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>One placement of a charge or grapple flare: the movement mode and where the pose places the flare.</summary>
/// <param name="Running">Whether the running placements hold it rather than the standing ones.</param>
/// <param name="Aim">The muzzle, or while turning the turning placement, the flare is placed at.</param>
internal readonly record struct ChargeFlarePlacement(bool Running, SamusFlareAim Aim);
