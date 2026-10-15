namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Explicit function-pointer phase for the currently translated grapple route.</summary>
public enum GrapplePhase
{
    /// <summary>The grapple beam is inactive.</summary>
    Inactive,
    /// <summary>The beam is extending from Samus's hand.</summary>
    Firing,
    /// <summary>The firing beam has requested cancellation.</summary>
    CancelPending,
    /// <summary>Samus is swinging from a connected grapple anchor.</summary>
    ConnectedSwinging,
    /// <summary>Connected swinging has requested release.</summary>
    ReleaseFromSwing,
    /// <summary>Samus is held at a locked grapple angle.</summary>
    ConnectedLocked,
    /// <summary>Samus is holding a grapple wall-grab pose.</summary>
    WallGrab,
    /// <summary>The wall-grab connection is being released.</summary>
    WallGrabRelease,
    /// <summary>The independent grapple wall-jump movement is active.</summary>
    WallJumping,
    /// <summary>The grapple connection was dropped without a swing release.</summary>
    Dropped,
}
