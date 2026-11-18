namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One run of a downward gate's images (<see cref="DownwardGateAnimations"/>, #2480).</summary>
internal enum DownwardGateSegment
{
    /// <summary>The open gate holding, then lowering image by image.</summary>
    Closing,

    /// <summary>The lowered gate rising image by image.</summary>
    Reopening,

    /// <summary>The closed gate for one update before the gate rests closed.</summary>
    Closed,

    /// <summary>The open gate for one update before the gate rests open.</summary>
    Open,
}
