using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>An item a suit- or boot-column control stands for.</summary>
/// <param name="Item">The item.</param>
public sealed record ItemPauseUpgrade(SamusEquipmentFlags Item) : PauseUpgrade
{
    /// <inheritdoc/>
    public override bool IsIn(SamusBeamFlags beams, SamusEquipmentFlags items) => (items & Item) != SamusEquipmentFlags.None;

    /// <inheritdoc/>
    public override SamusBeamFlags AddedTo(SamusBeamFlags beams) => beams;

    /// <inheritdoc/>
    public override SamusEquipmentFlags AddedTo(SamusEquipmentFlags items) => items | Item;

    /// <inheritdoc/>
    internal override void ToggleEquipped(SamusState samus) => Ensure.NotNull(samus).EquippedItems ^= Item;
}
