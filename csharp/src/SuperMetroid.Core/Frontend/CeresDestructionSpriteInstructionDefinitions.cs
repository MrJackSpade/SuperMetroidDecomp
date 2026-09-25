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

    private static ReadOnlySpan<byte> LargeAsteroid =>
    [
        0x0a, 0x00, 0x9d, 0x90, 0xbc, 0x94, 0x3f, 0xcc,
    ];

    private static ReadOnlySpan<byte> Planet =>
    [
        0x0a, 0x00, 0x58, 0x95, 0xbc, 0x94, 0xab, 0xcc,
    ];

    private static ReadOnlySpan<byte> Title =>
    [
        0x40, 0x00, 0x00, 0x00, 0xa5, 0xc9, 0x20, 0x00,
        0x54, 0x96, 0xaf, 0xc9, 0xc0, 0x00, 0x54, 0x96,
        0xbd, 0xc9, 0x60, 0x00, 0x54, 0x96, 0xc7, 0xc9,
        0x38, 0x94,
    ];

    private static ReadOnlySpan<byte> Explosions =>
    [
        0x03, 0x00, 0xf7, 0x97, 0x03, 0x00, 0xfe, 0x97,
        0x03, 0x00, 0x05, 0x98, 0x03, 0x00, 0x1b, 0x98,
        0x03, 0x00, 0x31, 0x98, 0x03, 0x00, 0x47, 0x98,
        0x38, 0x94, 0xd6, 0x94, 0x06, 0x00, 0x03, 0x00,
        0xf7, 0x97, 0x03, 0x00, 0xfe, 0x97, 0x03, 0x00,
        0x05, 0x98, 0x03, 0x00, 0x1b, 0x98, 0x03, 0x00,
        0x31, 0x98, 0x03, 0x00, 0x47, 0x98, 0x10, 0x00,
        0x00, 0x00, 0xc3, 0x94, 0xf9, 0xcc, 0x38, 0x94,
        0xd6, 0x94, 0x07, 0x00, 0x05, 0x00, 0xd2, 0x98,
        0x05, 0x00, 0xd9, 0x98, 0x05, 0x00, 0xe0, 0x98,
        0x05, 0x00, 0xe7, 0x98, 0x08, 0x00, 0x00, 0x00,
        0xc3, 0x94, 0x1f, 0xcd, 0x38, 0x94,
    ];

    private static ReadOnlySpan<byte> StarSheets =>
    [
        0x0a, 0x00, 0x5e, 0x97, 0xbc, 0x94, 0x83, 0xcd,
        0x0a, 0x00, 0x9c, 0x97, 0xbc, 0x94, 0x8b, 0xcd,
        0x0a, 0x00, 0xbc, 0x97, 0xbc, 0x94, 0x93, 0xcd,
        0x0a, 0x00, 0xd2, 0x97, 0xbc, 0x94, 0x9b, 0xcd,
    ];

    private static ReadOnlySpan<byte> StationBlast =>
    [
        0x05, 0x00, 0xee, 0x98, 0x05, 0x00, 0x04, 0x99,
        0x05, 0x00, 0x1a, 0x99, 0x05, 0x00, 0x30, 0x99,
        0x05, 0x00, 0x6e, 0x99, 0x05, 0x00, 0x98, 0x99,
        0x38, 0x94,
    ];

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer is >= LargeAsteroidStart and < LargeAsteroidEnd)
            return LargeAsteroid[pointer - LargeAsteroidStart];
        if (pointer is >= CeresFlightSpriteInstructionDefinitions.RearClusterStart and
            < CeresFlightSpriteInstructionDefinitions.RearClusterEnd)
            return CeresFlightSpriteInstructionDefinitions.ReadByte(pointer);
        if (pointer is >= PlanetStart and < PlanetEnd)
            return Planet[pointer - PlanetStart];
        if (pointer is >= TitleStart and < TitleEnd)
            return Title[pointer - TitleStart];
        if (pointer is >= ExplosionsStart and < ExplosionsEnd)
            return Explosions[pointer - ExplosionsStart];
        if (pointer is >= StarSheetsStart and < StarSheetsEnd)
            return StarSheets[pointer - StarSheetsStart];
        if (pointer is >= StationBlastStart and < StationBlastEnd)
            return StationBlast[pointer - StationBlastStart];
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
