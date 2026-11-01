namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Grapple functions a connection record installs after the pose handler runs.</summary>
public enum GrappleConnectionFunction
{
    /// <summary>Locked-in-place connection handler.</summary>
    LockedInPlace,
    /// <summary>Ordinary swinging connection handler.</summary>
    Swinging,
    /// <summary>Wall-grab connection handler.</summary>
    WallGrab,
}
