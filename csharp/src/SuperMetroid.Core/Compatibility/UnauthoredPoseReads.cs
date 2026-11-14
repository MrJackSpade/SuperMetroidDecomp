namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What Samus's three pose numbers past her authored poses define (#1551). The pose definitions author one
/// record per pose up to the last authored one; the game reads every pose's facing, movement, no-input pose, aim,
/// collision radius and shot height by the pose's number, so the three numbers after the last authored pose read the
/// X-ray cone code stored after the definitions as theirs. The first faces right, the other two face neither way; none
/// fires a shot, and each places a held charge's flare as its aim value modulo 16 selects; their radii and shot heights
/// are that data read as numbers. The second moves as standing and keeps its pose with no input; the first and
/// last read a movement beyond every movement type, which no movement step runs, and fall back with no input to the
/// rightward neutral jump transition and to landing aiming diagonally down facing left.
/// </summary>
/// <remarks>
/// A rule with no state, held by the unauthored pose catalog. Consulted wherever a pose's facing, movement, no-input
/// pose, aim, vertical collision radius or shot height is read, for a current or prospective pose. Evidence: the facing,
/// movement, no-input, aim, radius and projectile origin reads index the definitions by the pose number times eight with
/// no bound, and the pose dispatch, collision and projectile origin checks compare each of these definitions with the
/// original bytes through the pose record codec.
/// </remarks>
/// <typeparam name="TPose">A Samus pose.</typeparam>
/// <typeparam name="TDefinition">What a pose defines: its facing, movement, no-input pose, aim, radius and shot height.</typeparam>
[CompatibilityService("Samus's pose numbers past her authored poses read the code stored after the pose definitions as their facing, movement, no-input pose, aim, collision radius and shot height")]
public sealed class UnauthoredPoseReads<TPose, TDefinition>
    where TPose : notnull
    where TDefinition : notnull
{
    /// <summary>What each pose number past the authored poses defines.</summary>
    private readonly IReadOnlyDictionary<TPose, TDefinition> _definitions;

    /// <summary>Keeps what each pose number past the authored poses defines.</summary>
    /// <param name="definitions">What each such pose defines.</param>
    /// <exception cref="ArgumentException">No pose was given.</exception>
    public UnauthoredPoseReads(IReadOnlyDictionary<TPose, TDefinition> definitions)
    {
        Ensure.NotNull(definitions);
        if (definitions.Count == 0)
            throw new ArgumentException("At least one pose number lies past the authored poses.", nameof(definitions));
        _definitions = definitions;
    }

    /// <summary>
    /// What <paramref name="pose"/> defines when it is one of the pose numbers past the authored poses; false for an
    /// authored pose, whose own definition answers.
    /// </summary>
    /// <param name="pose">A current or prospective pose.</param>
    /// <param name="definition">What the pose reads past the authored poses, when it is one of them.</param>
    public bool TryRead(TPose pose, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TDefinition definition) =>
        _definitions.TryGetValue(pose, out definition);
}
