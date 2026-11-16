using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Core.Samus.Drawing.SamusAtmosphericEffectDefinitions;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Which splash or footstep effect Samus's movement and room produce.</summary>
internal static class SamusAtmosphericEffects
{
    /// <summary>Grounded postures produce the paired surface splash; other movement dives.</summary>
    internal static WaterSplashKind WaterSplashFor(SamusMovementType movementType)
    {
        return movementType switch
        {
            SamusMovementType.Standing or SamusMovementType.MorphBallGround or SamusMovementType.Crouching or
            SamusMovementType.TurningOnGround or SamusMovementType.PostureTransition or SamusMovementType.Moonwalking or
            SamusMovementType.SpringBallGround or SamusMovementType.RanIntoWall => WaterSplashKind.GroundedPair,
            SamusMovementType.Running or SamusMovementType.NormalJumping or SamusMovementType.SpinJumping or
                SamusMovementType.Falling or SamusMovementType.GlitchBall or
                SamusMovementType.MorphBallFalling or SamusMovementType.GlitchBallKnockback or
                SamusMovementType.Knockback or
                SamusMovementType.CompactAirborne or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling or SamusMovementType.WallJumping or
                SamusMovementType.Grappling or SamusMovementType.TurningWhileJumping or
                SamusMovementType.TurningWhileFalling or SamusMovementType.DamageBoost or
                SamusMovementType.DraygonHeld or SamusMovementType.Special => WaterSplashKind.Diving,
            _ => throw new InvalidDataException($"Water-splash movement type {movementType} is outside the 28 movement types."),
        };
    }

    /// <summary>
    /// Whether <paramref name="strideFrame"/>, a frame of the run's stride, is one of its two foot contacts
    /// (<see cref="SamusAnimationFrames.RunFirstFootContact"/>, <see cref="SamusAnimationFrames.RunSecondFootContact"/>).
    /// </summary>
    /// <exception cref="InvalidDataException">The frame is not one of the stride's pictures.</exception>
    internal static bool IsRunningFootContact(SamusAnimationTarget strideFrame)
    {
        if (strideFrame.Segment != SamusAnimationSegmentName.Run ||
            (uint)strideFrame.Frame >= (uint)SamusAnimationDelayDefinitions.RunningAnimation.Segment(SamusAnimationSegmentName.Run).Delays.Count)
            throw new InvalidDataException($"Running animation frame {strideFrame} is outside the run's stride.");
        return strideFrame == SamusAnimationFrames.RunFirstFootContact || strideFrame == SamusAnimationFrames.RunSecondFootContact;
    }

    /// <summary>A Crateria room's atmospheric effect on landings and footsteps, or null for a room the policy does not name.</summary>
    /// <exception cref="InvalidDataException">The room is outside Crateria.</exception>
    internal static CrateriaAtmosphericEffect? ForCrateriaRoom(RoomDefinition room)
    {
        if (Ensure.NotNull(room).Area != AreaId.Crateria)
            throw new InvalidDataException($"Room {room.Name} is outside Crateria's atmospheric policy.");
        return CrateriaRoomEffects.TryGetValue(room, out CrateriaAtmosphericEffect effect) ? effect : null;
    }

    /// <summary>Whether a landing in the Brinstar or Tourian room <paramref name="room"/> raises dust.</summary>
    /// <exception cref="InvalidDataException">The room is outside Brinstar and Tourian.</exception>
    internal static bool LandingRaisesDustInBrinstarOrTourian(RoomDefinition room)
    {
        if (Ensure.NotNull(room).Area is not (AreaId.Brinstar or AreaId.Tourian))
            throw new InvalidDataException($"Room {room.Name} is outside Brinstar's and Tourian's landing-dust policy.");
        return BrinstarAndTourianLandingDustRooms.Contains(room);
    }
}
