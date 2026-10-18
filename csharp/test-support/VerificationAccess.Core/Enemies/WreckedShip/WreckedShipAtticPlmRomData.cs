using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Named bank-$84 callback pointers owned by Wrecked Ship attic PLM $BB05.</summary>
internal static class WreckedShipAtticPlmRomData
{
    /// <summary>
    /// <c>$84:BAFA Setup_WreckedShipAttic</c>. Both the header setup and resident
    /// pre-instruction execute the same intentionally inert REP/SEP/RTS routine.
    /// </summary>
    public const ushort NoOpCallback = 0xbafa;

}
