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

    /// <summary>Calculates a native palette-FX area-list identity for index0..7.</summary>
    /// <remarks>$83:AC46 has eight pointers to AC66+20h*area. Each area owns an
    /// eight-word palette list followed by an eight-word animated-tile list.
    /// Includes the debug area; invalid indices retain the former span rejection.
    /// Independently verified against NTSC J/U v1.0 and pinned bank_83.asm
    /// (362be646929cf8e483f692b73a6561cfc2dc1d0d).</remarks>
    internal static ushort NativeAreaListPointer(int areaIndex)
    {
        if ((uint)areaIndex >= AreaCount) throw new IndexOutOfRangeException();
        return (ushort)(0xac66 + areaIndex * 0x20);
    }

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
    /// <summary>Selects the palette-FX object enabled by one area's effect bit.</summary>
    /// <remarks>Direct semantic area/bit dispatch for all eight areas and eight bits.
    /// Unused selections (including Ceres/debug) select the native empty object.
    /// Validate area before bit, preserving both rejection domains and precedence.
    /// All64 native words are independently verified; no selection matrix remains.</remarks>
    internal static ushort GetAreaDefinition(int areaIndex, int bitIndex)
    {
        if ((uint)areaIndex >= AreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex));
        if ((uint)bitIndex >= DefinitionsPerArea)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        return ((AreaId)areaIndex, bitIndex) switch
        {
            (AreaId.Crateria, 0) => 0xF765,
            (AreaId.Crateria, 1) => 0xFFE5,
            (AreaId.Crateria, 2) => 0xFFE9,
            (AreaId.Crateria, 3) => 0xFFD9,
            (AreaId.Crateria, 4) => 0xFFDD,
            (AreaId.Crateria, 5) => 0xFFE1,
            (AreaId.Crateria, 6) => 0xFFED,
            (AreaId.Crateria, 7) => 0xF781,
            (AreaId.Brinstar, 0) => 0xF775,
            (AreaId.Brinstar, 1) => 0xF77D,
            (AreaId.Brinstar, 2) => 0xF781,
            (AreaId.Brinstar, 3) => 0xF779,
            (AreaId.Norfair, 0) => 0xF761,
            (AreaId.Norfair, 1) => 0xF785,
            (AreaId.Norfair, 2) => 0xF789,
            (AreaId.Norfair, 3) => 0xF78D,
            (AreaId.Norfair, 4) => 0xF791,
            (AreaId.WreckedShip, 0) => 0xF76D,
            (AreaId.Maridia, 0) => 0xF795,
            (AreaId.Maridia, 1) => 0xF799,
            (AreaId.Maridia, 2) => 0xF79D,
            (AreaId.Tourian, 0) => 0xF761,
            (AreaId.Tourian, 1) => 0xF7A1,
            (AreaId.Tourian, 2) => 0xF7A5,
            (AreaId.Tourian, 3) => 0xFFC9,
            (AreaId.Tourian, 4) => 0xFFCD,
            (AreaId.Tourian, 5) => 0xFFD1,
            (AreaId.Tourian, 6) => 0xFFD5,
            _ => PaletteFxDeleteProgramMechanicsDefinitions.EmptyRoomEffectDefinition,
        };
    }

}
