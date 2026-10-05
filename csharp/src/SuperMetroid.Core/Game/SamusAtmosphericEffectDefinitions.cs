namespace SuperMetroid.Core.Game;

/// <summary>Physical water-entry particle layouts selected by Samus's movement type.</summary>
internal enum WaterSplashKind : byte
{
    Diving = 0,
    GroundedPair = 1,
}

/// <summary>
/// Composable room-policy bits used by the duplicated Crateria footstep and landing tables.
/// </summary>
[Flags]
internal enum CrateriaAtmosphericEffectFlags : byte
{
    None = 0,
    LandingSite = 1,
    WreckedShipEntrance = 2,
    WetFootsteps = 4,
}

/// <summary>
/// Compiled cartridge policy for Samus's water splashes, running foot contacts, and
/// Crateria-specific atmospheric effects.
/// </summary>
internal static class SamusAtmosphericEffectDefinitions
{
    /// <summary>$8F:91F8 LandingSite header, room index zero.</summary>
    private const byte LandingSiteRoom = 0;
    /// <summary>$8F:93FE WestOcean header, room index five.</summary>
    private const byte WestOceanRoom = 5;
    /// <summary>$8F:948C CrateriaKihunter header, room index seven.</summary>
    private const byte CrateriaKihunterRoom = 7;
    /// <summary>$8F:94FD EastOcean header, room index nine.</summary>
    private const byte EastOceanRoom = 9;
    /// <summary>$8F:9552 ForgottenHighwayKagos header, room index ten.</summary>
    private const byte ForgottenHighwayKagosRoom = 10;
    /// <summary>$8F:957D CrabMaze header, room index eleven.</summary>
    private const byte CrabMazeRoom = 11;
    /// <summary>$8F:95A8 ForgottenHighwayElbow header, room index twelve.</summary>
    private const byte ForgottenHighwayElbowRoom = 12;
    /// <summary>$8F:95FF Moat header, room index fourteen.</summary>
    private const byte MoatRoom = 14;

    /// <summary>$90:81A4: grounded postures produce the paired surface splash; other movement dives.</summary>
    internal static WaterSplashKind WaterSplashFor(SamusMovementType movementType)
    {
        if ((byte)movementType > (byte)SamusMovementType.Special)
            throw new InvalidDataException($"Water-splash movement type ${(byte)movementType:X2} is outside 28 retail selectors.");
        return movementType switch
        {
            SamusMovementType.Standing or SamusMovementType.MorphBallGround or SamusMovementType.Crouching or
            SamusMovementType.TurningOnGround or SamusMovementType.PostureTransition or SamusMovementType.Moonwalking or
            SamusMovementType.SpringBallGround or SamusMovementType.RanIntoWall => WaterSplashKind.GroundedPair,
            _ => WaterSplashKind.Diving,
        };
    }
    /// <summary>$90:A424: one contact at phase two of each five-frame running step.</summary>
    internal static bool IsRunningFootContact(ushort animationFrame)
    {
        if (animationFrame >= 10)
            throw new InvalidDataException($"Running animation frame {animationFrame} is outside ten foot-contact selectors.");
        return animationFrame % 5 == 2;
    }
    /// <summary>$90:EDC9 and $91:F0F3: room-specific landing/rain and wet-floor policies.</summary>
    internal static CrateriaAtmosphericEffectFlags ForCrateriaRoom(byte roomIndex)
    {
        if (roomIndex >= 16)
            throw new InvalidDataException($"Crateria atmospheric room index ${roomIndex:X2} is outside 16 selectors.");
        return roomIndex switch
        {
            LandingSiteRoom => CrateriaAtmosphericEffectFlags.LandingSite,
            WestOceanRoom => CrateriaAtmosphericEffectFlags.WreckedShipEntrance,
            CrateriaKihunterRoom or EastOceanRoom or ForgottenHighwayKagosRoom or CrabMazeRoom or
            ForgottenHighwayElbowRoom or MoatRoom => CrateriaAtmosphericEffectFlags.WetFootsteps,
            _ => CrateriaAtmosphericEffectFlags.None,
        };
    }
}
