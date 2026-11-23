namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a Spazer and Plasma beam reads past Samus's firing tables (#1500, #1506). The beam speed table and the three
/// cooldown rows author the twelve beam combinations the game's equipment can hold; a combination that pairs Spazer with
/// Plasma, which only the pause screen's same-frame toggle can equip, reads past them: its launch speeds from the
/// missile data after the speed table, its uncharged and charged cooldowns from the zeros after their rows, and
/// its held-fire cooldown from the projectile sound table after the held-fire row. The import pipeline decodes each
/// such combination's values once.
/// </summary>
/// <remarks>
/// The speeds are consulted when such a beam is fired, and when a reflection relaunches it, as its velocity is set from
/// its direction; a cooldown is consulted when such a beam is fired, as the shared shot timer is set. Evidence: the beam
/// initializer indexes the speed table by the four-bit combination times four bytes, and the fire routines index each
/// cooldown row by the combination's byte; the speed and cooldown checks compare every combination with the original
/// tables and the bytes after them.
/// </remarks>
/// <typeparam name="TBeams">A beam combination.</typeparam>
public sealed partial class UnauthoredBeamFiring<TBeams>
    where TBeams : notnull
{
    /// <summary>Each combination's horizontal and vertical launch speed.</summary>
    private readonly IReadOnlyDictionary<TBeams, SignedEightEight> _cardinal;

    /// <summary>Each combination's diagonal launch speed.</summary>
    private readonly IReadOnlyDictionary<TBeams, SignedEightEight> _diagonal;

    /// <summary>Each combination's cooldowns.</summary>
    private readonly IReadOnlyDictionary<TBeams, BeamShotCooldowns> _cooldowns;

    /// <summary>Keeps the decoded speeds and cooldowns.</summary>
    /// <param name="cardinal">Each combination's horizontal and vertical launch speed.</param>
    /// <param name="diagonal">Each combination's diagonal launch speed.</param>
    /// <param name="cooldowns">Each combination's cooldowns, each zero or more updates.</param>
    /// <exception cref="ArgumentOutOfRangeException">A cooldown is outside its range.</exception>
    /// <exception cref="ArgumentException">The three sets name different combinations.</exception>
    public UnauthoredBeamFiring(
        IReadOnlyDictionary<TBeams, SignedEightEight> cardinal,
        IReadOnlyDictionary<TBeams, SignedEightEight> diagonal,
        IReadOnlyDictionary<TBeams, BeamShotCooldowns> cooldowns)
    {
        Ensure.NotNull(cardinal);
        Ensure.NotNull(diagonal);
        Ensure.NotNull(cooldowns);
        if (cardinal.Count != diagonal.Count || cardinal.Count != cooldowns.Count ||
            cardinal.Keys.Any(beams => !diagonal.ContainsKey(beams) || !cooldowns.ContainsKey(beams)))
            throw new ArgumentException("Every combination needs launch speeds and cooldowns.", nameof(cooldowns));
        foreach (BeamShotCooldowns combination in cooldowns.Values)
        {
            Ensure.AtLeastZero(combination.Uncharged);
            Ensure.AtLeastZero(combination.Charged);
            Ensure.AtLeastZero(combination.HeldFire);
        }
        _cardinal = cardinal;
        _diagonal = diagonal;
        _cooldowns = cooldowns;
    }

    /// <summary>
    /// The speed <paramref name="beams"/> launches at: its diagonal speed when <paramref name="diagonal"/>, otherwise its
    /// horizontal and vertical one.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed table authors the combination's speeds.</exception>
    public SignedEightEight Speed(TBeams beams, bool diagonal) =>
        (diagonal ? _diagonal : _cardinal).TryGetValue(beams, out SignedEightEight speed)
            ? speed
            : throw new ArgumentOutOfRangeException(nameof(beams), beams, "The beam speed table authors this combination's launch speeds.");

    /// <summary>The updates the shared shot timer waits after <paramref name="beams"/> fires a <paramref name="shot"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cooldown rows author the combination's cooldowns.</exception>
    public int Cooldown(TBeams beams, BeamShotCooldowns.Shot shot) =>
        (_cooldowns.TryGetValue(beams, out BeamShotCooldowns? cooldowns)
            ? cooldowns
            : throw new ArgumentOutOfRangeException(nameof(beams), beams, "The cooldown rows author this combination's cooldowns.")).For(shot);
}
