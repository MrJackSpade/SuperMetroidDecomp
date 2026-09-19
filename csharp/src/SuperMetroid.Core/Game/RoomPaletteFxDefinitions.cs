namespace SuperMetroid.Core.Game;

/// <summary>One four-byte <c>PalFxDef</c> record from bank <c>$8D</c>.</summary>
/// <param name="SetupCallback">Native setup callback invoked when the object acquires a slot.</param>
/// <param name="InitialInstructionList">Initial mixed palette-animation program.</param>
internal readonly record struct RoomPaletteFxDefinition(
    ushort SetupCallback,
    ushort InitialInstructionList);

/// <summary>
/// Complete compiled palette-FX definition and room-area selection domains used by the
/// retail cartridge. Native pointers remain the external identity stored in room and
/// cinematic programs; fixed callback metadata is application-owned.
/// </summary>
internal static class RoomPaletteFxDefinitions
{
    /// <summary>$8D:E194, first definition in the contiguous cinematic/Samus group.</summary>
    internal const ushort CinematicDefinitionsBegin = 0xe194;
    /// <summary>$8D:E200, final definition in the contiguous cinematic/Samus group.</summary>
    internal const ushort CinematicDefinitionsEnd = 0xe200;
    /// <summary>$8D:F745, first definition in the contiguous room-effect group.</summary>
    internal const ushort RoomDefinitionsBegin = 0xf745;
    /// <summary>$8D:F7A5, final definition in the contiguous room-effect group.</summary>
    internal const ushort RoomDefinitionsEnd = 0xf7a5;
    /// <summary>$8D:FFC9, first definition in the contiguous Tourian escape group.</summary>
    internal const ushort TourianDefinitionsBegin = 0xffc9;
    /// <summary>$8D:FFED, final definition in the contiguous Tourian escape group.</summary>
    internal const ushort TourianDefinitionsEnd = 0xffed;

    internal const int DefinitionByteCount = 4;
    internal const int AreaCount = 8;
    internal const int DefinitionsPerArea = 8;

    private static readonly RoomPaletteFxDefinition[] CinematicDefinitions =
    [
        new(0xC685, 0xC696), new(0xC685, 0xC7AC), new(0xC685, 0xC7F2), new(0xC685, 0xC7FA),
        new(0xC685, 0xC862), new(0xC685, 0xC87A), new(0xC685, 0xC88E), new(0xC685, 0xC90E),
        new(0xC685, 0xC964), new(0xC685, 0xC906), new(0xE204, 0xC9BA), new(0xC685, 0xCA4E),
        new(0xC685, 0xCAAA), new(0xC685, 0xCB3C), new(0xC685, 0xCD62), new(0xC685, 0xD36A),
        new(0xC685, 0xD3CA), new(0xC685, 0xD44A), new(0xC685, 0xD48E), new(0xC685, 0xD5A4),
        new(0xC685, 0xD6BA), new(0xC685, 0xD362), new(0xC685, 0xD9D0), new(0xC685, 0xD900),
        new(0xC685, 0xDB62), new(0xC685, 0xDCC8), new(0xC685, 0xDE2E), new(0xC685, 0xDF94),
    ];

    private static readonly RoomPaletteFxDefinition[] RoomDefinitions =
    [
        new(0xC685, 0xE220), new(0xC685, 0xE222), new(0xC685, 0xE22A), new(0xC685, 0xE232),
        new(0xC685, 0xE23A), new(0xC685, 0xE2E9), new(0xC685, 0xE331), new(0xE440, 0xE45E),
        new(0xC685, 0xEB3B), new(0xC685, 0xEC6E), new(0xC685, 0xEAE2), new(0xC685, 0xEAE2),
        new(0xC685, 0xED99), new(0xF730, 0xEE2D), new(0xC685, 0xEED7), new(0xC685, 0xEFF7),
        new(0xC685, 0xF08E), new(0xC685, 0xF1D1), new(0xC685, 0xF2D9), new(0xC685, 0xF3E1),
        new(0xC685, 0xF4E9), new(0xC685, 0xF541), new(0xC685, 0xF579), new(0xC685, 0xF632),
        new(0xC685, 0xF62A),
    ];

