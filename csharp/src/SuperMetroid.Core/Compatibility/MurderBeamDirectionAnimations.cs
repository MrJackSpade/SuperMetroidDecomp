namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The animation the Murder Beam's shot runs in each direction (#1517). The charged shot of the combination of all four
/// beams, Spazer with Plasma, which only the pause screen's same-frame toggle can equip, takes the Power Bomb's damage
/// (<see cref="UnauthoredBeamShots{TBeams, TProjectile}"/>) and reads its direction's animation from the Power Bomb's
/// definition, which names one animation, and on into the definitions stored after it: fired up it runs the Power Bomb's
/// animation, right the bomb's, down the beam explosion's, down-left the missile explosion's and up-left the bomb
/// explosion's; fired left it reads an empty entry and runs no animation, the one direction in which the shot is
/// harmless; in the other four directions it reads a damage value as an animation, which this rule does not reproduce and
/// rejects.
/// </summary>
/// <remarks>
/// A rule with no state, held by the projectile selection catalog. Consulted when the Murder Beam's shot is fired and when
/// an enemy reflects it. Evidence: the
/// fire routines index the selected kind's animation list by the shot's direction, one entry per direction, and the
/// projectile animation check compares each such entry past the Power Bomb's list with the original's.
/// </remarks>
/// <typeparam name="TDirection">A shot direction.</typeparam>
/// <typeparam name="TAnimation">An animation.</typeparam>
[CompatibilityService("the Murder Beam's shot runs the animation each direction reads from the Power Bomb's program list and the lists stored after it")]
public sealed class MurderBeamDirectionAnimations<TDirection, TAnimation>
    where TDirection : notnull
    where TAnimation : struct
{
    /// <summary>The animation each readable direction runs, or null for the direction that runs none.</summary>
    private readonly IReadOnlyDictionary<TDirection, TAnimation?> _programs;

    /// <summary>Keeps the animation each readable direction runs.</summary>
    /// <param name="animations">The animation each direction whose read names one runs, or null for the direction whose read is empty.</param>
    public MurderBeamDirectionAnimations(IReadOnlyDictionary<TDirection, TAnimation?> animations) => _programs = Ensure.NotNull(animations);

    /// <summary>The animation the shot fired in <paramref name="direction"/> runs, or null for none.</summary>
    /// <exception cref="InvalidDataException">The direction reads a damage value as an animation.</exception>
    public TAnimation? Animation(TDirection direction) =>
        _programs.TryGetValue(direction, out TAnimation? animation)
            ? animation
            : throw new InvalidDataException($"The Murder Beam fired {direction} reads a damage word as its animation.");
}
