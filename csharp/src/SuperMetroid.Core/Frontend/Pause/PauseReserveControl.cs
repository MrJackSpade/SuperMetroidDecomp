namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>A control of the equipment page's reserve-tank column (#1772): the AUTO/MANUAL mode switch or the manual transfer.</summary>
public sealed class PauseReserveControl : PauseEquipmentControl
{
    /// <summary>Names a reserve control whose selector stands at the installed anchor <paramref name="anchor"/>.</summary>
    internal PauseReserveControl(string anchor) : base(PauseEquipmentCategory.Reserves, anchor)
    {
    }
}