    private static readonly RoomPaletteFxDefinition[] TourianDefinitions =
    [
        new(0xC685, 0xF7A9), new(0xC685, 0xF891), new(0xC685, 0xF941), new(0xC685, 0xF949),
        new(0xC685, 0xFA69), new(0xC685, 0xFBC1), new(0xC685, 0xFC5F), new(0xC685, 0xFCFD),
        new(0xC685, 0xFE01), new(0xC685, 0xFF27),
    ];

    private static readonly ushort[] AreaListPointers =
        [0xac66, 0xac86, 0xaca6, 0xacc6, 0xace6, 0xad06, 0xad26, 0xad46];

    /// <summary>
    /// Bank-$83 area-list identities selected by <c>$83:AC46-$83:AC55</c>. Retained for
    /// cartridge diagnostics; production selection uses the compiled matrix below.
    /// </summary>
    internal static ReadOnlySpan<ushort> NativeAreaListPointers => AreaListPointers;

    private static readonly ushort[] AreaDefinitions =
    [
        0xF765, 0xFFE5, 0xFFE9, 0xFFD9, 0xFFDD, 0xFFE1, 0xFFED, 0xF781,
        0xF775, 0xF77D, 0xF781, 0xF779, 0xF745, 0xF745, 0xF745, 0xF745,
        0xF761, 0xF785, 0xF789, 0xF78D, 0xF791, 0xF745, 0xF745, 0xF745,
        0xF76D, 0xF745, 0xF745, 0xF745, 0xF745, 0xF745, 0xF745, 0xF745,
        0xF795, 0xF799, 0xF79D, 0xF745, 0xF745, 0xF745, 0xF745, 0xF745,
        0xF761, 0xF7A1, 0xF7A5, 0xFFC9, 0xFFCD, 0xFFD1, 0xFFD5, 0xF745,
        0xF745, 0xF745, 0xF745, 0xF745, 0xF745, 0xF745, 0xF745, 0xF745,
        0xF745, 0xF745, 0xF745, 0xF745, 0xF745, 0xF745, 0xF745, 0xF745,
    ];

    /// <summary>Returns one compiled setup/list pair by its native bank-$8D identity.</summary>
    internal static RoomPaletteFxDefinition Get(ushort pointer)
    {
        if (TryGet(CinematicDefinitions, CinematicDefinitionsBegin, CinematicDefinitionsEnd, pointer, out var definition) ||
            TryGet(RoomDefinitions, RoomDefinitionsBegin, RoomDefinitionsEnd, pointer, out definition) ||
            TryGet(TourianDefinitions, TourianDefinitionsBegin, TourianDefinitionsEnd, pointer, out definition))
        {
            return definition;
        }

        throw new InvalidDataException(
            $"Palette-FX definition $8D:{pointer:X4} is outside the compiled retail domain.");
    }

    /// <summary>Returns the palette-FX definition selected by one area bit.</summary>
    internal static ushort GetAreaDefinition(int areaIndex, int bitIndex)
    {
        if ((uint)areaIndex >= AreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex));
        if ((uint)bitIndex >= DefinitionsPerArea)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        return AreaDefinitions[areaIndex * DefinitionsPerArea + bitIndex];
    }

    private static bool TryGet(
        RoomPaletteFxDefinition[] definitions,
        ushort first,
        ushort last,
        ushort pointer,
        out RoomPaletteFxDefinition definition)
    {
        int offset = pointer - first;
        if (pointer >= first && pointer <= last && offset % DefinitionByteCount == 0)
        {
            definition = definitions[offset / DefinitionByteCount];
            return true;
        }

        definition = default;
        return false;
    }
}
