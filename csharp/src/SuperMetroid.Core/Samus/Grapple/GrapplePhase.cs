namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The grapple beam's phase.</summary>
public enum GrapplePhase
{
    /// <summary>The grapple beam is inactive.</summary>
    Inactive,
    /// <summary>The beam is extending from Samus's hand.</summary>
    Firing,
    /// <summary>
    /// The grapple has requested cancellation that goes on beside Samus's ordinary movement: a firing beam's, or a locked
    /// connection's while Draygon holds her.
    /// </summary>
    CancelPending,
    /// <summary>
    /// A locked connection has requested cancellation from its connected pose (#2560), which holds this movement phase until
    /// her pose's no-input fallback takes effect.
    /// </summary>
    CancelPendingFromConnectedPose,
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
