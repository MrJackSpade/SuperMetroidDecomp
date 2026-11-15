using static SuperMetroid.Core.Bosses.Torizo.BombTorizoStatueFragmentDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Places the fragments of Bomb Torizo's breaking statue hand.</summary>
internal static class BombTorizoStatueFragments
{
    /// <summary>Returns the definition of <paramref name="fragment"/>, from zero to <see cref="FragmentCount"/> minus one:
    /// its animation, its place on the hand and its fall.</summary>
    internal static BombTorizoStatueFragmentDefinition ForFragment(int fragment)
    {
        if ((uint)fragment >= FragmentCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fragment),
                $"Bomb Torizo statue fragment {fragment} is outside the {FragmentCount} fragments.");
        }

        // A three-by-three grid of sixteen-pixel cells, omitting the first corner.
        int cell = fragment + 1;
        return new(
            TorizoProjectileAnimations.StatueFragment(fragment),
            new Offset16<RoomPixels>(16 * (cell % 3) - 8, 16 * (cell / 3) - 8),
            YVelocity,
            Acceleration);
    }
}
