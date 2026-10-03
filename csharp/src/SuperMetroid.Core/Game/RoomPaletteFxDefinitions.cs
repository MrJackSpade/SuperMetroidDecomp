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
    /// <summary>$8D:F761, Norfair Samus-in-heat palette-FX owner selected by area lists.</summary>
    internal const ushort SamusInHeat = 0xf761;
    /// <summary>$8D:FFC9, first definition in the contiguous Tourian escape group.</summary>
    internal const ushort TourianDefinitionsBegin = 0xffc9;
    /// <summary>$8D:FFED, final definition in the contiguous Tourian escape group.</summary>
    internal const ushort TourianDefinitionsEnd = 0xffed;

    internal const int DefinitionByteCount = 4;
    internal const int AreaCount = 8;
    internal const int DefinitionsPerArea = 8;

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

    /// <summary>Selects setup callback and initial program by native palette-FX object identity.</summary>
    /// <remarks>The 63 aligned objects occupy E194..E200, F745..F7A5 and FFC9..FFED
    /// in bank8D. Each identity dispatches to one setup operation and animation program.
    /// Named setup cases preserve null/intro/Norfair/Brinstar semantics, including
    /// aliases between distinct objects. Unknown/interior addresses reject.
    /// Both fields are independently checked against supported NTSC J/U v1.0 and
    /// pinned bank_8D.asm (362be646929cf8e483f692b73a6561cfc2dc1d0d).
    /// No stored definition array or generated cache remains.</remarks>
    internal static RoomPaletteFxDefinition Get(ushort pointer) => pointer switch
    {
        0xE194 => new(PaletteFxSetupCodes.Null, 0xC696),
        0xE198 => new(PaletteFxSetupCodes.Null, 0xC7AC),
        0xE19C => new(PaletteFxSetupCodes.Null, 0xC7F2),
        0xE1A0 => new(PaletteFxSetupCodes.Null, 0xC7FA),
        0xE1A4 => new(PaletteFxSetupCodes.Null, 0xC862),
        0xE1A8 => new(PaletteFxSetupCodes.Null, 0xC87A),
        0xE1AC => new(PaletteFxSetupCodes.Null, 0xC88E),
        0xE1B0 => new(PaletteFxSetupCodes.Null, 0xC90E),
        0xE1B4 => new(PaletteFxSetupCodes.Null, 0xC964),
        0xE1B8 => new(PaletteFxSetupCodes.Null, 0xC906),
        0xE1BC => new(PaletteFxSetupCodes.Intro, 0xC9BA),
        0xE1C0 => new(PaletteFxSetupCodes.Null, 0xCA4E),
        0xE1C4 => new(PaletteFxSetupCodes.Null, 0xCAAA),
        0xE1C8 => new(PaletteFxSetupCodes.Null, 0xCB3C),
        0xE1CC => new(PaletteFxSetupCodes.Null, 0xCD62),
        0xE1D0 => new(PaletteFxSetupCodes.Null, 0xD36A),
        0xE1D4 => new(PaletteFxSetupCodes.Null, 0xD3CA),
        0xE1D8 => new(PaletteFxSetupCodes.Null, 0xD44A),
        0xE1DC => new(PaletteFxSetupCodes.Null, 0xD48E),
        0xE1E0 => new(PaletteFxSetupCodes.Null, 0xD5A4),
        0xE1E4 => new(PaletteFxSetupCodes.Null, 0xD6BA),
        0xE1E8 => new(PaletteFxSetupCodes.Null, 0xD362),
        0xE1EC => new(PaletteFxSetupCodes.Null, 0xD9D0),
        0xE1F0 => new(PaletteFxSetupCodes.Null, 0xD900),
        0xE1F4 => new(PaletteFxSetupCodes.Null, 0xDB62),
        0xE1F8 => new(PaletteFxSetupCodes.Null, 0xDCC8),
        0xE1FC => new(PaletteFxSetupCodes.Null, 0xDE2E),
        0xE200 => new(PaletteFxSetupCodes.Null, 0xDF94),
        0xF745 => new(PaletteFxSetupCodes.Null, 0xE220),
        0xF749 => new(PaletteFxSetupCodes.Null, 0xE222),
        0xF74D => new(PaletteFxSetupCodes.Null, 0xE22A),
        0xF751 => new(PaletteFxSetupCodes.Null, 0xE232),
        0xF755 => new(PaletteFxSetupCodes.Null, 0xE23A),
        0xF759 => new(PaletteFxSetupCodes.Null, 0xE2E9),
        0xF75D => new(PaletteFxSetupCodes.Null, 0xE331),
        0xF761 => new(PaletteFxSetupCodes.Norfair, 0xE45E),
        0xF765 => new(PaletteFxSetupCodes.Null, 0xEB3B),
        0xF769 => new(PaletteFxSetupCodes.Null, 0xEC6E),
        0xF76D => new(PaletteFxSetupCodes.Null, 0xEAE2),
        0xF771 => new(PaletteFxSetupCodes.Null, 0xEAE2),
        0xF775 => new(PaletteFxSetupCodes.Null, 0xED99),
        0xF779 => new(PaletteFxSetupCodes.Brinstar, 0xEE2D),
        0xF77D => new(PaletteFxSetupCodes.Null, 0xEED7),
        0xF781 => new(PaletteFxSetupCodes.Null, 0xEFF7),
        0xF785 => new(PaletteFxSetupCodes.Null, 0xF08E),
        0xF789 => new(PaletteFxSetupCodes.Null, 0xF1D1),
        0xF78D => new(PaletteFxSetupCodes.Null, 0xF2D9),
        0xF791 => new(PaletteFxSetupCodes.Null, 0xF3E1),
        0xF795 => new(PaletteFxSetupCodes.Null, 0xF4E9),
        0xF799 => new(PaletteFxSetupCodes.Null, 0xF541),
        0xF79D => new(PaletteFxSetupCodes.Null, 0xF579),
        0xF7A1 => new(PaletteFxSetupCodes.Null, 0xF632),
        0xF7A5 => new(PaletteFxSetupCodes.Null, 0xF62A),
        0xFFC9 => new(PaletteFxSetupCodes.Null, 0xF7A9),
        0xFFCD => new(PaletteFxSetupCodes.Null, 0xF891),
        0xFFD1 => new(PaletteFxSetupCodes.Null, 0xF941),
        0xFFD5 => new(PaletteFxSetupCodes.Null, 0xF949),
        0xFFD9 => new(PaletteFxSetupCodes.Null, 0xFA69),
        0xFFDD => new(PaletteFxSetupCodes.Null, 0xFBC1),
        0xFFE1 => new(PaletteFxSetupCodes.Null, 0xFC5F),
        0xFFE5 => new(PaletteFxSetupCodes.Null, 0xFCFD),
        0xFFE9 => new(PaletteFxSetupCodes.Null, 0xFE01),
        0xFFED => new(PaletteFxSetupCodes.Null, 0xFF27),
        _ => throw new InvalidDataException(
            $"Palette-FX definition $8D:{pointer:X4} is outside the compiled retail domain."),
    };
    /// <summary>Returns the palette-FX definition selected by one area bit.</summary>
    internal static ushort GetAreaDefinition(int areaIndex, int bitIndex)
    {
        if ((uint)areaIndex >= AreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex));
        if ((uint)bitIndex >= DefinitionsPerArea)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        return AreaDefinitions[areaIndex * DefinitionsPerArea + bitIndex];
    }

}
