using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Movement;
using static SuperMetroid.Core.Samus.Drawing.SamusAtmosphericEffectDefinitions;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Which splash or footstep effect Samus's movement and room produce.</summary>
internal static class SamusAtmosphericEffects
{
    /// <summary>$90:81A4: grounded postures produce the paired surface splash; other movement dives.</summary>
    internal static WaterSplashKind WaterSplashFor(SamusMovementType movementType)
    {
        return movementType switch
        {
            SamusMovementType.Standing or SamusMovementType.MorphBallGround or SamusMovementType.Crouching or
            SamusMovementType.TurningOnGround or SamusMovementType.PostureTransition or SamusMovementType.Moonwalking or
            SamusMovementType.SpringBallGround or SamusMovementType.RanIntoWall => WaterSplashKind.GroundedPair,
            SamusMovementType.Running or SamusMovementType.NormalJumping or SamusMovementType.SpinJumping or
                SamusMovementType.Falling or SamusMovementType.UnusedGlitchBall or
                SamusMovementType.MorphBallFalling or SamusMovementType.UnusedGlitchBallAlternate or
                SamusMovementType.Knockback or SamusMovementType.Unused0B or SamusMovementType.Unused0C or
                SamusMovementType.Unused0D or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling or SamusMovementType.WallJumping or
                SamusMovementType.Grappling or SamusMovementType.TurningWhileJumping or
                SamusMovementType.TurningWhileFalling or SamusMovementType.DamageBoost or
                SamusMovementType.DraygonHeld or SamusMovementType.Special => WaterSplashKind.Diving,
            _ => throw new InvalidDataException($"Water-splash movement type {movementType} is outside 28 retail selectors."),
        };
    }

    /// <summary>$90:A424: one contact at phase two of each five-frame running step.</summary>
    internal static bool IsRunningFootContact(int animationFrame)
    {
        if (animationFrame >= 10)
            throw new InvalidDataException($"Running animation frame {animationFrame} is outside ten foot-contact selectors.");
        return animationFrame % 5 == 2;
    }

    /// <summary>$90:EDC9 and $91:F0F3: room-specific landing/rain and wet-floor policies.</summary>
    internal static CrateriaAtmosphericEffectFlags ForCrateriaRoom(RoomIdentity room)
    {
        if (room.Area != AreaId.Crateria || room.RoomIndex >= CrateriaPolicyRooms)
            throw new InvalidDataException($"Room {room} is outside the {CrateriaPolicyRooms} Crateria atmospheric selectors.");
        return CrateriaRoomEffects.TryGetValue(room, out CrateriaAtmosphericEffectFlags effects)
            ? effects
            : CrateriaAtmosphericEffectFlags.None;
    }
}
