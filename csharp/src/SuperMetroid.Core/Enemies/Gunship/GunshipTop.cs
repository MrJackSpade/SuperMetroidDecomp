namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// What the gunship's top hull holds from its setup (#2554): the pieces it found, its own hull's animation and the ship's
/// state, which it runs.
/// </summary>
/// <param name="Pieces">The ship's three pieces, with the entrance pad's animation the top hull starts and steps.</param>
/// <param name="Hull">The top hull's own animation.</param>
/// <param name="Ship">The ship's phase, timers and motion.</param>
internal sealed partial record GunshipTop(GunshipPieces Pieces, GunshipPieceState Hull, GunshipState Ship);
