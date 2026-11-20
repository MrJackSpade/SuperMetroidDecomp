namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship's top hull as its setup builds it (#2554): the ship's pieces, the hull's own animation and the ship's state.</summary>
/// <param name="Pieces">The ship's three pieces, which glow and move together.</param>
/// <param name="Piece">The top hull's animation.</param>
/// <param name="Ship">The state of the whole ship, which the top hull moves.</param>
internal sealed record GunshipTop(GunshipPieces Pieces, GunshipPieceState Piece, GunshipState Ship);
