using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The standing poses of a Dachora facing one way: its idle, pecks and blink each play these, so each is written once over a
/// facing's poses, as its runs are written once over a <see cref="DachoraStride"/>.
/// </summary>
/// <param name="HeadUp">Standing with its head up.</param>
/// <param name="NeckStretched">Standing with its neck stretched.</param>
/// <param name="HeadLowered">Its head lowered towards the ground.</param>
/// <param name="HeadToGround">Its head at the ground, pecking.</param>
/// <param name="HeadRaising">Its head raising from a peck.</param>
/// <param name="EyeHalfShut">Standing with its eye half shut.</param>
/// <param name="EyeShut">Standing with its eye shut.</param>
internal sealed record DachoraStance(F HeadUp, F NeckStretched, F HeadLowered, F HeadToGround, F HeadRaising, F EyeHalfShut, F EyeShut)
{
    /// <summary>Standing facing left.</summary>
    internal static DachoraStance Left { get; } = new(
        F.DachoraStandingFacingLeftHeadUp, F.DachoraStandingFacingLeftNeckStretched, F.DachoraStandingFacingLeftHeadLowered,
        F.DachoraStandingFacingLeftHeadToGround, F.DachoraStandingFacingLeftHeadRaising, F.DachoraStandingFacingLeftEyeHalfShut,
        F.DachoraStandingFacingLeftEyeShut);

    /// <summary>Standing facing right.</summary>
    internal static DachoraStance Right { get; } = new(
        F.DachoraStandingFacingRightHeadUp, F.DachoraStandingFacingRightNeckStretched, F.DachoraStandingFacingRightHeadLowered,
        F.DachoraStandingFacingRightHeadToGround, F.DachoraStandingFacingRightHeadRaising, F.DachoraStandingFacingRightEyeHalfShut,
        F.DachoraStandingFacingRightEyeShut);
}
