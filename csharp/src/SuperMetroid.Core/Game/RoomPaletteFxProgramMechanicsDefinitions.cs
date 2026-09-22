namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared resolver for immutable mechanics words in translated room palette-FX programs.
/// </summary>
public static class RoomPaletteFxProgramMechanicsDefinitions
{
    /// <summary>Resolves one compiled bank-$8D program word across translated owners.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (CeresCinematicLightPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (PlanetZebesTextPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (CinematicGlowPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (PaletteFxHeatProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out value))
            return true;
        if (WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (TourianStatueGreyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (TorizoBellyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (CrateriaLightningPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (TourianGlowPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (BeaconPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }
        if (CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    /// <summary>Resolves one compiled bank-$8D byte operand across translated owners.</summary>
    public static bool TryReadMechanicsByte(ushort pointer, out byte value)
    {
        if (CrateriaLightningPaletteFxProgramMechanicsDefinitions.TryReadMechanicsByte(
                pointer,
                out value))
        {
            return true;
        }
        if (NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.TryReadMechanicsByte(
                pointer,
                out value))
        {
            return true;
        }

        value = 0;
        return false;
    }
}
