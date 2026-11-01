using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Complete compiled palette-FX definition and room-area selection domains used by the
/// retail game: each <see cref="PaletteFxObject"/>'s setup and initial program, and the object each
/// area's effect bit selects.
/// </summary>
internal static class RoomPaletteFxDefinitions
{
    /// <summary>Eight areas in the room palette-FX selection domain: the seven retail areas and one that selects nothing.</summary>
    internal const int AreaCount = 8;
    /// <summary>Eight effect-bit selections within each area.</summary>
    internal const int DefinitionsPerArea = 8;
}
