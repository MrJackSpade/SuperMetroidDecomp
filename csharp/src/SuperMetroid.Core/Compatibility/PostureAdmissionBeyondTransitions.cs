namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether the posture-transition weapon admission admits weapons for a pose outside the transition table (#1411, #1474).
/// The admission reads one flag per pose, indexed by the pose's number from the table's first transition, so a pose
/// outside the table reads the turning, posture, spin, held and X-ray handlers' code stored around it: a zero admits
/// weapons and anything else preserves the charge. The import pipeline decodes each such pose's answer once.
/// </summary>
/// <remarks>
/// Consulted by the beam producer's posture-transition dispatch and the Grapple admission, during Samus's projectile
/// update. Evidence: the HUD admission check, which compares every pose's answer with the original flags and code.
/// </remarks>
/// <typeparam name="TPose">A Samus pose.</typeparam>
[CompatibilityService("posture-transition weapon admission for a pose outside its table reads the code stored around the table")]
public sealed class PostureAdmissionBeyondTransitions<TPose>
    where TPose : notnull
{
    /// <summary>Whether each pose outside the table admits weapons.</summary>
    private readonly IReadOnlyDictionary<TPose, bool> _admits;

    /// <summary>Keeps the decoded answer for every pose outside the table that the admission can read.</summary>
    /// <param name="admits">Whether each pose admits weapons.</param>
    public PostureAdmissionBeyondTransitions(IReadOnlyDictionary<TPose, bool> admits) => _admits = Ensure.NotNull(admits);

    /// <summary>Whether the admission admits weapons for <paramref name="pose"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The admission never reads a flag for the pose outside the table.</exception>
    public bool AdmitsWeapons(TPose pose) =>
        _admits.TryGetValue(pose, out bool admits)
            ? admits
            : throw new ArgumentOutOfRangeException(nameof(pose), pose, "The posture-transition admission reads no flag beyond its table for this pose.");
}
