using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The poses of one of the Wrecked Ship statue's legs as it steps with Samus in its hand (#2477): lifting, striding, planting
/// and planted. Its walk and its last strides are written once over its legs, as a Work Robot's strides are written once over
/// its feet (#2451).
/// </summary>
/// <param name="Lifting">The leg lifting.</param>
/// <param name="Striding">The leg striding forward.</param>
/// <param name="Planting">The leg planting.</param>
/// <param name="Planted">The leg planted.</param>
internal sealed partial record ChozoStatueLeg(F Lifting, F Striding, F Planting, F Planted)
{
    /// <summary>The Wrecked Ship statue's near leg.</summary>
    internal static ChozoStatueLeg WreckedShipNear { get; } = new(
        F.ChozoStatueWreckedShipWalkingNearLegLifting, F.ChozoStatueWreckedShipWalkingNearLegStriding,
        F.ChozoStatueWreckedShipWalkingNearLegPlanting, F.ChozoStatueWreckedShipWalkingNearLegPlanted);

    /// <summary>The Wrecked Ship statue's far leg.</summary>
    internal static ChozoStatueLeg WreckedShipFar { get; } = new(
        F.ChozoStatueWreckedShipWalkingFarLegLifting, F.ChozoStatueWreckedShipWalkingFarLegStriding,
        F.ChozoStatueWreckedShipWalkingFarLegPlanting, F.ChozoStatueWreckedShipWalkingFarLegPlanted);
}
