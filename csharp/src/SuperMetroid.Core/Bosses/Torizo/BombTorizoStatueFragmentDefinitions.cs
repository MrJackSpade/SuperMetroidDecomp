namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Fixed projectile definitions for Bomb Torizo's breaking statue hand.</summary>
internal static class BombTorizoStatueFragmentDefinitions
{
    /// <summary>The fragments the breaking statue hand emits.</summary>
    internal const int FragmentCount = 16;

    /// <summary>Returns the definition of <paramref name="fragment"/>, from zero to <see cref="FragmentCount"/> minus one.
    /// The initializer at $86:A764 reads program pointers at $86:A7AB, X offsets at
    /// $86:A7CB, and eight wrapping Y/velocity/acceleration rows at $86:A7EB/A7FB/A80B.</summary>
    internal static BombTorizoStatueFragmentDefinition ForFragment(int fragment)
    {
        if ((uint)fragment >= FragmentCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fragment),
                $"Bomb Torizo statue fragment {fragment} is outside the sixteen fragments.");
        }

        // A three-by-three grid of sixteen-pixel cells, omitting the first corner.
        // The second eight-fragment set mirrors X about the middle column (x = 8).
        int cell = fragment % 8 + 1;
        int x = 16 * (cell % 3) - 8;
        if (fragment >= 8) x = 16 - x;
        return new(
            TorizoProjectileAnimations.StatueFragment(fragment),
            new Offset16<RoomPixels>((short)x, (short)(16 * (cell / 3) - 8)),
            YVelocity: 0x0100,
            Acceleration: 0x0010);
    }
}
