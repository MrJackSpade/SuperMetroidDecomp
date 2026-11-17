namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The authored timing of the Bomb Torizo hand (#1554): the intact hand shows for one update and then rests until Samus
/// has Bombs; woken, it holds, then drops eight fragments at quickening intervals before the cleared hand and the boss
/// music. The fragments show their own installed pictures (#1406).
/// </summary>
internal static class BombTorizoHandCrumbleDefinitions
{
    /// <summary>How long the woken hand holds before it crumbles.</summary>
    internal const int CrumbleDelay = 120;

    /// <summary>What the resting hand shows on its first update.</summary>
    internal const BombTorizoHandDraw RestingImage = BombTorizoHandDraw.Intact;

    /// <summary>
    /// The woken hand's crumble, in order; the boss music follows its last step. The hand holds, then drops a piece before
    /// each of its waits: a long first wait and a shorter second, then each wait one update quicker.
    /// </summary>
    internal static IReadOnlyList<BombTorizoHandCrumbleStep> Crumble { get; } =
    [
        new(BombTorizoHandDraw.Intact, CrumbleDelay, null),
        new(BombTorizoHandDraw.Intact, 96, null),
        new(BombTorizoHandDraw.Intact, 48, BombTorizoStatueFragment.TopMiddle),
        new(BombTorizoHandDraw.Intact, 15, BombTorizoStatueFragment.TopRight),
        new(BombTorizoHandDraw.Intact, 14, BombTorizoStatueFragment.MiddleLeft),
        new(BombTorizoHandDraw.Intact, 13, BombTorizoStatueFragment.Centre),
        new(BombTorizoHandDraw.Intact, 12, BombTorizoStatueFragment.MiddleRight),
        new(BombTorizoHandDraw.Intact, 11, BombTorizoStatueFragment.BottomLeft),
        new(BombTorizoHandDraw.Intact, 10, BombTorizoStatueFragment.BottomMiddle),
        new(BombTorizoHandDraw.Cleared, 1, BombTorizoStatueFragment.BottomRight),
    ];
}