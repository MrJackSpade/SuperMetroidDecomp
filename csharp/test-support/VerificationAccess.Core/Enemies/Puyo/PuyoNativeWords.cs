using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Verification's native view of the Puyo identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class PuyoNativeWords
{
    /// <summary>The routine word naming each <see cref="PuyoAirborneFunction"/>.</summary>
    internal static readonly NativeWordIdentities<PuyoAirborneFunction> PuyoAirborneFunctions = new(new Dictionary<PuyoAirborneFunction, ushort>
    {
        [PuyoAirborneFunction.NormalShortHop] = 0x9d0b,
        [PuyoAirborneFunction.NormalBigHop] = 0x9d2b,
        [PuyoAirborneFunction.NormalLongHop] = 0x9d4b,
        [PuyoAirborneFunction.GiantHop] = 0x9d6b,
        [PuyoAirborneFunction.Dropping] = 0x9d98,
        [PuyoAirborneFunction.Dropped] = 0x9dcd,
    });

    /// <summary>The routine word naming each <see cref="PuyoEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<PuyoEnemyFunction> PuyoEnemyFunctions = new(new Dictionary<PuyoEnemyFunction, ushort>
    {
        [PuyoEnemyFunction.Grounded] = 0x9b65,
        [PuyoEnemyFunction.Airborne] = 0x9b81,
    });
}
