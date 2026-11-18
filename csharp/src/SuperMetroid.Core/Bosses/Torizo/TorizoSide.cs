using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo's poses facing one way (#2411): seated and rising, jumping, its stances with either foot forward and its steps on
/// either leg. Each faced animation is written once over a side, as Ridley's are (#2175); the displacements, walking moves
/// and placements a side's animations carry are mirrored by the way it faces.
/// </summary>
/// <param name="Facing">The way the Torizo faces.</param>
/// <param name="Seated">Seated.</param>
/// <param name="RisingFromSeat">Rising from its seat.</param>
/// <param name="Crouched">Crouched.</param>
/// <param name="HalfRisen">Half risen.</param>
/// <param name="NearlyUpright">Nearly upright.</param>
/// <param name="UprightStepping">Upright, stepping.</param>
/// <param name="UprightWideStance">Upright in its wide stance.</param>
/// <param name="JumpCrouch">Crouched to jump.</param>
/// <param name="Springing">Springing off the ground, and the pose a fall rests in.</param>
/// <param name="Leaping">Leaping, the pose a jump holds while it rises.</param>
/// <param name="NearFootForward">Standing with the foot on the side it faces forward: its left facing left.</param>
/// <param name="FarFootForward">Standing with the foot away from the side it faces forward: its right facing left.</param>
/// <param name="NearLegStep">A step of the leg on the side it faces, standing on the other foot.</param>
/// <param name="FarLegStep">A step of the leg away from the side it faces, standing on the foot on that side.</param>
internal sealed record TorizoSide(
    HorizontalDirection Facing, F Seated, F RisingFromSeat, F Crouched, F HalfRisen, F NearlyUpright, F UprightStepping, F UprightWideStance,
    F JumpCrouch, F Springing, F Leaping, TorizoStance NearFootForward, TorizoStance FarFootForward, TorizoStride NearLegStep, TorizoStride FarLegStep)
{
    /// <summary>The poses facing left.</summary>
    internal static TorizoSide Left { get; } = new(
        HorizontalDirection.Left, F.TorizoFacingLeftSeated, F.TorizoFacingLeftRisingFromSeat, F.TorizoFacingLeftCrouched, F.TorizoFacingLeftHalfRisen,
        F.TorizoFacingLeftNearlyUpright, F.TorizoFacingLeftUprightStepping, F.TorizoFacingLeftUprightWideStance,
        F.TorizoFacingLeftJumpCrouch, F.TorizoFacingLeftSpringing, F.TorizoFacingLeftLeaping,
        NearFootForward: new(
            F.TorizoFacingLeftLeftFootForwardSpewingHeadLowered, F.TorizoFacingLeftLeftFootForwardSpewingHeadRaised,
            F.TorizoFacingLeftLeftFootForwardSpewingMouthOpening, F.TorizoFacingLeftLeftFootForwardSpewingArmThrust,
            F.TorizoFacingLeftLeftFootForwardSpewingArmRaised, F.TorizoFacingLeftLeftFootForwardSpewingArmDrawnBack,
            NearArm: new(F.TorizoFacingLeftLeftFootForwardNearArmDown, F.TorizoFacingLeftLeftFootForwardNearArmLifting,
                F.TorizoFacingLeftLeftFootForwardNearArmLevel, F.TorizoFacingLeftLeftFootForwardNearArmRaised, F.TorizoFacingLeftLeftFootForwardNearArmOverhead),
            FarArm: new(F.TorizoFacingLeftLeftFootForwardFarArmDown, F.TorizoFacingLeftLeftFootForwardFarArmLifting,
                F.TorizoFacingLeftLeftFootForwardFarArmLevel, F.TorizoFacingLeftLeftFootForwardFarArmRaised, F.TorizoFacingLeftLeftFootForwardFarArmOverhead)),
        FarFootForward: new(
            F.TorizoFacingLeftRightFootForwardSpewingHeadLowered, F.TorizoFacingLeftRightFootForwardSpewingHeadRaised,
            F.TorizoFacingLeftRightFootForwardSpewingMouthOpening, F.TorizoFacingLeftRightFootForwardSpewingArmThrust,
            F.TorizoFacingLeftRightFootForwardSpewingArmRaised, F.TorizoFacingLeftRightFootForwardSpewingArmDrawnBack,
            NearArm: new(F.TorizoFacingLeftRightFootForwardNearArmDown, F.TorizoFacingLeftRightFootForwardNearArmLifting,
                F.TorizoFacingLeftRightFootForwardNearArmLevel, F.TorizoFacingLeftRightFootForwardNearArmRaised, F.TorizoFacingLeftRightFootForwardNearArmOverhead),
            FarArm: new(F.TorizoFacingLeftRightFootForwardFarArmDown, F.TorizoFacingLeftRightFootForwardFarArmLifting,
                F.TorizoFacingLeftRightFootForwardFarArmLevel, F.TorizoFacingLeftRightFootForwardFarArmRaised, F.TorizoFacingLeftRightFootForwardFarArmOverhead)),
        NearLegStep: new(HorizontalDirection.Right, F.TorizoFacingLeftWalkingLeftLegLifting, F.TorizoFacingLeftWalkingLeftLegSwinging,
            F.TorizoFacingLeftWalkingLeftLegReaching, F.TorizoFacingLeftWalkingLeftLegPlanting, F.TorizoFacingLeftWalkingLeftLegPlanted,
            TorizoStrideDefinitions.NearLeg),
        FarLegStep: new(HorizontalDirection.Left, F.TorizoFacingLeftWalkingRightLegLifting, F.TorizoFacingLeftWalkingRightLegSwinging,
            F.TorizoFacingLeftWalkingRightLegReaching, F.TorizoFacingLeftWalkingRightLegPlanting, F.TorizoFacingLeftWalkingRightLegPlanted,
            TorizoStrideDefinitions.FarLeg));

    /// <summary>The poses facing right.</summary>
    internal static TorizoSide Right { get; } = new(
        HorizontalDirection.Right, F.TorizoFacingRightSeated, F.TorizoFacingRightRisingFromSeat, F.TorizoFacingRightCrouched, F.TorizoFacingRightHalfRisen,
        F.TorizoFacingRightNearlyUpright, F.TorizoFacingRightUprightStepping, F.TorizoFacingRightUprightWideStance,
        F.TorizoFacingRightJumpCrouch, F.TorizoFacingRightSpringing, F.TorizoFacingRightLeaping,
        NearFootForward: new(
            F.TorizoFacingRightRightFootForwardSpewingHeadLowered, F.TorizoFacingRightRightFootForwardSpewingHeadRaised,
            F.TorizoFacingRightRightFootForwardSpewingMouthOpening, F.TorizoFacingRightRightFootForwardSpewingArmThrust,
            F.TorizoFacingRightRightFootForwardSpewingArmRaised, F.TorizoFacingRightRightFootForwardSpewingArmDrawnBack,
            NearArm: new(F.TorizoFacingRightRightFootForwardNearArmDown, F.TorizoFacingRightRightFootForwardNearArmLifting,
                F.TorizoFacingRightRightFootForwardNearArmLevel, F.TorizoFacingRightRightFootForwardNearArmRaised, F.TorizoFacingRightRightFootForwardNearArmOverhead),
            FarArm: new(F.TorizoFacingRightRightFootForwardFarArmDown, F.TorizoFacingRightRightFootForwardFarArmLifting,
                F.TorizoFacingRightRightFootForwardFarArmLevel, F.TorizoFacingRightRightFootForwardFarArmRaised, F.TorizoFacingRightRightFootForwardFarArmOverhead)),
        FarFootForward: new(
            F.TorizoFacingRightLeftFootForwardSpewingHeadLowered, F.TorizoFacingRightLeftFootForwardSpewingHeadRaised,
            F.TorizoFacingRightLeftFootForwardSpewingMouthOpening, F.TorizoFacingRightLeftFootForwardSpewingArmThrust,
            F.TorizoFacingRightLeftFootForwardSpewingArmRaised, F.TorizoFacingRightLeftFootForwardSpewingArmDrawnBack,
            NearArm: new(F.TorizoFacingRightLeftFootForwardNearArmDown, F.TorizoFacingRightLeftFootForwardNearArmLifting,
                F.TorizoFacingRightLeftFootForwardNearArmLevel, F.TorizoFacingRightLeftFootForwardNearArmRaised, F.TorizoFacingRightLeftFootForwardNearArmOverhead),
            FarArm: new(F.TorizoFacingRightLeftFootForwardFarArmDown, F.TorizoFacingRightLeftFootForwardFarArmLifting,
                F.TorizoFacingRightLeftFootForwardFarArmLevel, F.TorizoFacingRightLeftFootForwardFarArmRaised, F.TorizoFacingRightLeftFootForwardFarArmOverhead)),
        NearLegStep: new(HorizontalDirection.Left, F.TorizoFacingRightWalkingRightLegLifting, F.TorizoFacingRightWalkingRightLegSwinging,
            F.TorizoFacingRightWalkingRightLegReaching, F.TorizoFacingRightWalkingRightLegPlanting, F.TorizoFacingRightWalkingRightLegPlanted,
            TorizoStrideDefinitions.NearLeg),
        FarLegStep: new(HorizontalDirection.Right, F.TorizoFacingRightWalkingLeftLegLifting, F.TorizoFacingRightWalkingLeftLegSwinging,
            F.TorizoFacingRightWalkingLeftLegReaching, F.TorizoFacingRightWalkingLeftLegPlanting, F.TorizoFacingRightWalkingLeftLegPlanted,
            TorizoStrideDefinitions.FarLeg));

    /// <summary><paramref name="pixels"/> forward, the way this side faces: negative to the left.</summary>
    internal int Forward(int pixels) => Facing == HorizontalDirection.Right ? pixels : -pixels;

    /// <summary>A Torizo facing right's <paramref name="facingRight"/> move, mirrored for a side facing left.</summary>
    internal Offset16<RoomPixels> Forward(Offset16<RoomPixels> facingRight) =>
        Facing == HorizontalDirection.Right ? facingRight : facingRight.MirroredX();
}
