namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// One control of the equipment page the cursor rests on (#1772): a reserve-tank control or an upgrade's label. Its
/// category is the column it stands in and chooses the selector's picture; its anchor names the installed selector
/// position. The controls are <see cref="PauseEquipmentControls"/>'s instances, each column an ordered list of them; the
/// cursor holds a control and moves between list neighbours.
/// </summary>
public abstract class PauseEquipmentControl
{
    /// <summary>Names a control of <paramref name="category"/> whose selector stands at the installed anchor <paramref name="anchor"/>.</summary>
    private protected PauseEquipmentControl(PauseEquipmentCategory category, string anchor)
    {
        Category = category;
        Anchor = anchor;
    }

    /// <summary>The column the control stands in.</summary>
    public PauseEquipmentCategory Category { get; }

    /// <summary>The stable name of the control's installed selector anchor, which also keys an upgrade's installed label.</summary>
    public string Anchor { get; }

    /// <inheritdoc/>
    public override string ToString() => Anchor;
}
