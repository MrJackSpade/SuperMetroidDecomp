namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Each of the equipment screen's item labels (#1467).</summary>
internal static class PauseEquipmentLabelDefinitions
{
    /// <summary>An ordinary beam label: a beam strip, which the Hyper label replaces.</summary>
    private static readonly PauseEquipmentLabelDefinition OrdinaryBeam =
        new(PauseEquipmentLabelWidth.Beam, PauseEquipmentLabelWidth.Beam, ReplacedByHyper: true, BorrowsPageCells: false);

    /// <summary>An equipment label: an equipment strip of its own.</summary>
    private static readonly PauseEquipmentLabelDefinition Equipment =
        new(PauseEquipmentLabelWidth.Equipment, PauseEquipmentLabelWidth.Equipment, ReplacedByHyper: false, BorrowsPageCells: false);

    /// <summary>
    /// Each label: the five ordinary beam labels; the equipment labels; and Hyper, stored as wide as an equipment label but
    /// shown over a beam strip in the beam labels' cells.
    /// </summary>
    internal static readonly IReadOnlyDictionary<PauseEquipmentLabelId, PauseEquipmentLabelDefinition> Labels =
        new Dictionary<PauseEquipmentLabelId, PauseEquipmentLabelDefinition>
        {
            [PauseEquipmentLabelId.Charge] = OrdinaryBeam,
            [PauseEquipmentLabelId.Ice] = OrdinaryBeam,
            [PauseEquipmentLabelId.Wave] = OrdinaryBeam,
            [PauseEquipmentLabelId.Spazer] = OrdinaryBeam,
            [PauseEquipmentLabelId.Plasma] = OrdinaryBeam,
            [PauseEquipmentLabelId.Varia] = Equipment,
            [PauseEquipmentLabelId.Gravity] = Equipment,
            [PauseEquipmentLabelId.MorphBall] = Equipment,
            [PauseEquipmentLabelId.Bombs] = Equipment,
            [PauseEquipmentLabelId.SpringBall] = Equipment,
            [PauseEquipmentLabelId.ScrewAttack] = Equipment,
            [PauseEquipmentLabelId.HiJump] = Equipment,
            [PauseEquipmentLabelId.SpaceJump] = Equipment,
            [PauseEquipmentLabelId.SpeedBooster] = Equipment,
            [PauseEquipmentLabelId.Hyper] = new(PauseEquipmentLabelWidth.Equipment, PauseEquipmentLabelWidth.Beam, ReplacedByHyper: false, BorrowsPageCells: true),
        };
}
