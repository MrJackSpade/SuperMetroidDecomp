using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>The resident station family that owns access animation, resource changes, and frontend activation events.</summary>
public enum StationKind : byte
{
    /// <summary>Acquires the current area's map and keeps Samus input locked until map-screen unpause releases it.</summary>
    Map,
    /// <summary>Refills Samus health to its current maximum after the access arm's extension hold.</summary>
    Energy,
    /// <summary>Refills ordinary Missiles to their current maximum after the access arm's extension hold.</summary>
    Missile,
    /// <summary>Uses the floor trigger to request confirmation, then runs the save-pod animation and completion-message handshake.</summary>
    Save,
}
