namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>How a block the X-ray reveal shows lays out its picture (#1392, #1431).</summary>
public enum RevealedBlockArrangement
{
    /// <summary>As the definition gives them: a reveal block, or a copied block the level does not mirror.</summary>
    AsDefined,

    /// <summary>Mirrored left to right, each quarter's own flip toggled: a copied block the level mirrors so.</summary>
    MirroredHorizontally,

    /// <summary>Mirrored top to bottom, each quarter's own flip toggled: a copied block the level mirrors so.</summary>
    MirroredVertically,

    /// <summary>Mirrored both ways, each quarter's own flip toggled: a copied block the level mirrors so.</summary>
    MirroredBoth,

    /// <summary>
    /// Its top and bottom halves swapped and the quarters left unflipped: an item or room overlay the level flips vertically, as
    /// the X-ray reveal draws it.
    /// </summary>
    RowsSwapped,
}
