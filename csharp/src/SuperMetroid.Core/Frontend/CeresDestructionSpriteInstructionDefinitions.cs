namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Fixed bank-$8B cinematic-sprite lists consumed by the Ceres destruction
/// and following Zebes reveal. The shared small-asteroid and vortex lists
/// delegate to the approach owner; visual spritemaps are separate assets.
/// </summary>
internal static class CeresDestructionSpriteInstructionDefinitions
{
    /// <summary>$8B:CC3F, first byte of the distinct destruction large-asteroid list.</summary>
    internal const ushort LargeAsteroidStart = 0xcc3f;
    /// <summary>$8B:CC47, exclusive end before the approach's under-attack list.</summary>
    internal const ushort LargeAsteroidEnd = 0xcc47;
    /// <summary>$8B:CCAB, first byte of the planet-Zebes actor list.</summary>
    internal const ushort PlanetStart = 0xccab;
    /// <summary>$8B:CCB3, exclusive end before an unrelated actor list.</summary>
    internal const ushort PlanetEnd = 0xccb3;
    /// <summary>$8B:CCBB, first byte of the PLANET ZEBES title/callback list.</summary>
    internal const ushort TitleStart = 0xccbb;
    /// <summary>$8B:CCD5, exclusive end before the subtitle list.</summary>
    internal const ushort TitleEnd = 0xccd5;
    /// <summary>$8B:CCDB, first byte of the initial small-explosion list.</summary>
    internal const ushort ExplosionsStart = 0xccdb;
    /// <summary>$8B:CCF5, end of the initial small-explosion list.</summary>
    internal const ushort InitialExplosionEnd = 0xccf5;
    /// <summary>$8B:CD1B, end of the repeating small-explosion list.</summary>
    internal const ushort RepeatingExplosionEnd = 0xcd1b;
    /// <summary>$8B:CD39, exclusive end after the final-wave explosion list.</summary>
    internal const ushort ExplosionsEnd = 0xcd39;
    /// <summary>$8B:CD83, first byte of four adjacent Zebes star-sheet lists.</summary>
    internal const ushort StarSheetsStart = 0xcd83;
    /// <summary>$8B:CDA3, exclusive end before the approach's star-field list.</summary>
    internal const ushort StarSheetsEnd = 0xcda3;
    /// <summary>$8B:CE1B, first byte of the final station-blast list.</summary>
    internal const ushort StationBlastStart = 0xce1b;
    /// <summary>$8B:CE35, exclusive end before the spawner's separate program.</summary>
    internal const ushort StationBlastEnd = 0xce35;

    /// <summary>$8C:909D, Ceres under-attack large-asteroid spritemap.</summary>
    private const ushort LargeAsteroidFrame = 0x909d;
    /// <summary>$8C:9558, SpaceSpritemaps_Zebes.</summary>
    private const ushort PlanetFrame = 0x9558;
    /// <summary>$8C:9654, SpaceSpritemaps_PlanetZebes title.</summary>
    private const ushort TitleFrame = 0x9654;
    /// <summary>$8C:975E, SpaceSpritemaps_ZebesStars2, upper-left sheet.</summary>
    private const ushort UpperLeftStars = 0x975e;
    /// <summary>$8C:979C, SpaceSpritemaps_ZebesStars3, upper-right sheet.</summary>
    private const ushort UpperRightStars = 0x979c;
    /// <summary>$8C:97BC, SpaceSpritemaps_ZebesStars4, lower-left sheet.</summary>
    private const ushort LowerLeftStars = 0x97bc;
    /// <summary>$8C:97D2, SpaceSpritemaps_ZebesStars5, lower-right sheet.</summary>
    private const ushort LowerRightStars = 0x97d2;

    private static byte LoopByte(int offset, ushort start, ushort frame)
    {
        ushort word = (offset / 2) switch
        {
            0 => 10,
            1 => frame,
            2 => CinematicCodePointers.CinematicSpriteObject_Instruction_Goto,
            3 => start,
            _ => throw new InvalidDataException("Ceres static-frame loop cursor is invalid."),
        };
        return (byte)(word >> (8 * (offset & 1)));
    }

    private static byte TitleByte(int offset)
    {
        ushort word = (offset / 2) switch
        {
            0 => 64, // Blank wait before fading in.
            1 => 0,
            2 => CinematicCodePointers.Instruction_FadeInPlanetZebesText,
            3 => 32,
            4 or 7 or 10 => TitleFrame,
            5 => CinematicCodePointers.Instruction_SpawnPlanetZebesJapanTextIfNeeded,
            6 => 192,
            8 => CinematicCodePointers.Instruction_FadeOutPlanetZebesText,
            9 => 96,
            11 => CinematicCodePointers.Instruction_StartFlyingToZebes,
            12 => CinematicCodePointers.CinematicSpriteObject_Instruction_Delete,
            _ => throw new InvalidDataException("Planet Zebes title cursor is invalid."),
        };
        return (byte)(word >> (8 * (offset & 1)));
    }

    private static byte StarSheetByte(int offset)
    {
        int quadrant = offset / 8;
        ushort frame = quadrant switch
        {
            0 => UpperLeftStars,
            1 => UpperRightStars,
            2 => LowerLeftStars,
            3 => LowerRightStars,
            _ => throw new InvalidDataException("Zebes star-sheet quadrant is invalid."),
        };
        return LoopByte(offset % 8, (ushort)(StarSheetsStart + quadrant * 8), frame);
    }
    internal static byte ReadByte(ushort pointer)
    {
        if (pointer is >= LargeAsteroidStart and < LargeAsteroidEnd)
            return LoopByte(pointer - LargeAsteroidStart, LargeAsteroidStart, LargeAsteroidFrame);
        if (pointer is >= CeresFlightSpriteInstructionDefinitions.RearClusterStart and
            < CeresFlightSpriteInstructionDefinitions.RearClusterEnd)
            return CeresFlightSpriteInstructionDefinitions.ReadByte(pointer);
        if (pointer is >= PlanetStart and < PlanetEnd)
            return LoopByte(pointer - PlanetStart, PlanetStart, PlanetFrame);
        if (pointer is >= TitleStart and < TitleEnd)
            return TitleByte(pointer - TitleStart);
        if (pointer is >= ExplosionsStart and < ExplosionsEnd)
            return CeresExplosionInstructionDefinitions.ReadByte(pointer);
        if (pointer is >= StarSheetsStart and < StarSheetsEnd)
            return StarSheetByte(pointer - StarSheetsStart);
        if (pointer is >= StationBlastStart and < StationBlastEnd)
            return CeresExplosionInstructionDefinitions.ReadByte(pointer);
        throw new InvalidDataException(
            $"Ceres destruction instruction $8B:{pointer:X4} leaves its compiled lists.");
    }

    internal static ushort ReadWord(ushort pointer)
    {
        ushort next = unchecked((ushort)(pointer + 1));
        if (next is LargeAsteroidEnd or PlanetEnd or TitleEnd or
            InitialExplosionEnd or RepeatingExplosionEnd or ExplosionsEnd or
            StationBlastEnd or StarSheetsEnd or
            CeresFlightSpriteInstructionDefinitions.RearClusterEnd ||
            (next >= StarSheetsStart && next < StarSheetsEnd &&
                (next - StarSheetsStart) % 8 == 0))
        {
            throw new InvalidDataException(
                $"Ceres destruction word $8B:{pointer:X4} crosses a compiled-list boundary.");
        }
        return (ushort)(ReadByte(pointer) | ReadByte(next) << 8);
    }
}
