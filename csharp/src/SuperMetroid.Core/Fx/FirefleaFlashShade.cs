namespace SuperMetroid.Core.Fx;

/// <summary>
/// One look of the Fireflea room's flash (#2367): the grey, in fade levels, the flash adds to the darkness the room
/// subtracts, from none at its darkest to <see cref="Brightest"/> at its brightest.
/// </summary>
public readonly record struct FirefleaFlashShade
{
    /// <summary>The flash's brightest look, which the room holds once enough Firefleas are killed.</summary>
    public static FirefleaFlashShade Brightest { get; } = new(6);

    /// <summary>Names the look that adds <paramref name="level"/> fade levels of grey.</summary>
    /// <param name="level">The fade levels the look adds, none through six.</param>
    /// <exception cref="ArgumentOutOfRangeException">The flash has no such look.</exception>
    public FirefleaFlashShade(int level) => Level = Ensure.BetweenInclusive(0, 6, level);

    /// <summary>The fade levels of grey the look adds to the room's darkness.</summary>
    public int Level { get; }
}
