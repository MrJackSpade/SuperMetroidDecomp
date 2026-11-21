using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The poses of Draygon's arms, limp arm, jaw and eye facing one way (#2557): his arms' sway, swoop and grab, his dying
/// arm, his goop spit and roar and his eye's blink, dying and death are written once over a facing's poses, as his tail's
/// lashes are over <see cref="DraygonTailPoses"/>.
/// </summary>
/// <param name="Facing">The way Draygon faces.</param>
/// <param name="ArmsForward">His arms forward picture.</param>
/// <param name="ArmsLowering">His arms lowering picture.</param>
/// <param name="ArmsDown">His arms down picture.</param>
/// <param name="ArmsDownStretched">His arms down stretched picture.</param>
/// <param name="ArmsRaising">His arms raising picture.</param>
/// <param name="ArmsReaching">His arms reaching picture.</param>
/// <param name="ArmsSpreadWide">His arms spread wide picture.</param>
/// <param name="ArmsClosing">His arms closing picture.</param>
/// <param name="ArmsGathering">His arms gathering picture.</param>
/// <param name="ArmsTucked">His arms tucked picture.</param>
/// <param name="LimpArmClawRaised">His limp arm claw raised picture.</param>
/// <param name="LimpArmClawDropping">His limp arm claw dropping picture.</param>
/// <param name="LimpArmClawDropped">His limp arm claw dropped picture.</param>
/// <param name="LimpArmClawCurled">His limp arm claw curled picture.</param>
/// <param name="BodyJawShut">His body jaw shut picture.</param>
/// <param name="BodyJawParting">His body jaw parting picture.</param>
/// <param name="BodyJawOpen">His body jaw open picture.</param>
/// <param name="BodyJawWide">His body jaw wide picture.</param>
/// <param name="EyeShut">His eye shut picture.</param>
/// <param name="EyeOpening">His eye opening picture.</param>
/// <param name="EyeHalfOpen">His eye half open picture.</param>
/// <param name="EyeOpen">His eye open picture.</param>
/// <param name="EyeNarrowing">His eye narrowing picture.</param>
/// <param name="EyeClosing">His eye closing picture.</param>
/// <param name="EyeBloodshot">His eye bloodshot picture.</param>
/// <param name="EyeWide">His eye wide picture.</param>
internal sealed record DraygonFacingPoses(
    HorizontalDirection Facing,
    F ArmsForward,
    F ArmsLowering,
    F ArmsDown,
    F ArmsDownStretched,
    F ArmsRaising,
    F ArmsReaching,
    F ArmsSpreadWide,
    F ArmsClosing,
    F ArmsGathering,
    F ArmsTucked,
    F LimpArmClawRaised,
    F LimpArmClawDropping,
    F LimpArmClawDropped,
    F LimpArmClawCurled,
    F BodyJawShut,
    F BodyJawParting,
    F BodyJawOpen,
    F BodyJawWide,
    F EyeShut,
    F EyeOpening,
    F EyeHalfOpen,
    F EyeOpen,
    F EyeNarrowing,
    F EyeClosing,
    F EyeBloodshot,
    F EyeWide)
{
    /// <summary>The poses facing left.</summary>
    internal static DraygonFacingPoses Left { get; } = new(
        HorizontalDirection.Left,
        F.DraygonArmsFacingLeftForward,
        F.DraygonArmsFacingLeftLowering,
        F.DraygonArmsFacingLeftDown,
        F.DraygonArmsFacingLeftDownStretched,
        F.DraygonArmsFacingLeftRaising,
        F.DraygonArmsFacingLeftReaching,
        F.DraygonArmsFacingLeftSpreadWide,
        F.DraygonArmsFacingLeftClosing,
        F.DraygonArmsFacingLeftGathering,
        F.DraygonArmsFacingLeftTucked,
        F.DraygonLimpArmFacingLeftClawRaised,
        F.DraygonLimpArmFacingLeftClawDropping,
        F.DraygonLimpArmFacingLeftClawDropped,
        F.DraygonLimpArmFacingLeftClawCurled,
        F.DraygonBodyFacingLeftJawShut,
        F.DraygonBodyFacingLeftJawParting,
        F.DraygonBodyFacingLeftJawOpen,
        F.DraygonBodyFacingLeftJawWide,
        F.DraygonEyeFacingLeftShut,
        F.DraygonEyeFacingLeftOpening,
        F.DraygonEyeFacingLeftHalfOpen,
        F.DraygonEyeFacingLeftOpen,
        F.DraygonEyeFacingLeftNarrowing,
        F.DraygonEyeFacingLeftClosing,
        F.DraygonEyeFacingLeftBloodshot,
        F.DraygonEyeFacingLeftWide);

    /// <summary>The poses facing right.</summary>
    internal static DraygonFacingPoses Right { get; } = new(
        HorizontalDirection.Right,
        F.DraygonArmsFacingRightForward,
        F.DraygonArmsFacingRightLowering,
        F.DraygonArmsFacingRightDown,
        F.DraygonArmsFacingRightDownStretched,
        F.DraygonArmsFacingRightRaising,
        F.DraygonArmsFacingRightReaching,
        F.DraygonArmsFacingRightSpreadWide,
        F.DraygonArmsFacingRightClosing,
        F.DraygonArmsFacingRightGathering,
        F.DraygonArmsFacingRightTucked,
        F.DraygonLimpArmFacingRightClawRaised,
        F.DraygonLimpArmFacingRightClawDropping,
        F.DraygonLimpArmFacingRightClawDropped,
        F.DraygonLimpArmFacingRightClawCurled,
        F.DraygonBodyFacingRightJawShut,
        F.DraygonBodyFacingRightJawParting,
        F.DraygonBodyFacingRightJawOpen,
        F.DraygonBodyFacingRightJawWide,
        F.DraygonEyeFacingRightShut,
        F.DraygonEyeFacingRightOpening,
        F.DraygonEyeFacingRightHalfOpen,
        F.DraygonEyeFacingRightOpen,
        F.DraygonEyeFacingRightNarrowing,
        F.DraygonEyeFacingRightClosing,
        F.DraygonEyeFacingRightBloodshot,
        F.DraygonEyeFacingRightWide);
}
