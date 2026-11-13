using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Verification's native view of Waver's animations (#1481): the cartridge's animation-table index naming each
/// <see cref="WaverSegment"/>, whose low bit is the right facing and whose second bit is the spin. Core asks for
/// animations by name; a native snapshot or ROM oracle translates through this table.
/// </summary>
internal static class WaverNativeWords
{
    /// <summary>The animation-table index naming each <see cref="WaverSegment"/>.</summary>
    internal static readonly NativeWordIdentities<WaverSegment> Animations = new(new Dictionary<WaverSegment, ushort>
    {
        [WaverSegment.SteadyFacingLeft] = 0,
        [WaverSegment.SteadyFacingRight] = 1,
        [WaverSegment.SpinningFacingLeft] = 2,
        [WaverSegment.SpinningFacingRight] = 3,
    });
}
