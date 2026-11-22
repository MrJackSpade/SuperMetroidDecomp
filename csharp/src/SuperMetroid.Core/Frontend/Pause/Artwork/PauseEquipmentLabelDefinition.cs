namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>One of the equipment screen's item labels (#1467): how wide its picture is stored and shown, and how Hyper treats it.</summary>
/// <param name="StoredWidth">The strip width the label's picture is stored at.</param>
/// <param name="ShownWidth">The strip width the label covers on the equipment page.</param>
/// <param name="ReplacedByHyper">Whether the Hyper label shows in its place while Samus fires the Hyper Beam: the ordinary beam labels.</param>
/// <param name="BorrowsPageCells">Whether the label is written over another label's page cells rather than cells of its own.</param>
[IndependentFlag("independent facts of one label: whether Hyper Beam replaces it and whether it is written over another label's page cells")]
internal sealed partial record PauseEquipmentLabelDefinition(
    PauseEquipmentLabelWidth StoredWidth,
    PauseEquipmentLabelWidth ShownWidth,
    bool ReplacedByHyper,
    bool BorrowsPageCells);
