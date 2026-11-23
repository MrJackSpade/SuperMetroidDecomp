namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The course a downward gate's animation runs (<see cref="DownwardGateObject"/>): what the gate's sequence is showing, and,
/// once the closed or open gate has shown for its update and nothing follows, which way the resting gate stands (#2553).
/// </summary>
internal enum DownwardGatePhase
{
    /// <summary>Spawned: its first update shows the closed gate.</summary>
    Starting,
    /// <summary>The closed gate shows; after its update the gate clears its trigger and rests closed, until it is triggered or Samus is in its column.</summary>
    Closed,
    /// <summary>Rising, image by image.</summary>
    Reopening,
    /// <summary>The open gate shows; after its update the gate clears its trigger and rests open, until it is triggered.</summary>
    Open,
    /// <summary>Lowering, image by image.</summary>
    Closing,
}
