namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The order the cartridge's statue-fragment tables at $86:A7AB list Bomb Torizo's statue pieces in, which its crumble's
/// spawn operands index two bytes a piece (#2029). Only verification reads it; the port names each piece.
/// </summary>
internal static class BombTorizoStatueFragmentNativeOrder
{
    /// <summary>The pieces in table order.</summary>
    internal static IReadOnlyList<BombTorizoStatueFragment> Pieces { get; } =
    [
        BombTorizoStatueFragment.TopMiddle, BombTorizoStatueFragment.TopRight, BombTorizoStatueFragment.MiddleLeft,
        BombTorizoStatueFragment.Centre, BombTorizoStatueFragment.MiddleRight, BombTorizoStatueFragment.BottomLeft,
        BombTorizoStatueFragment.BottomMiddle, BombTorizoStatueFragment.BottomRight,
    ];

    /// <summary>The place of <paramref name="piece"/> in the cartridge's tables.</summary>
    internal static int IndexOf(BombTorizoStatueFragment piece)
    {
        int index = Pieces.ToList().IndexOf(piece);
        return index >= 0 ? index : throw new ArgumentOutOfRangeException(nameof(piece), piece, "No statue piece of the cartridge's tables.");
    }
}
