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
        if (!GrantAllEquipmentEnabled && options.GrantAllEquipment) testerInventoryRecipient = null;
        GrantAllEquipmentEnabled = options.GrantAllEquipment;
        bool openingTourian = !UnlockTourianEnabled && options.UnlockTourian;
        UnlockTourianEnabled = options.UnlockTourian;
        ApplyTesterInventory();
        if (openingTourian && TourianStatues.Enabled) TourianStatues.Load(this);
        PreventEscapeTimeout = options.PreventEscapeTimeout;
        MapRevealMode = options.MapReveal;
    }
}
