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

    /// <summary>
    /// The turn a standing or running pose takes, by the direction it aims, over all ten: a straight-down aim turns as the
    /// diagonal-down aim facing its way does, as the cartridge's turning table gives it.
    /// </summary>
    internal static IdentityTable<Shot, SamusPoseId> StandingTurns { get; } = new(new Dictionary<Shot, SamusPoseId>
    {
        [Shot.UpFacingRight] = SamusPoseId.TurningRightToLeftAimUpPose,
        [Shot.UpRight] = SamusPoseId.TurningRightToLeftAimDiagonalUpPose,
        [Shot.Right] = SamusPoseId.TurningRightToLeftPose,
        [Shot.DownRight] = SamusPoseId.TurningRightToLeftAimDiagonalDownPose,
        [Shot.DownFacingRight] = SamusPoseId.TurningRightToLeftAimDiagonalDownPose,
        [Shot.DownFacingLeft] = SamusPoseId.TurningLeftToRightAimDiagonalDownPose,
        [Shot.DownLeft] = SamusPoseId.TurningLeftToRightAimDiagonalDownPose,
        [Shot.Left] = SamusPoseId.TurningLeftToRightPose,
        [Shot.UpLeft] = SamusPoseId.TurningLeftToRightAimDiagonalUpPose,
        [Shot.UpFacingLeft] = SamusPoseId.TurningLeftToRightAimUpPose,
    });

    /// <summary>
    /// The turn a crouching pose takes, by the direction it aims, over all ten: a straight-down aim turns as the
    /// diagonal-down aim facing its way does, as the cartridge's turning table gives it.
    /// </summary>
    internal static IdentityTable<Shot, SamusPoseId> CrouchingTurns { get; } = new(new Dictionary<Shot, SamusPoseId>
    {
        [Shot.UpFacingRight] = SamusPoseId.TurningRightToLeftCrouchingAimUpPose,
        [Shot.UpRight] = SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose,
        [Shot.Right] = SamusPoseId.TurningRightToLeftCrouchingPose,
        [Shot.DownRight] = SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose,
        [Shot.DownFacingRight] = SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose,
        [Shot.DownFacingLeft] = SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose,
        [Shot.DownLeft] = SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose,
        [Shot.Left] = SamusPoseId.TurningLeftToRightCrouchingPose,
        [Shot.UpLeft] = SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose,
        [Shot.UpFacingLeft] = SamusPoseId.TurningLeftToRightCrouchingAimUpPose,
    });

    /// <summary>
    /// The turning jump a moonwalk takes when it reverses with Jump held, by the direction it aims; a moonwalk aims only
    /// level or diagonally.
    /// </summary>
    internal static IReadOnlyDictionary<Shot, SamusPoseId> MoonwalkTurnToSpinJumps { get; } = new Dictionary<Shot, SamusPoseId>
    {
        [Shot.UpRight] = SamusPoseId.MoonwalkTurnToSpinJumpAimUpLeftPose,
        [Shot.Right] = SamusPoseId.MoonwalkTurnToSpinJumpLeftPose,
        [Shot.DownRight] = SamusPoseId.MoonwalkTurnToSpinJumpAimDownLeftPose,
        [Shot.DownLeft] = SamusPoseId.MoonwalkTurnToSpinJumpAimDownRightPose,
        [Shot.Left] = SamusPoseId.MoonwalkTurnToSpinJumpRightPose,
        [Shot.UpLeft] = SamusPoseId.MoonwalkTurnToSpinJumpAimUpRightPose,
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
    /// The landing a jump or fall that can shoot takes, by the direction it aims, over all ten as the cartridge's landing
    /// table gives them: a straight-down aim lands in the normal landing facing its way, Shoot held or not. Compact
    /// straight-down poses land through the compact landing, which needs the room's blocks, and takes the same poses.
    /// </summary>
    internal static IdentityTable<Shot, SamusLandingChoice> Landings { get; } = new(new Dictionary<Shot, SamusLandingChoice>
    {
        [Shot.UpFacingRight] = new(SamusPoseId.LandingAimUpRightPose, SamusPoseId.LandingAimUpRightPose),
        [Shot.UpRight] = new(SamusPoseId.LandingAimDiagonalUpRightPose, SamusPoseId.LandingAimDiagonalUpRightPose),
        [Shot.Right] = new(SamusPoseId.NormalLandingRightPose, SamusPoseId.FiringLandingRightPose),
        [Shot.DownRight] = new(SamusPoseId.LandingAimDiagonalDownRightPose, SamusPoseId.LandingAimDiagonalDownRightPose),
        [Shot.DownFacingRight] = new(SamusPoseId.NormalLandingRightPose, SamusPoseId.NormalLandingRightPose),
        [Shot.DownFacingLeft] = new(SamusPoseId.NormalLandingLeftPose, SamusPoseId.NormalLandingLeftPose),
        [Shot.DownLeft] = new(SamusPoseId.LandingAimDiagonalDownLeftPose, SamusPoseId.LandingAimDiagonalDownLeftPose),
        [Shot.Left] = new(SamusPoseId.NormalLandingLeftPose, SamusPoseId.FiringLandingLeftPose),
        [Shot.UpLeft] = new(SamusPoseId.LandingAimDiagonalUpLeftPose, SamusPoseId.LandingAimDiagonalUpLeftPose),
        [Shot.UpFacingLeft] = new(SamusPoseId.LandingAimUpLeftPose, SamusPoseId.LandingAimUpLeftPose),
    });
}
