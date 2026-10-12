using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>The bank-$84 instruction lists and draw a grey door runs through its lifetime (#633).</summary>
internal readonly record struct GreyDoorPrograms(
    ushort InitialList, ushort ClosedBlueList, ushort ClosedGreyDraw, ushort FlashList, ushort OpeningList);
