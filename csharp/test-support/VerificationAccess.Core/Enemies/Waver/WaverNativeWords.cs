using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Verification's native view of Waver's animations (#1481): the cartridge's animation-table index naming each
/// <see cref="WaverProgram"/>, whose low bit is the right facing and whose second bit is the spin. Core asks for
/// animations by name; a native snapshot or ROM oracle translates through this table.
/// </summary>
internal static class WaverNativeWords
{
    /// <summary>The animation-table index naming each <see cref="WaverProgram"/>.</summary>
    internal static readonly NativeWordIdentities<WaverProgram> Animations = new(new Dictionary<WaverProgram, ushort>
    {
        [WaverProgram.SteadyFacingLeft] = 0,
        [WaverProgram.SteadyFacingRight] = 1,
        [WaverProgram.SpinningFacingLeft] = 2,
        [WaverProgram.SpinningFacingRight] = 3,
    });
}
