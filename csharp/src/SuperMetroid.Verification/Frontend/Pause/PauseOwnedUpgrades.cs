using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>The beams and items a pause-equipment fixture owns.</summary>
/// <param name="Beams">The beams owned.</param>
/// <param name="Items">The items owned.</param>
internal readonly record struct PauseOwnedUpgrades(SamusBeamFlags Beams, SamusEquipmentFlags Items)
{
    /// <summary>No beam or item.</summary>
    internal static PauseOwnedUpgrades None { get; } = new(SamusBeamFlags.None, SamusEquipmentFlags.None);

    /// <summary>Only <paramref name="upgrade"/>.</summary>
    internal static PauseOwnedUpgrades Only(PauseUpgrade upgrade) => None.With(upgrade);

    /// <summary>These upgrades with <paramref name="upgrade"/> added.</summary>
    internal PauseOwnedUpgrades With(PauseUpgrade upgrade) => new(upgrade.AddedTo(Beams), upgrade.AddedTo(Items));
}
