using static SuperMetroid.Core.Bosses.Phantoon.PhantoonDeathExplosionDefinitions;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Reads Phantoon's death explosions in the order they burst: the opening explosions, then the repeated ones, each place
/// in the schedule counted from its first explosion.
/// </summary>
public static class PhantoonDeathExplosions
{
    /// <summary>The places in the schedule: the opening explosions, then the repeated ones once.</summary>
    public static int Count => Opening.Length + Repeated.Length;

    /// <summary>The place the schedule returns to after its last explosion: the first of the repeated explosions.</summary>
    public static int RepeatStart => Opening.Length;

    /// <summary>The explosion at <paramref name="index"/>: an opening explosion, or one of the repeated explosions after them.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The schedule has no such place.</exception>
    public static PhantoonDeathExplosionDefinition Read(int index) =>
        (uint)index < (uint)Opening.Length ? Opening[index]
        : (uint)(index - Opening.Length) < (uint)Repeated.Length ? Repeated[index - Opening.Length]
        : throw new ArgumentOutOfRangeException(nameof(index));
}
