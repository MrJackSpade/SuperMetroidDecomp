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

    /// <summary>The byte offset of each <see cref="PuyoHopType"/>'s eight-byte record in the cartridge's hop table.</summary>
    internal static readonly NativeWordIdentities<PuyoHopType> HopRecordOffsets = new(new Dictionary<PuyoHopType, ushort>
    {
        [PuyoHopType.NormalSmall] = 0,
        [PuyoHopType.NormalBig] = 8,
        [PuyoHopType.NormalHigh] = 16,
        [PuyoHopType.Giant] = 24,
        [PuyoHopType.Dropping] = 32,
        [PuyoHopType.DroppedSmall] = 40,
        [PuyoHopType.DroppedBig] = 48,
    });

    /// <summary>The routine word naming each <see cref="PuyoAction"/>.</summary>
    internal static readonly NativeWordIdentities<PuyoAction> PuyoActions = new(new Dictionary<PuyoAction, ushort>
    {
        [PuyoAction.Grounded] = 0x9b65,
        [PuyoAction.Airborne] = 0x9b81,
    });
}
