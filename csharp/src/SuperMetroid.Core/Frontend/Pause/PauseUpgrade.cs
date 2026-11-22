using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The upgrade one pause-menu equipment control stands for (#1323): a beam of the beam column
/// (<see cref="BeamPauseUpgrade"/>) or an item of the suit and boot columns (<see cref="ItemPauseUpgrade"/>).
/// </summary>
public abstract partial record PauseUpgrade
{
    /// <summary>Only the beam and item upgrades derive from it.</summary>
    private protected PauseUpgrade()
    {
    }

    /// <summary>Whether the upgrade is among <paramref name="beams"/> or <paramref name="items"/>.</summary>
    public abstract bool IsIn(SamusBeamFlags beams, SamusEquipmentFlags items);

    /// <summary><paramref name="beams"/> with the upgrade added when it is a beam.</summary>
    public abstract SamusBeamFlags AddedTo(SamusBeamFlags beams);

    /// <summary><paramref name="items"/> with the upgrade added when it is an item.</summary>
    public abstract SamusEquipmentFlags AddedTo(SamusEquipmentFlags items);

    /// <summary>Equips the upgrade when <paramref name="samus"/> has it unequipped, and unequips it otherwise.</summary>
    internal abstract void ToggleEquipped(SamusState samus);
}
