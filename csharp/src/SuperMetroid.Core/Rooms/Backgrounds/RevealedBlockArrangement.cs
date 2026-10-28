namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>How a block the X-ray reveal shows lays out its picture (#1392, #1431).</summary>
public enum RevealedBlockArrangement
{
    /// <summary>As the definition gives them: a reveal metatile, or a copied block the level does not mirror.</summary>
    AsDefined,

    /// <summary>Mirrored left to right, each tile's own flip toggled: a copied block the level mirrors so.</summary>
    MirroredHorizontally,

    /// <summary>Mirrored top to bottom, each tile's own flip toggled: a copied block the level mirrors so.</summary>
    MirroredVertically,

    /// <summary>Mirrored both ways, each tile's own flip toggled: a copied block the level mirrors so.</summary>
    MirroredBoth,

    /// <summary>
    /// Its two tile rows swapped and the tiles left unflipped: an item or room overlay the level flips vertically, as
    /// <c>$91:D0A6</c> draws it.
    /// </summary>
    RowsSwapped,
}
