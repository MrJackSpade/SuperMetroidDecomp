namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The setup the pose update runs when Samus's input chooses no new pose (#2506), from her fallback pose: a pose names its
/// own, and the ground turn and the normal jump name theirs on their movement (<see cref="SamusNoTransitionSetups"/>).
/// Each setup runs itself through the pose phase (#2571), so no switch over setup identities picks the response.
/// </summary>
internal sealed class SamusNoTransitionSetup
{
    /// <summary>The setup's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Runs the setup.</summary>
    private readonly Action<SamusPoseSetupSteps, SamusControlFrame, SamusState, SamusPoseId, SamusPoseId?> _run;

    /// <summary>A setup named <paramref name="name"/> that runs <paramref name="run"/>.</summary>
    /// <param name="name">The setup's name, for diagnostics.</param>
    /// <param name="run">Runs the setup.</param>
    private SamusNoTransitionSetup(string name, Action<SamusPoseSetupSteps, SamusControlFrame, SamusState, SamusPoseId, SamusPoseId?> run)
    {
        _name = name;
        _run = run;
    }

    /// <summary>Nothing more is set up: her pose and momentum stay as her movement left them.</summary>
    internal static SamusNoTransitionSetup KeepsMomentum { get; } = new(nameof(KeepsMomentum), (_, _, _, _, _) => { });

    /// <summary>A knockback kept as its fallback stops her momentum.</summary>
    internal static SamusNoTransitionSetup StopKnockback { get; } = new(nameof(StopKnockback),
        (_, _, samus, source, fallback) => SamusPoseSetupSteps.RespondStopKnockback(samus, source, fallback));

    /// <summary>Releasing every damage-boost chord leaves the boost for the neutral jump.</summary>
    internal static SamusNoTransitionSetup ReleaseDamageBoost { get; } = new(nameof(ReleaseDamageBoost),
        (owner, _, samus, source, fallback) => owner.RespondReleaseDamageBoost(samus, source, fallback));

    /// <summary>Draygon's hold returns her aimed, firing or struggling pose to the hold's neutral one.</summary>
    internal static SamusNoTransitionSetup ReturnToNeutralDraygonHold { get; } = new(nameof(ReturnToNeutralDraygonHold),
        (_, _, samus, _, fallback) => SamusPoseSetupSteps.RespondReturnToNeutralDraygonHold(samus, fallback));

    /// <summary>A wall jump gives way to the ordinary spin, losing its speed unless the spin's setup promotes it.</summary>
    internal static SamusNoTransitionSetup SpinFromWallJump { get; } = new(nameof(SpinFromWallJump),
        (_, _, samus, _, fallback) => SamusPoseSetupSteps.RespondSpinFromWallJump(samus, fallback));

    /// <summary>A falling Morph Ball kept as its fallback slows down.</summary>
    internal static SamusNoTransitionSetup DecelerateFallingBall { get; } = new(nameof(DecelerateFallingBall),
        (_, frame, samus, source, fallback) => SamusPoseSetupSteps.RespondDecelerateFallingBall(frame, samus, source, fallback));

    /// <summary>A still or airborne ball kept as its fallback loses her horizontal speed.</summary>
    internal static SamusNoTransitionSetup StopBall { get; } = new(nameof(StopBall),
        (_, _, samus, source, fallback) => SamusPoseSetupSteps.RespondStopBall(samus, source, fallback));

    /// <summary>A rolling ball stops, losing her horizontal speed, and takes its still pose.</summary>
    internal static SamusNoTransitionSetup StopRollingBall { get; } = new(nameof(StopRollingBall),
        (_, _, samus, _, fallback) => SamusPoseSetupSteps.RespondStopRollingBall(samus, fallback));

    /// <summary>Plain running slows down while she keeps speed, and otherwise stops in the standing pose.</summary>
    internal static SamusNoTransitionSetup StopRunning { get; } = new(nameof(StopRunning),
        (_, _, samus, source, fallback) => SamusPoseSetupSteps.RespondStopRunning(samus, source, fallback));

    /// <summary>Aimed or firing running slows down while she keeps speed, and otherwise lowers her aim to standing.</summary>
    internal static SamusNoTransitionSetup StopAimedRunning { get; } = new(nameof(StopAimedRunning),
        (_, _, samus, source, fallback) => SamusPoseSetupSteps.RespondStopAimedRunning(samus, source, fallback));

    /// <summary>A moonwalk ends in its standing or aimed fallback.</summary>
    internal static SamusNoTransitionSetup StopMoonwalk { get; } = new(nameof(StopMoonwalk),
        (owner, _, samus, _, fallback) => owner.RespondStopMoonwalk(samus, fallback));

    /// <summary>An aimed standing, crouching or wall-stop pose lowers her aim to its plain pose.</summary>
    internal static SamusNoTransitionSetup LowerGroundedAim { get; } = new(nameof(LowerGroundedAim),
        (_, _, samus, _, fallback) => SamusPoseSetupSteps.RespondLowerGroundedAim(samus, fallback));

    /// <summary>An aimed crouching turn still in its own pose is abandoned for its crouch.</summary>
    internal static SamusNoTransitionSetup AbandonAimedCrouchingTurn { get; } = new(nameof(AbandonAimedCrouchingTurn),
        (_, frame, samus, source, fallback) => SamusPoseSetupSteps.RespondAbandonAimedCrouchingTurn(frame, samus, source, fallback));

    /// <summary>A turn still in its own pose stops her momentum without restarting the turn.</summary>
    internal static SamusNoTransitionSetup StopTurn { get; } = new(nameof(StopTurn),
        (_, frame, samus, source, _) => SamusPoseSetupSteps.RespondStopTurn(frame, samus, source));

    /// <summary>A normal jump takes its fallback's aim and slows down.</summary>
    internal static SamusNoTransitionSetup ContinueNormalJump { get; } = new(nameof(ContinueNormalJump),
        (_, frame, samus, _, fallback) => SamusPoseSetupSteps.RespondContinueNormalJump(frame, samus, fallback));

    /// <summary>An aimed fall takes its fallback's aim.</summary>
    internal static SamusNoTransitionSetup LowerAerialAim { get; } = new(nameof(LowerAerialAim),
        (_, _, samus, _, fallback) => SamusPoseSetupSteps.RespondLowerAerialAim(samus, fallback));

    /// <summary>Runs the response to input that chose no new pose.</summary>
    /// <param name="owner">The steps the setup runs, which the pose phase hands it.</param>
    /// <param name="frame">The frame the setup reads.</param>
    /// <param name="samus">The samus the setup reads.</param>
    /// <param name="source">The source the setup reads.</param>
    /// <param name="fallback">The fallback the setup reads.</param>
    internal void Apply(SamusPoseSetupSteps owner, SamusControlFrame frame, SamusState samus, SamusPoseId source, SamusPoseId? fallback) => _run(owner, frame, samus, source, fallback);

    /// <summary>Names the setup.</summary>
    public override string ToString() => _name;
}
