using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Verification's native view of the Bull identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class BullNativeWords
{
    /// <summary>The routine word naming each <see cref="BullEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<BullEnemyFunction> BullEnemyFunctions = new(new Dictionary<BullEnemyFunction, ushort>
    {
        [BullEnemyFunction.MovementDelay] = 0xd92b,
        [BullEnemyFunction.TargetSamus] = 0xd940,
        [BullEnemyFunction.Accelerating] = 0xd963,
        [BullEnemyFunction.Decelerating] = 0xd97c,
    });
}
