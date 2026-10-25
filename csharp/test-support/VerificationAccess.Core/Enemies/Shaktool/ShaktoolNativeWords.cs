using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Verification's native view of the Shaktool identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class ShaktoolNativeWords
{
    /// <summary>The routine word naming each <see cref="ShaktoolPreInstruction"/>.</summary>
    internal static readonly NativeWordIdentities<ShaktoolPreInstruction> ShaktoolPreInstructions = new(new Dictionary<ShaktoolPreInstruction, ushort>
    {
        [ShaktoolPreInstruction.IdleAfterAttack] = 0xdcaa,
        [ShaktoolPreInstruction.IdleHead] = 0xdcab,
        [ShaktoolPreInstruction.OrbitPreviousSegment] = 0xdcac,
        [ShaktoolPreInstruction.OrbitAndOrientCenter] = 0xdcd7,
        [ShaktoolPreInstruction.DriveTailAndReverseAtWalls] = 0xdd25,
    });
}
