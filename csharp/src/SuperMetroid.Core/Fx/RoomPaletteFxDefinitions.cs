using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Complete compiled palette-FX definition and room-area selection domains used by the
/// retail cartridge: each <see cref="PaletteFxObject"/>'s setup and initial program, and the object each
/// area's effect bit selects.
/// </summary>
internal static class RoomPaletteFxDefinitions
{
    /// <summary>Eight retail area rows in the room palette-FX selection domain.</summary>
    internal const int AreaCount = 8;
    /// <summary>Eight effect-bit selections within each area row.</summary>
    internal const int DefinitionsPerArea = 8;
}
