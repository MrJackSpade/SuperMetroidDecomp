using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Visual flare placement resource contract: one placement per named aiming direction and movement mode, and the beam
/// flare's placements while Samus turns.
/// </summary>
public static class ChargeFlarePlacementDefinitions
{
    /// <summary>The ten named aiming directions each movement mode places the flare for.</summary>
    public const int DirectionCount = 10;

    /// <summary>The beam flare's placements while Samus turns, one for each aim a turning pose holds.</summary>
    public const int TurningPlacementCount = 3;

    /// <summary>
    /// The selections the standing beam flare is placed by: the ten named aiming directions, then the turning placements.
    /// A turning pose selects the turning placement of the aim it holds. No pose selects past them, and no running pose
    /// selects past the named directions, so those selections are rejected (#1500).
    /// </summary>
    public const int StandingDirectionCount = DirectionCount + TurningPlacementCount;

    /// <summary>The turning placements in selection order after the named aiming directions.</summary>
    internal static readonly SamusTurningAim[] TurningPlacements = [SamusTurningAim.Up, SamusTurningAim.Neutral, SamusTurningAim.Down];
}
