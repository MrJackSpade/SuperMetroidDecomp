using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>The ways the title demonstrations stage Samus (#2500).</summary>
public static partial class AttractDemoSamusSetups
{
    /// <summary>Facing forward, as she stands on arriving at the Landing Site.</summary>
    public static AttractDemoSamusSetup.ForwardFacing LandingSite { get; } = new();

    /// <summary>Standing facing right.</summary>
    public static AttractDemoSamusSetup.Posed StandingRight { get; } = new(SamusPoseId.FacingRightNormalPose);

    /// <summary>Standing facing left.</summary>
    public static AttractDemoSamusSetup.Posed StandingLeft { get; } = new(SamusPoseId.FacingLeftNormalPose);

    /// <summary>Standing facing left, nearly out of energy, for the Crystal Flash's demonstration; her capacity is unchanged.</summary>
    public static AttractDemoSamusSetup.Posed LowHealthLeft { get; } =
        new(SamusPoseId.FacingLeftNormalPose, Energy: AttractDemoDefinitions.LowHealthEnergy);

    /// <summary>
    /// Rolling left on the ground in the morph ball, for the pseudo Screw Attack's demonstration: the rolling pose, not the
    /// morph ball at rest (#2500).
    /// </summary>
    public static AttractDemoSamusSetup.Posed MorphLeft { get; } = new(SamusPoseId.MorphBallMovingLeftPose);

    /// <summary>Falling facing left, for the Dachora's demonstration.</summary>
    public static AttractDemoSamusSetup.Posed FallingLeft { get; } = new(SamusPoseId.FallingLeftPose);

    /// <summary>Already launched into a shinespark diagonally up and right.</summary>
    public static AttractDemoSamusSetup.Shinespark DiagonalShinespark { get; } = new(SamusPoseId.ShinesparkDiagonalRightPose);

    /// <summary>Already launched into a shinespark horizontally to the left.</summary>
    public static AttractDemoSamusSetup.Shinespark HorizontalShinespark { get; } = new(SamusPoseId.ShinesparkHorizontalLeftPose);
}
