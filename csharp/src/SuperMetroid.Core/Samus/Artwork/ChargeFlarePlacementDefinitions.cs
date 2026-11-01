using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Visual flare placement resource contract: one placement per named aiming direction and movement mode.</summary>
public static class ChargeFlarePlacementDefinitions
{
    /// <summary>
    /// Required revision of the shared beam/Grapple placement schema. Revision two holds the ten named directions of each
    /// movement mode; revision one also held the selections past them, which no flare draw reaches (#1500).
    /// </summary>
    public const int Version = 2;

    /// <summary>
    /// The directions each movement mode places the flare for: the ten named aiming directions. The beam flare draws only
    /// for a pose whose shot direction is one of them, and the Grapple flare takes its typed firing direction, so no
    /// placement past them is read (#1500).
    /// </summary>
    public const int DirectionCount = SamusFiringTableDefinitions.DirectionCount;
}
