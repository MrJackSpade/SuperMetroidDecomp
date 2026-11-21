namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// Says which of the equipment screen's item labels (#1467) are the ordinary beam labels Hyper replaces; each upgrade's
/// label is its control's (<see cref="PauseUpgradeControl.Label"/>), and the import pictures each label whole.
/// </summary>
public static class PauseEquipmentLabels
{
    /// <summary>Whether a label is one of the five ordinary beam labels, which Hyper replaces.</summary>
    internal static bool IsOrdinaryBeam(PauseEquipmentLabelId label) => PauseEquipmentLabelDefinitions.Labels[label].ReplacedByHyper;
}
