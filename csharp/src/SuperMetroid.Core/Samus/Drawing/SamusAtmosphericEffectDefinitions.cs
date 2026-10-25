using System.Collections.Frozen;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Compiled cartridge policy for Samus's water splashes, running foot contacts, and
/// Crateria-specific atmospheric effects.
/// </summary>
internal static class SamusAtmosphericEffectDefinitions
{
    /// <summary>
    /// The atmospheric effect type whose native sprite-list pointer is zero (<c>$90:8C03</c>), so <c>$90:8B03</c>'s
    /// <c>LDA $0000,Y</c> reads direct-page scratch as its attributes.
    /// </summary>
    internal const SamusAtmosphericEffectType ScratchListType = SamusAtmosphericEffectType.AlternateFootstepSplash;

    /// <summary>The number of Crateria rooms the $90:EDC9/$91:F0F3 atmospheric policy covers.</summary>
    private const int CrateriaPolicyRooms = 16;

    /// <summary>
    /// The Crateria rooms whose $90:EDC9/$91:F0F3 atmospheric policy is nonzero, and that policy; every other room the
    /// policy covers has none.
    /// </summary>
    private static readonly FrozenDictionary<RoomIdentity, CrateriaAtmosphericEffectFlags> CrateriaRoomEffects =
        new Dictionary<RoomIdentity, CrateriaAtmosphericEffectFlags>
        {
            [RoomIdentities.CrateriaLandingSite] = CrateriaAtmosphericEffectFlags.LandingSite,
            [RoomIdentities.CrateriaWestOcean] = CrateriaAtmosphericEffectFlags.WreckedShipEntrance,
            [RoomIdentities.CrateriaKihunterRoom] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomIdentities.CrateriaEastOcean] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomIdentities.CrateriaForgottenHighwayKagoRoom] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomIdentities.CrateriaCrabMaze] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomIdentities.CrateriaForgottenHighwayElbow] = CrateriaAtmosphericEffectFlags.WetFootsteps,
            [RoomIdentities.CrateriaMoat] = CrateriaAtmosphericEffectFlags.WetFootsteps,
        }.ToFrozenDictionary();

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
