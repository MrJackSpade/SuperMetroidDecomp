using Shot = SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The poses Samus's turns, landings and wall stops choose by the direction she aims (#2323): each keeps her aim through
/// the change, and an aim a choice does not list is one the poses it starts from never have.
/// </summary>
internal static class SamusAimedPoseChoiceDefinitions
{
    /// <summary>
    /// The turn a normal jump takes, by the direction its pose aims. A straight-down aim takes the down-aimed turn away
    /// from the way it faces, as a diagonal-down aim does.
    /// </summary>
    internal static IReadOnlyDictionary<Shot, SamusPoseId> JumpingTurns { get; } = new Dictionary<Shot, SamusPoseId>
    {
        [Shot.UpFacingRight] = SamusPoseId.TurningRightToLeftJumpAimUpPose,
        [Shot.UpRight] = SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose,
        [Shot.Right] = SamusPoseId.TurningRightToLeftJumpPose,
        [Shot.DownRight] = SamusPoseId.TurningRightToLeftJumpAimDownPose,
        [Shot.DownFacingRight] = SamusPoseId.TurningRightToLeftJumpAimDownPose,
        [Shot.DownFacingLeft] = SamusPoseId.TurningLeftToRightJumpAimDownPose,
        [Shot.DownLeft] = SamusPoseId.TurningLeftToRightJumpAimDownPose,
        [Shot.Left] = SamusPoseId.TurningLeftToRightJumpPose,
        [Shot.UpLeft] = SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose,
        [Shot.UpFacingLeft] = SamusPoseId.TurningLeftToRightJumpAimUpPose,
    };

    /// <summary>
    /// The turn a fall takes, by the direction its pose aims. A straight-down aim takes the down-aimed turn away from the
    /// way it faces, as a diagonal-down aim does.
    /// </summary>
    internal static IReadOnlyDictionary<Shot, SamusPoseId> FallingTurns { get; } = new Dictionary<Shot, SamusPoseId>
    {
        [Shot.UpFacingRight] = SamusPoseId.TurningRightToLeftFallingAimUpPose,
        [Shot.UpRight] = SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose,
        [Shot.Right] = SamusPoseId.TurningRightToLeftFallingPose,
        [Shot.DownRight] = SamusPoseId.TurningRightToLeftFallingAimDownPose,
        [Shot.DownFacingRight] = SamusPoseId.TurningRightToLeftFallingAimDownPose,
        [Shot.DownFacingLeft] = SamusPoseId.TurningLeftToRightFallingAimDownPose,
        [Shot.DownLeft] = SamusPoseId.TurningLeftToRightFallingAimDownPose,
        [Shot.Left] = SamusPoseId.TurningLeftToRightFallingPose,
        [Shot.UpLeft] = SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose,
        [Shot.UpFacingLeft] = SamusPoseId.TurningLeftToRightFallingAimUpPose,
    };

    /// <summary>The turn a standing or running pose takes, by the direction it aims; no grounded pose aims straight down.</summary>
    internal static IReadOnlyDictionary<Shot, SamusPoseId> StandingTurns { get; } = new Dictionary<Shot, SamusPoseId>
    {
        [Shot.UpFacingRight] = SamusPoseId.TurningRightToLeftAimUpPose,
        [Shot.UpRight] = SamusPoseId.TurningRightToLeftAimDiagonalUpPose,
        [Shot.Right] = SamusPoseId.TurningRightToLeftPose,
        [Shot.DownRight] = SamusPoseId.TurningRightToLeftAimDiagonalDownPose,
        [Shot.DownLeft] = SamusPoseId.TurningLeftToRightAimDiagonalDownPose,
        [Shot.Left] = SamusPoseId.TurningLeftToRightPose,
        [Shot.UpLeft] = SamusPoseId.TurningLeftToRightAimDiagonalUpPose,
        [Shot.UpFacingLeft] = SamusPoseId.TurningLeftToRightAimUpPose,
    };

