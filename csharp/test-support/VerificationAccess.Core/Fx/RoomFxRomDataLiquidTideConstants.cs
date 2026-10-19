using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>Cartridge values and helpers of <see cref="RoomFxRomData.LiquidTide"/> that only verification reads.</summary>
internal static class RoomFxRomDataLiquidTideConstants
{
        /// <summary>Liquid-options bit seven selects the ±8-pixel tide.</summary>
        public const ushort SmallTideOption = 0x0080;
        /// <summary>Liquid-options bit six selects the ±32-pixel tide.</summary>
        public const ushort LargeTideOption = 0x0040;
}
