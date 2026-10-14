using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Core.Runtime;

/// <summary>The host's tester conveniences around the cartridge-authentic runtime: invincibility, infinite ammo, map reveal, escape-timeout prevention, and new-session equipment and Tourian unlocks (#633).</summary>
public sealed record HostGameplayOverrides
{
    /// <summary>Retail behavior: no tester convenience is active.</summary>
    public static HostGameplayOverrides None { get; } = new();

    /// <summary>Whether the frame-exit guard prevents lethal player energy.</summary>
    public bool PlayerInvincibilityEnabled { get; init; } = false;

    /// <summary>Whether the frame-exit guard restores depleted unlocked ammunition.</summary>
    public bool InfiniteAmmoEnabled { get; init; } = false;

    /// <summary>Host map-visibility override applied without changing saved exploration bits.</summary>
    public MapRevealMode MapRevealMode { get; init; } = MapRevealMode.None;

    /// <summary>Whether the host suppresses the escape timer's terminal failure.</summary>
    public bool PreventEscapeTimeout { get; init; } = false;

    /// <summary>Whether a new gameplay session receives the host all-equipment override.</summary>
    public bool GrantAllEquipment { get; init; } = false;

    /// <summary>Whether a new gameplay session receives the host Tourian-unlock override.</summary>
    public bool UnlockTourian { get; init; } = false;
}
