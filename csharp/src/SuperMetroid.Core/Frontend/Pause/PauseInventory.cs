using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The inventory the equipment page is built from (#1467): what Samus has collected and equipped, and Hyper.</summary>
/// <param name="CollectedBeams">The beams collected.</param>
/// <param name="EquippedBeams">The beams equipped.</param>
/// <param name="CollectedItems">The items collected.</param>
/// <param name="EquippedItems">The items equipped.</param>
/// <param name="HyperBeam">Whether Hyper replaces the beams.</param>
internal readonly partial record struct PauseInventory(SamusBeamFlags CollectedBeams, SamusBeamFlags EquippedBeams,
    SamusEquipmentFlags CollectedItems, SamusEquipmentFlags EquippedItems, bool HyperBeam)
{
    /// <summary>How the label of <paramref name="upgrade"/> shows: blank uncollected, else enabled while equipped and disabled otherwise.</summary>
    internal PauseEquipmentLabelState StateOf(PauseUpgrade upgrade) =>
        !upgrade.IsIn(CollectedBeams, CollectedItems) ? PauseEquipmentLabelState.Blank
            : upgrade.IsIn(EquippedBeams, EquippedItems) ? PauseEquipmentLabelState.Enabled : PauseEquipmentLabelState.Disabled;
}
