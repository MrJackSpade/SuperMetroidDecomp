using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// An upgrade's control on the equipment page (#1772): the upgrade A toggles while the cursor rests on it, and the label
/// the page shows in that upgrade's state.
/// </summary>
public sealed class PauseUpgradeControl : PauseEquipmentControl
{
    /// <summary>Names the control of <paramref name="upgrade"/> in <paramref name="category"/>, shown by <paramref name="label"/>.</summary>
    internal PauseUpgradeControl(PauseEquipmentCategory category, string anchor, PauseEquipmentBit upgrade, PauseEquipmentLabelId label)
        : base(category, anchor)
    {
        Upgrade = upgrade;
        Label = label;
    }

    /// <summary>The upgrade the control tests and toggles.</summary>
    public PauseEquipmentBit Upgrade { get; }

    /// <summary>The label the page shows for the upgrade.</summary>
    public PauseEquipmentLabelId Label { get; }
}
