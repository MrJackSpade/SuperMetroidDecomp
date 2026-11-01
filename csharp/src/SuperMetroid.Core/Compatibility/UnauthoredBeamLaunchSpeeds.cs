namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The speeds a Spazer and Plasma beam launches at (#1500). The beam speed table authors a cardinal and a diagonal speed
/// for the twelve beam combinations retail equipment can hold; a combination that pairs Spazer with Plasma, which only the
/// pause screen's same-frame toggle can equip, reads its pair from the words stored after the table, the missile data that
/// follows it. The import pipeline decodes each such combination's two speeds once.
/// </summary>
/// <remarks>
/// Consulted when such a beam is fired, and when a reflection relaunches it, as its velocity is set from its direction.
/// Evidence: the beam initializer indexes the speed table by the four-bit combination times four bytes, and the speed
/// check compares every combination's launch velocity with the original table and the words after it.
/// </remarks>
/// <typeparam name="TBeams">A beam combination.</typeparam>
[CompatibilityService("a Spazer and Plasma beam launches at the speeds read past the beam speed table, from the missile data after it")]
public sealed class UnauthoredBeamLaunchSpeeds<TBeams>
    where TBeams : notnull
{
    /// <summary>Each combination's horizontal and vertical launch speed, in 256ths of a pixel per update.</summary>
    private readonly IReadOnlyDictionary<TBeams, int> _cardinal;

    /// <summary>Each combination's diagonal launch speed, in 256ths of a pixel per update.</summary>
    private readonly IReadOnlyDictionary<TBeams, int> _diagonal;

    /// <summary>Keeps the decoded speeds, each -32768 through 32767 in 256ths of a pixel per update.</summary>
    /// <param name="cardinal">Each combination's horizontal and vertical launch speed.</param>
    /// <param name="diagonal">Each combination's diagonal launch speed.</param>
    /// <exception cref="ArgumentOutOfRangeException">A speed is outside a signed sixteen-bit range.</exception>
    /// <exception cref="ArgumentException">The two sets name different combinations.</exception>
    public UnauthoredBeamLaunchSpeeds(IReadOnlyDictionary<TBeams, int> cardinal, IReadOnlyDictionary<TBeams, int> diagonal)
    {
        Ensure.NotNull(cardinal);
        Ensure.NotNull(diagonal);
        if (cardinal.Count != diagonal.Count || cardinal.Keys.Any(beams => !diagonal.ContainsKey(beams)))
            throw new ArgumentException("Every combination needs both a cardinal and a diagonal launch speed.", nameof(diagonal));
        foreach (int speed in cardinal.Values.Concat(diagonal.Values))
            Ensure.BetweenInclusive(short.MinValue, short.MaxValue, speed);
        _cardinal = cardinal;
        _diagonal = diagonal;
    }

    /// <summary>
    /// The speed <paramref name="beams"/> launches at, in 256ths of a pixel per update: its diagonal speed when
    /// <paramref name="diagonal"/>, otherwise its horizontal and vertical one.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed table authors the combination's speeds.</exception>
    public int Speed(TBeams beams, bool diagonal) =>
        (diagonal ? _diagonal : _cardinal).TryGetValue(beams, out int speed)
            ? speed
            : throw new ArgumentOutOfRangeException(nameof(beams), beams, "The beam speed table authors this combination's launch speeds.");
}
