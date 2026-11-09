namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Where a downward gate is in its course (<see cref="DownwardGateObject"/>).</summary>
internal enum DownwardGatePhase
{
    /// <summary>Spawned: its first update shows the closed gate.</summary>
    Starting,
    /// <summary>The closed gate shows; the next update clears its trigger and lets it rest closed.</summary>
    ShowingClosed,
    /// <summary>Closed, until it is triggered or Samus is in its column.</summary>
    RestingClosed,
    /// <summary>Rising, image by image.</summary>
    Reopening,
    /// <summary>The open gate shows; the next update clears its trigger and lets it rest open.</summary>
    ShowingOpen,
    /// <summary>Open, until it is triggered.</summary>
    RestingOpen,
    /// <summary>Lowering, image by image.</summary>
    Closing,
}
