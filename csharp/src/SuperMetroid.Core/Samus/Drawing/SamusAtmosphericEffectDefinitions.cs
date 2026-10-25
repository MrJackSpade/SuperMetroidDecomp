using SuperMetroid.Core.Fx.PaletteFx;
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

    // The Crateria rooms whose $90:EDC9/$91:F0F3 entries are nonzero, by their room index within the area; the
    // other rooms have none.

    /// <summary>The Landing Site's room index ($8F:91F8).</summary>
    private const int LandingSiteRoom = 0;
    /// <summary>The West Ocean's room index ($8F:93FE).</summary>
    private const int WestOceanRoom = 5;
    /// <summary>The Crateria Kihunter room's room index ($8F:948C).</summary>
    private const int CrateriaKihunterRoom = 7;
    /// <summary>The East Ocean's room index ($8F:94FD).</summary>
    private const int EastOceanRoom = 9;
    /// <summary>The Forgotten Highway Kago room's room index ($8F:9552).</summary>
    private const int ForgottenHighwayKagosRoom = 10;
    /// <summary>The Crab Maze's room index ($8F:957D).</summary>
    private const int CrabMazeRoom = 11;
    /// <summary>The Forgotten Highway elbow's room index ($8F:95A8).</summary>
    private const int ForgottenHighwayElbowRoom = 12;
    /// <summary>The Moat's room index ($8F:95FF).</summary>
    private const int MoatRoom = 14;

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
    internal static CrateriaAtmosphericEffectFlags ForCrateriaRoom(int roomIndex)
    {
        if ((uint)roomIndex >= 16)
            throw new InvalidDataException($"Crateria atmospheric room index ${roomIndex:X2} is outside 16 selectors.");
        return roomIndex switch
        {
            LandingSiteRoom => CrateriaAtmosphericEffectFlags.LandingSite,
            WestOceanRoom => CrateriaAtmosphericEffectFlags.WreckedShipEntrance,
            CrateriaKihunterRoom or EastOceanRoom or ForgottenHighwayKagosRoom or
            CrabMazeRoom or ForgottenHighwayElbowRoom or MoatRoom =>
                CrateriaAtmosphericEffectFlags.WetFootsteps,
            _ => CrateriaAtmosphericEffectFlags.None,
        };
    }
}
