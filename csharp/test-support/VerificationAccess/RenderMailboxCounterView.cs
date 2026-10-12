using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>One consistent read of a render mailbox's private publication counters.</summary>
[PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
internal readonly record struct RenderMailboxCounterView(
    long Generation,
    long Published,
    long Taken,
    long Replaced,
    long Invalidated,
    bool HasPendingFrame);
