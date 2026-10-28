namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Bank-$9B grapple functions a connection record installs after the pose handler runs.</summary>
public enum GrappleConnectionFunction
{
    /// <summary>Bank-$9B locked-in-place connection handler at $9B:C77E.</summary>
    LockedInPlace,
    /// <summary>Bank-$9B ordinary swinging connection handler at $9B:C79D.</summary>
    Swinging,
    /// <summary>Bank-$9B wall-grab connection handler at $9B:C814.</summary>
    WallGrab,
}
