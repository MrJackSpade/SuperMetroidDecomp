namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One statue fragment the breaking Bomb Torizo hand spawns at its block.</summary>
/// <param name="Fragment">
/// Which of the sixteen fragments, from zero to <see cref="BombTorizoStatueFragmentDefinitions.FragmentCount"/> minus
/// one; it selects the fragment's program, placement and fall.
/// </param>
/// <param name="PlmBlock">The block the hand occupies, which the fragment's placement is relative to.</param>
public readonly record struct BombTorizoStatueProjectileRequest(
    int Fragment,
    Position16<RoomBlocks> PlmBlock);
