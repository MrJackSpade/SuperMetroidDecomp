namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// How a block the X-ray reveal shows lays out its picture (#1392, #1431): as the block is mirrored, whether it is a copied
/// block of the level or an item or room overlay's block (#2047).
/// </summary>
public enum RevealedBlockArrangement
{
    /// <summary>As the definition gives them: a reveal block, or a block that is not mirrored.</summary>
    AsDefined,

    /// <summary>Mirrored left to right: a block mirrored so.</summary>
    MirroredHorizontally,

    /// <summary>Mirrored top to bottom: a block mirrored so.</summary>
    MirroredVertically,

    /// <summary>Mirrored both ways: a block mirrored so.</summary>
    MirroredBoth,
}
