namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>How the equipment page shows an item's label (#1467, #1515): the state the item is in.</summary>
internal enum PauseEquipmentLabelState
{
    /// <summary>The blank strip an uncollected item shows.</summary>
    Blank,
    /// <summary>The label in its own colours: the item is collected and equipped.</summary>
    Enabled,
    /// <summary>The label recoloured grey: the item is collected but not equipped.</summary>
    Disabled,
}
