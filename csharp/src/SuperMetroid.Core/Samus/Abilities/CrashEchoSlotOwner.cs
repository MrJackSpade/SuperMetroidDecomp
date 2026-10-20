namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Who owns a released shinespark crash echo's slot.</summary>
public enum CrashEchoSlotOwner
{
    /// <summary>The echo owns its slot and ends when the slot is inactive.</summary>
    Echo,

    /// <summary>A projectile owns the slot.</summary>
    Projectile,
}
