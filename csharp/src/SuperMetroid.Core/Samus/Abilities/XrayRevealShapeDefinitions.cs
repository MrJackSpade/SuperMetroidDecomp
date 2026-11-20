using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Each X-ray revealed-block shape the game reaches.</summary>
internal static class XrayRevealShapeDefinitions
{
    /// <summary>
    /// Each shape: the two extensions, which copy no blocks; the copies of one, two side by side, two one above the other and
    /// a square of four blocks; and the copy of one that Brinstar's own Speed Booster blocks reveal, in Brinstar only (#1805).
    /// </summary>
    internal static readonly IReadOnlyDictionary<XrayRevealShape, XrayRevealShapeDefinition> Shapes = new Dictionary<XrayRevealShape, XrayRevealShapeDefinition>
    {
        [XrayRevealShape.VerticalExtension] = new(CopiedBlocks: 0),
        [XrayRevealShape.HorizontalExtension] = new(CopiedBlocks: 0),
        [XrayRevealShape.CopyOne] = new(CopiedBlocks: 1),
        [XrayRevealShape.CopyBrinstar] = new(CopiedBlocks: 1, OnlyIn: AreaId.Brinstar),
        [XrayRevealShape.CopyWide] = new(CopiedBlocks: 2),
        [XrayRevealShape.CopyTall] = new(CopiedBlocks: 2),
        [XrayRevealShape.CopySquare] = new(CopiedBlocks: 4),
    };
}
