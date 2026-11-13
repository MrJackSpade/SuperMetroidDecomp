namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The authored timing of the Bomb Torizo hand (#1554): the intact hand shows for one update and then rests until Samus
/// has Bombs; woken, it holds, then drops eight fragments at quickening intervals before the cleared hand and the boss
/// music. The game's crumble also prepares the debris pictures the fragments are drawn with (#1406); the fragments show
/// their own installed pictures, so that preparation has no step here and takes no time.
/// </summary>
internal static class BombTorizoHandCrumbleDefinitions
{
    /// <summary>How long the woken hand holds before it crumbles.</summary>
    internal const int CrumbleDelay = 120;

    /// <summary>How long the hand shows before each fragment drops: a long first wait and a shorter second, then each wait one update quicker.</summary>
    internal static IReadOnlyList<int> FragmentDelays { get; } = [96, 48, 15, 14, 13, 12, 11, 10];

    /// <summary>What the resting hand shows on its first update.</summary>
    internal const BombTorizoHandDraw RestingImage = BombTorizoHandDraw.Intact;

    /// <summary>The woken hand's crumble, in order; the boss music follows its last step.</summary>
    internal static IReadOnlyList<BombTorizoHandCrumbleStep> Crumble { get; } =
    [
        new(BombTorizoHandDraw.Intact, CrumbleDelay, null),
        new(BombTorizoHandDraw.Intact, FragmentDelays[0], null),
        new(BombTorizoHandDraw.Intact, FragmentDelays[1], 0),
        new(BombTorizoHandDraw.Intact, FragmentDelays[2], 1),
        new(BombTorizoHandDraw.Intact, FragmentDelays[3], 2),
        new(BombTorizoHandDraw.Intact, FragmentDelays[4], 3),
        new(BombTorizoHandDraw.Intact, FragmentDelays[5], 4),
        new(BombTorizoHandDraw.Intact, FragmentDelays[6], 5),
        new(BombTorizoHandDraw.Intact, FragmentDelays[7], 6),
        new(BombTorizoHandDraw.Cleared, 1, 7),
    ];
}
