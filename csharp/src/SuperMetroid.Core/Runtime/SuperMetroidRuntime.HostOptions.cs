using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Updates live host guards without reconstructing any cartridge-owned state.</summary>
    internal void ApplyHostOptions(SuperMetroidGameOptions options)
    {
        if (!Enum.IsDefined(options.MapReveal))
            throw new ArgumentOutOfRangeException(nameof(options), "Unknown map reveal mode.");
        PlayerInvincibilityEnabled = options.Invincibility;
        InfiniteAmmoEnabled = options.InfiniteAmmo;
        PreventEscapeTimeout = options.PreventEscapeTimeout;
        MapRevealMode = options.MapReveal;
    }
}
