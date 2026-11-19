using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Verification's native view of the Puyo identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class PuyoNativeWords
{
    /// <summary>The routine word naming each <see cref="PuyoAirborneMovement"/>.</summary>
    internal static readonly NativeWordIdentities<PuyoAirborneMovement> PuyoAirborneMovements = new(new Dictionary<PuyoAirborneMovement, ushort>
    {
        [PuyoAirborneMovement.NormalShortHop] = 0x9d0b,
        [PuyoAirborneMovement.NormalBigHop] = 0x9d2b,
        [PuyoAirborneMovement.NormalLongHop] = 0x9d4b,
        [PuyoAirborneMovement.GiantHop] = 0x9d6b,
        [PuyoAirborneMovement.Dropping] = 0x9d98,
        [PuyoAirborneMovement.Dropped] = 0x9dcd,
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

    /// <summary>
    /// The routine word naming each <see cref="PuyoAction"/>: the airborne routine names the rise, the first half of the
    /// hop; the cartridge's falling flag tells the fall from it (#2512).
    /// </summary>
    internal static readonly NativeWordIdentities<PuyoAction> PuyoActions = new(new Dictionary<ushort, PuyoAction>
    {
        [0x9b65] = PuyoAction.Grounded,
        [0x9b81] = PuyoAction.Rising,
    });
}
