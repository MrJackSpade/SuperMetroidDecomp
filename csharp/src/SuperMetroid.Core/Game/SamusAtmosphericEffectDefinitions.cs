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
    /// <summary>The 28 movement-type splash selectors at <c>$90:81A4-$81BF</c>.</summary>
    private static readonly WaterSplashKind[] WaterSplashKinds =
    [
        WaterSplashKind.GroundedPair,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.GroundedPair,
        WaterSplashKind.GroundedPair,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.GroundedPair,
        WaterSplashKind.GroundedPair,
        WaterSplashKind.GroundedPair,
        WaterSplashKind.GroundedPair,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.GroundedPair,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
        WaterSplashKind.Diving,
    ];

    /// <summary>The ten running-animation foot-contact flags at <c>$90:A424-$A42D</c>.</summary>
    private static readonly bool[] RunningFootContacts =
    [
        false, false, true, false, false,
        false, false, true, false, false,
    ];

    /// <summary>
    /// The 16 Crateria room classifications duplicated at <c>$90:EDC9-$EDD8</c> for
    /// footsteps and <c>$91:F0F3-$F102</c> for landing effects.
    /// </summary>
    private static readonly CrateriaAtmosphericEffectFlags[] CrateriaRoomEffects =
    [
        CrateriaAtmosphericEffectFlags.LandingSite,
        CrateriaAtmosphericEffectFlags.None,
        CrateriaAtmosphericEffectFlags.None,
        CrateriaAtmosphericEffectFlags.None,
        CrateriaAtmosphericEffectFlags.None,
        CrateriaAtmosphericEffectFlags.WreckedShipEntrance,
        CrateriaAtmosphericEffectFlags.None,
        CrateriaAtmosphericEffectFlags.WetFootsteps,
        CrateriaAtmosphericEffectFlags.None,
        CrateriaAtmosphericEffectFlags.WetFootsteps,
        CrateriaAtmosphericEffectFlags.WetFootsteps,
        CrateriaAtmosphericEffectFlags.WetFootsteps,
        CrateriaAtmosphericEffectFlags.WetFootsteps,
        CrateriaAtmosphericEffectFlags.None,
        CrateriaAtmosphericEffectFlags.WetFootsteps,
        CrateriaAtmosphericEffectFlags.None,
    ];

    /// <summary>Returns the water-entry particle layout for one native movement type.</summary>
    internal static WaterSplashKind WaterSplashFor(SamusMovementType movementType)
    {
        int index = (byte)movementType;
        if ((uint)index >= WaterSplashKinds.Length)
        {
            throw new InvalidDataException(
                $"Water-splash movement type ${index:X2} is outside 28 retail selectors.");
        }

        return WaterSplashKinds[index];
    }

    /// <summary>Returns whether one running-animation frame is a foot contact.</summary>
    internal static bool IsRunningFootContact(ushort animationFrame)
    {
        if (animationFrame >= RunningFootContacts.Length)
        {
            throw new InvalidDataException(
                $"Running animation frame {animationFrame} is outside ten foot-contact selectors.");
        }

        return RunningFootContacts[animationFrame];
    }

    /// <summary>Returns the atmospheric-effect policy for one bounded Crateria room index.</summary>
    internal static CrateriaAtmosphericEffectFlags ForCrateriaRoom(byte roomIndex)
    {
        if (roomIndex >= CrateriaRoomEffects.Length)
        {
            throw new InvalidDataException(
                $"Crateria atmospheric room index ${roomIndex:X2} is outside 16 selectors.");
        }

        return CrateriaRoomEffects[roomIndex];
    }
}
