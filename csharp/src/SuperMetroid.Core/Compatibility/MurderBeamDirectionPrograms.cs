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
/// fire routines index the selected header by the shot's direction, one word per direction, and the projectile program
/// check compares each such word after the Power Bomb header with the original's.
/// </remarks>
/// <typeparam name="TDirection">A shot direction.</typeparam>
/// <typeparam name="TProgram">An animation program.</typeparam>
[CompatibilityService("the Murder Beam's shot runs the animation each direction reads from the Power Bomb header and the headers after it")]
public sealed class MurderBeamDirectionPrograms<TDirection, TProgram>
    where TDirection : notnull
    where TProgram : struct
{
    /// <summary>The animation each readable direction runs, or null for the direction that runs none.</summary>
    private readonly IReadOnlyDictionary<TDirection, TProgram?> _programs;

    /// <summary>Keeps the animation each readable direction runs.</summary>
    /// <param name="programs">The animation each direction whose read names one runs, or null for the direction whose read is empty.</param>
    public MurderBeamDirectionPrograms(IReadOnlyDictionary<TDirection, TProgram?> programs) => _programs = Ensure.NotNull(programs);

    /// <summary>The animation the shot fired in <paramref name="direction"/> runs, or null for none.</summary>
    /// <exception cref="InvalidDataException">The direction reads a damage value as an animation.</exception>
    public TProgram? Program(TDirection direction) =>
        _programs.TryGetValue(direction, out TProgram? program)
            ? program
            : throw new InvalidDataException($"The Murder Beam fired {direction} reads a damage word as its animation.");
}
