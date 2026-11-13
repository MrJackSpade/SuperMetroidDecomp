namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far the opening cinematic's egg-shell fragments fall on the updates after their authored gravity curve ends (#1500).
/// Each fragment falls by the curve's step for the number of updates it has fallen until it reaches the floor; the curve
/// has 38 steps, and the two fragments that start highest are still airborne after them, so the game reads
/// their next three steps from the data that follows the curve. Those steps decide where the two fragments are drawn on
/// their last updates and when they land. The import pipeline decodes the three steps once from the player's game data.
/// </summary>
/// <remarks>
/// Consulted by a falling shell fragment's update once it has fallen for the whole curve, before the floor test. Evidence:
/// the fragment motion indexes the curve by the updates fallen, with no count, and deletes a fragment at the floor; from
/// their fixed spawn positions and launch speeds, fragments two to five land within the curve and fragments zero and one
/// land on their third update past it. The intro egg motion check decodes the steps from the cartridge, compares them
/// with this service and follows every fragment to the floor.
/// </remarks>
[CompatibilityService("the two highest egg-shell fragments fall by the steps read past the gravity curve before they land")]
public sealed class ShellFragmentFallPastCurve
{
    /// <summary>The steps, in the order the updates past the curve read them.</summary>
    private readonly IReadOnlyList<SignedSixteenSixteen> _steps;

    /// <summary>Keeps the decoded steps.</summary>
    /// <param name="steps">The steps, in the order the updates past the curve read them.</param>
    /// <exception cref="ArgumentException">No step was decoded.</exception>
    public ShellFragmentFallPastCurve(IReadOnlyList<SignedSixteenSixteen> steps)
    {
        Ensure.NotNull(steps);
        if (steps.Count == 0)
            throw new ArgumentException("A shell fragment falls past its curve for at least one update.", nameof(steps));
        _steps = steps;
    }

    /// <summary>The step a fragment falls by on the update <paramref name="updatesPastCurve"/> after its curve ends.</summary>
    /// <param name="updatesPastCurve">Updates fallen past the curve, zero for the first.</param>
    /// <exception cref="ArgumentOutOfRangeException">No fragment is still airborne that far past the curve.</exception>
    public SignedSixteenSixteen Step(int updatesPastCurve) =>
        updatesPastCurve >= 0 && updatesPastCurve < _steps.Count
            ? _steps[updatesPastCurve]
            // Rejected, not read: every fragment has reached the floor by the last decoded step, so a later read is
            // never made; the intro egg motion check follows every fragment from its spawn to its deletion.
            : throw new ArgumentOutOfRangeException(nameof(updatesPastCurve), updatesPastCurve,
                $"Every shell fragment has landed within {_steps.Count} updates past its gravity curve.");
}
