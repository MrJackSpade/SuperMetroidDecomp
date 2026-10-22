using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One write of a scroll program: room scroll <paramref name="Index"/> takes <paramref name="State"/>.</summary>
/// <param name="Index">The room scroll the write sets, in the scroll grid's storage order.</param>
/// <param name="State">The state the scroll takes.</param>
internal readonly record struct RoomScrollWrite(int Index, RoomScrollState State);
