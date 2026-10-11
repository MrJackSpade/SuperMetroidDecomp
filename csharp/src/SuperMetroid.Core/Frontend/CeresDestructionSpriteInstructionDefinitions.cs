using SuperMetroid.Core.Assets;

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

    private static byte LoopByte(int offset, ushort start, ushort frame)
    {
        ushort word = (offset / 2) switch
        {
            0 => 10,
            1 => frame,
            2 => (ushort)CinematicSpriteInstruction.Goto,
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
            2 => (ushort)ZebesTitleInstruction.FadeInText,
            3 => 32,
            4 or 7 or 10 => (ushort)CeresDestructionBackdrop.Title,
            5 => (ushort)ZebesTitleInstruction.SpawnJapaneseTextIfNeeded,
            6 => 192,
            8 => (ushort)ZebesTitleInstruction.FadeOutText,
            9 => 96,
            11 => (ushort)ZebesTitleInstruction.StartFlyingToZebes,
            12 => (ushort)CinematicSpriteInstruction.Delete,
            _ => throw new InvalidDataException("Planet Zebes title cursor is invalid."),
        };
        return (byte)(word >> (8 * (offset & 1)));
    }

    private static byte StarSheetByte(int offset)
    {
        int quadrant = offset / 8;
        ushort frame = quadrant switch
        {
            0 => (ushort)CeresDestructionBackdrop.UpperLeftStars,
            1 => (ushort)CeresDestructionBackdrop.UpperRightStars,
            2 => (ushort)CeresDestructionBackdrop.LowerLeftStars,
            3 => (ushort)CeresDestructionBackdrop.LowerRightStars,
            _ => throw new InvalidDataException("Zebes star-sheet quadrant is invalid."),
        };
        return LoopByte(offset % 8, (ushort)(StarSheetsStart + quadrant * 8), frame);
    }
    internal static byte ReadByte(ushort pointer)
    {
        if (pointer is >= LargeAsteroidStart and < LargeAsteroidEnd)
            return LoopByte(pointer - LargeAsteroidStart, LargeAsteroidStart, (ushort)CeresDestructionBackdrop.LargeAsteroids);
        if (pointer is >= CeresFlightSpriteInstructionDefinitions.RearClusterStart and
            < CeresFlightSpriteInstructionDefinitions.RearClusterEnd)
            return CeresFlightSpriteInstructionDefinitions.ReadByte(pointer);
        if (pointer is >= PlanetStart and < PlanetEnd)
            return LoopByte(pointer - PlanetStart, PlanetStart, (ushort)CeresDestructionBackdrop.Planet);
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
