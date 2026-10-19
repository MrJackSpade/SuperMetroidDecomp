using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>Cartridge values and helpers of <see cref="ChozoStatuePlmRomData"/> that only verification reads.</summary>
internal static class ChozoStatuePlmRomDataConstants
{
    /// <summary>$84:D15C, wait for blank air at (4,8), then install Lower Norfair BTS $83.</summary>
    public const ushort WaitForLowerNorfairHand = 0xd15c;
}
