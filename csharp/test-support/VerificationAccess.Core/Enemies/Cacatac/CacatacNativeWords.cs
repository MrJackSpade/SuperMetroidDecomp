using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Verification's native view of the Cacatac identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class CacatacNativeWords
{
    /// <summary>The routine word naming each <see cref="CacatacAction"/>.</summary>
    internal static readonly NativeWordIdentities<CacatacAction> CacatacActions = new(new Dictionary<CacatacAction, ushort>
    {
        [CacatacAction.MovingLeft] = 0x9fba,
        [CacatacAction.MovingRight] = 0x9fec,
        [CacatacAction.Stopped] = 0xa01b,
    });

    /// <summary>The direction word naming each <see cref="CacatacDirection"/>, as the placement and the enemy's state hold it.</summary>
    internal static readonly NativeWordIdentities<CacatacDirection> Directions = new(new Dictionary<CacatacDirection, ushort>
    {
        [CacatacDirection.Left] = 0,
        [CacatacDirection.Right] = 1,
        [CacatacDirection.InitialStop] = 2,
    });

    /// <summary>
    /// The even byte offset naming each <see cref="CacatacSpikeDirection"/> in the spike spawn command and the spike
    /// instruction and speed tables; offsets from <c>$0C</c> on are the diagonals.
    /// </summary>
    internal static readonly NativeWordIdentities<CacatacSpikeDirection> SpikeDirections = new(new Dictionary<CacatacSpikeDirection, ushort>
    {
        [CacatacSpikeDirection.LeftFacingUp] = 0x00,
        [CacatacSpikeDirection.Up] = 0x02,
        [CacatacSpikeDirection.RightFacingUp] = 0x04,
        [CacatacSpikeDirection.LeftFacingDown] = 0x06,
        [CacatacSpikeDirection.Down] = 0x08,
        [CacatacSpikeDirection.RightFacingDown] = 0x0a,
        [CacatacSpikeDirection.UpLeft] = 0x0c,
        [CacatacSpikeDirection.UpRight] = 0x0e,
        [CacatacSpikeDirection.DownLeft] = 0x10,
        [CacatacSpikeDirection.DownRight] = 0x12,
    });
}
