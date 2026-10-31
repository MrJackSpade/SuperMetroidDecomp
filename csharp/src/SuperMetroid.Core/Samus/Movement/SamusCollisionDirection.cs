namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The direction a Samus collision probe moves in.</summary>
public enum SamusCollisionDirection
{
    /// <summary>Probes toward decreasing room X using the current left edge.</summary>
    Left,
    /// <summary>Probes toward increasing room X using the current right edge.</summary>
    Right,
    /// <summary>Probes toward decreasing room Y using the current top edge.</summary>
    Up,
    /// <summary>Probes toward increasing room Y using the current bottom edge.</summary>
    Down,
    /// <summary>The pose-observation probe; not an actual contact in any direction.</summary>
    NonDirectionalProbe,
}
