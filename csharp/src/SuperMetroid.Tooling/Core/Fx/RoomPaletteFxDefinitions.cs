namespace SuperMetroid.Core.Fx;

/// <summary>
/// The dimensions of the cartridge's per-area palette-FX selection lists, which <see cref="AreaPaletteFxObjects"/>
/// selects from and <see cref="RoomFxRecordCodec"/> resolves a record's palette-effect bits through (#1526).
/// </summary>
internal static class RoomPaletteFxDefinitions
{
    /// <summary>Eight areas in the room palette-FX selection domain: the seven retail areas and one that selects nothing.</summary>
    internal const int AreaCount = 8;
    /// <summary>Eight effect-bit selections within each area.</summary>
    internal const int DefinitionsPerArea = 8;
}
