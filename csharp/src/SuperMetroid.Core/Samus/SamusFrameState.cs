using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Samus and her per-frame state: the actor, movement mode, prospective poses, the last movement and X-ray
/// results, death-sequence and palette-FX steps, and load appearance.
/// </summary>
internal sealed class SamusFrameState
{
    /// <summary>Remaining native load-appearance updates before ordinary Samus drawing resumes.</summary>
    internal WrappingCountdown LoadAppearanceFramesRemaining { get; private set; }
    /// <summary>Palette-FX object selected while Samus's load appearance is active.</summary>
    internal PaletteFxObject? LoadAppearancePaletteFx { get; private set; }

    /// <summary>Whether Samus's saved-game load appearance is still running.</summary>
    internal bool LoadAppearanceActive => !LoadAppearanceFramesRemaining.IsZero;

    /// <summary>Starts the saved-game load appearance with its palette-FX object and length.</summary>
    internal void BeginLoadAppearance(PaletteFxObject paletteFx, WrappingCountdown frames)
    {
        LoadAppearancePaletteFx = paletteFx;
        LoadAppearanceFramesRemaining = frames;
    }

    /// <summary>Counts one load-appearance update; returns whether that was its last.</summary>
    internal bool AdvanceLoadAppearance()
    {
        LoadAppearanceFramesRemaining = LoadAppearanceFramesRemaining.Decrement().Remaining;
        return LoadAppearanceFramesRemaining.IsZero;
    }

    /// <summary>Ends the load appearance, releasing its palette-FX object.</summary>
    internal void EndLoadAppearance() => LoadAppearancePaletteFx = null;

    /// <summary>
    /// Normal-gameplay Samus rendering state. Null means the current scenario has not
    /// explicitly introduced Samus; the Landing Site cinematic does not do so by itself.
    /// </summary>
    public SamusState? Samus { get; internal set; }

    /// <summary>Samus once the scenario has introduced her; gameplay phases that move, draw or damage her require it.</summary>
    public SamusState IntroducedSamus => Samus ??
        throw new InvalidOperationException("Samus has not been introduced; this gameplay phase requires her actor.");

    /// <summary>
    /// Winning cartridge pose-transition entry for the most recently latched controller
    /// chord. The grounded debug scenario applies its verified $01/$02/$09/$0A/$25/$26
    /// routes; every other winner remains diagnostic until its native side effects exist.
    /// </summary>
    public SamusPoseTransition? ProspectiveSamusPose { get; internal set; }

    /// <summary>
    /// Block-only result of `$91:EADE` after movement. This is separate from the input
    /// table record because a killed running speed can create `$89/$8A/$CF-$D2` even when
    /// no controller record matched, and a one-pixel probe can replace a running target.
    /// </summary>
    public SamusPoseId? ProspectiveSamusWallCollisionPose { get; internal set; }


    /// <summary>
    /// No-button fallback selected by <c>Samus_Pose_CancelGrapple</c> at $91:82D9. This is
    /// separate from <see cref="SamusFrameState.ProspectiveSamusPose"/> because no six-byte table entry wins.
    /// </summary>
    public SamusPoseId? ProspectiveSamusFallbackPose { get; internal set; }

    /// <summary>
    /// True only for the explicit grounded gameplay-debug scenario. The cinematic render
    /// stimulus remains stationary and continues treating pose matches as diagnostics.
    /// </summary>
    public bool GroundedSamusMovementEnabled { get; internal set; }

    /// <summary>
    /// Low-byte snapshot at WRAM <c>$0A11</c> used by X-ray stability and post-draw spin audio.
    /// The runtime updates it after each completed gameplay frame.
    /// </summary>
    public SamusMovementType PreviousMovementTypeForXray { get; internal set; }

    /// <summary>
    /// Horizontal and vertical collision results from the most recent translated grounded
    /// movement pass. Null before movement, and always null for the cinematic stimulus.
    /// </summary>
    public GroundedMovementResult? LastGroundedSamusMovement { get; internal set; }

    /// <summary>Most recent ordinary-air collision result, exposed for debugger watches.</summary>
    public AerialMovementResult? LastAerialSamusMovement { get; internal set; }

    /// <summary>Most recent ordinary Morph-Ball collision result, exposed for debugger watches.</summary>
    public MorphBallMovementResult? LastMorphBallMovement { get; internal set; }

    /// <summary>Most recent special bomb-jump handler result, exposed for debugger watches.</summary>
    public BombJumpMovementResult? LastBombJumpMovement { get; internal set; }

    /// <summary>Most recent special knockback-handler result, exposed for debugger watches.</summary>
    public KnockbackMovementResult? LastKnockbackMovement { get; internal set; }


    /// <summary>Most recent bank-$9B connected-grapple function result.</summary>
    public GrappleMovementResult? LastGrappleMovement { get; internal set; }

    /// <summary>Most recent stored-shine windup or active shinespark handler result.</summary>
    public ShinesparkMovementResult? LastShinesparkMovement { get; internal set; }

    /// <summary>Most recent Crystal Flash start, ammo-drain, or finish handler result.</summary>
    public CrystalFlashMovementResult? LastCrystalFlashMovement { get; internal set; }


    /// <summary>Most recent call of X-ray's bank-$88 setup/beam/deactivation state.</summary>
    public XrayBeamStepResult? LastXrayBeamStep { get; internal set; }


    /// <summary>Most recent game-state-owned fatal-damage animation/palette/VRAM call.</summary>
    public SamusDeathSequenceStepResult? LastDeathSequenceStep { get; internal set; }


    /// <summary>Most recent call of drained Samus's installed `$90:94CB` falling handler.</summary>
    public DrainedSamusMovementResult? LastDrainedSamusMovement { get; internal set; }

    /// <summary>Most recent normal movement-type-`$1A` dispatcher result.</summary>
    public DraygonGrabbedMovementResult? LastDraygonGrabbedMovement { get; internal set; }

    /// <summary>Most recent `$90:E2A1` alternating-D-pad escape-handler result.</summary>
    public DraygonEscapeResult? LastDraygonEscape { get; internal set; }

    /// <summary>
    /// Explicit debugger substitute for the untranslated HUD item selector. When enabled,
    /// a new Shoot edge starts grapple firing from the current pose. Normal scenarios leave
    /// this false, so X retains its existing bomb/projectile meaning.
    /// </summary>
    public bool DebugGrappleItemSelected { get; private set; }
}