    /// <summary>The turn a crouching pose takes, by the direction it aims; no crouch aims straight down.</summary>
    internal static IReadOnlyDictionary<Shot, SamusPoseId> CrouchingTurns { get; } = new Dictionary<Shot, SamusPoseId>
    {
        [Shot.UpFacingRight] = SamusPoseId.TurningRightToLeftCrouchingAimUpPose,
        [Shot.UpRight] = SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose,
        [Shot.Right] = SamusPoseId.TurningRightToLeftCrouchingPose,
        [Shot.DownRight] = SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose,
        [Shot.DownLeft] = SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose,
        [Shot.Left] = SamusPoseId.TurningLeftToRightCrouchingPose,
        [Shot.UpLeft] = SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose,
        [Shot.UpFacingLeft] = SamusPoseId.TurningLeftToRightCrouchingAimUpPose,
    };

    /// <summary>
    /// The turning jump a moonwalk takes when it reverses with Jump held, by the direction it aims; a moonwalk aims only
    /// level or diagonally.
    /// </summary>
    internal static IReadOnlyDictionary<Shot, SamusPoseId> MoonwalkTurnJumps { get; } = new Dictionary<Shot, SamusPoseId>
    {
        [Shot.UpRight] = SamusPoseId.MoonwalkTurnJumpAimUpLeftPose,
        [Shot.Right] = SamusPoseId.MoonwalkTurnJumpLeftPose,
        [Shot.DownRight] = SamusPoseId.MoonwalkTurnJumpAimDownLeftPose,
        [Shot.DownLeft] = SamusPoseId.MoonwalkTurnJumpAimDownRightPose,
        [Shot.Left] = SamusPoseId.MoonwalkTurnJumpRightPose,
        [Shot.UpLeft] = SamusPoseId.MoonwalkTurnJumpAimUpRightPose,
    };

    /// <summary>
    /// The pose Samus stops in when she runs into a wall, by the direction she aims: aiming straight up she stands aiming
    /// up, and a straight-down aim stops as a level one.
    /// </summary>
    internal static IdentityTable<Shot, SamusPoseId> WallStops { get; } = new(new Dictionary<Shot, SamusPoseId>
    {
        [Shot.UpFacingRight] = SamusPoseId.StandingAimUpRightPose,
        [Shot.UpRight] = SamusPoseId.RanIntoWallAimUpRightPose,
        [Shot.Right] = SamusPoseId.RanIntoWallRightPose,
        [Shot.DownFacingRight] = SamusPoseId.RanIntoWallRightPose,
        [Shot.DownRight] = SamusPoseId.RanIntoWallAimDownRightPose,
        [Shot.DownFacingLeft] = SamusPoseId.RanIntoWallLeftPose,
        [Shot.Left] = SamusPoseId.RanIntoWallLeftPose,
        [Shot.DownLeft] = SamusPoseId.RanIntoWallAimDownLeftPose,
        [Shot.UpLeft] = SamusPoseId.RanIntoWallAimUpLeftPose,
        [Shot.UpFacingLeft] = SamusPoseId.StandingAimUpLeftPose,
    });

    /// <summary>
    /// The landing a jump or fall that can shoot takes, by the direction it aims; a straight-down aim lands through the
    /// compact landing, which needs the room's blocks.
    /// </summary>
    internal static IReadOnlyDictionary<Shot, SamusLandingChoice> Landings { get; } = new Dictionary<Shot, SamusLandingChoice>
    {
        [Shot.UpFacingRight] = new(SamusPoseId.LandingAimUpRightPose, SamusPoseId.LandingAimUpRightPose),
        [Shot.UpRight] = new(SamusPoseId.LandingAimDiagonalUpRightPose, SamusPoseId.LandingAimDiagonalUpRightPose),
        [Shot.Right] = new(SamusPoseId.NormalLandingRightPose, SamusPoseId.FiringLandingRightPose),
        [Shot.DownRight] = new(SamusPoseId.LandingAimDiagonalDownRightPose, SamusPoseId.LandingAimDiagonalDownRightPose),
        [Shot.DownLeft] = new(SamusPoseId.LandingAimDiagonalDownLeftPose, SamusPoseId.LandingAimDiagonalDownLeftPose),
        [Shot.Left] = new(SamusPoseId.NormalLandingLeftPose, SamusPoseId.FiringLandingLeftPose),
        [Shot.UpLeft] = new(SamusPoseId.LandingAimDiagonalUpLeftPose, SamusPoseId.LandingAimDiagonalUpLeftPose),
        [Shot.UpFacingLeft] = new(SamusPoseId.LandingAimUpLeftPose, SamusPoseId.LandingAimUpLeftPose),
    };
}
