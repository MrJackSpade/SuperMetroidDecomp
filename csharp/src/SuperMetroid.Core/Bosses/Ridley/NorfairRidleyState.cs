using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Lower Norfair Ridley's encounter (#2228): the shared flight and tail with his Lower Norfair phase, his arena's reveal,
/// his health band, his grab and carry of Samus, his pogo and his death. The combat values carry the meanings their
/// consumers establish.
/// </summary>
public sealed partial class NorfairRidleyState : RidleyEncounterState
{
    /// <summary>Begins Lower Norfair Ridley's encounter below his arena, facing right.</summary>
    internal NorfairRidleyState()
        : base(
            new RidleyFlightState(RidleyFacing.Right, NorfairRidleyArenaDefinitions.MovementMinimum, NorfairRidleyArenaDefinitions.MovementMaximum,
                firstWingPose: RidleyWingDefinitions.NorfairFirstPose),
            new RidleyTailState(damage: RidleyTailDefinitions.NorfairTipDamage))
    {
    }

    /// <summary>The phase Lower Norfair Ridley runs once he has lifted off.</summary>
    /// <exception cref="InvalidOperationException">He has not lifted off.</exception>
    public NorfairRidleyPhase Phase
    {
        get => FightPhase<NorfairRidleyPhase>();
        internal set => SetFightPhase(value);
    }

    /// <summary>
    /// His arena's reveal (#2228, #2409), which he holds his roar through: his roar's hold begins it, and he takes off once it
    /// rests.
    /// </summary>
    /// <exception cref="InvalidOperationException">His roar's hold has not begun it.</exception>
    internal AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue> ArenaReveal
    {
        get => field ?? throw new InvalidOperationException("Lower Norfair Ridley's arena reveal has not begun: his roar's hold begins it.");
        private set;
    }

    /// <summary>Begins his arena's reveal as his roar's hold starts: its first step shows on the hold's first update.</summary>
    internal void BeginArenaReveal() => ArenaReveal = RidleyFadeAnimations.ArenaRevealing();

    /// <summary>The countdown of the swoop phase he flies, apart from his phase timer: each stage of his swoop lasts its own updates.</summary>
    public BossCountdown SwoopPhaseTimer { get; } = new();

    /// <summary>Gets his health band, updated from his health after each of his updates; he starts unhurt.</summary>
    public NorfairRidleyHealthBand HealthBand { get; internal set; } = NorfairRidleyHealthBandDefinitions.Unhurt;
    /// <summary>Gets the grabbed-Samus state controlling claw attachment and release: carrying Samus while Ridley holds her, otherwise released.</summary>
    public RidleyGrabState GrabState { get; internal set; }
    /// <summary>
    /// The countdown of his body's intangibility after he releases Samus, which lets her touch him again as it runs out;
    /// null while none runs.
    /// </summary>
    public BossCountdown? ReleaseIntangibility { get; internal set; }
    /// <summary>Gets the current movement target's whole-pixel room position.</summary>
    public Position16<RoomPixels> Target { get; internal set; }
    /// <summary>Gets the pogo bounce counter, reset once it reaches the bounces each fireballing takes.</summary>
    public int PogoBounceCount { get; internal set; }
    /// <summary>
    /// The side his pogo bounces toward (#2569): each launch takes its sideways speed toward it, and each bounce chooses
    /// it afresh, keeping or reversing it.
    /// </summary>
    public HorizontalDirection PogoBounce { get; internal set; }
    /// <summary>Gets the positive velocity increment the health band's pogo launch gives downward pogo motion.</summary>
    public SignedEightEight PogoDownwardAcceleration { get; internal set; }
    /// <summary>Gets the positive velocity increment the health band's pogo launch gives upward pogo motion.</summary>
    public SignedEightEight PogoUpwardAcceleration { get; internal set; }
    /// <summary>Gets Samus's signed whole-pixel displacement from the claw anchor; carry updates decay each axis toward zero.</summary>
    public Offset16<RoomPixels> GrabOffset { get; internal set; }
    /// <summary>The lag a held Samus builds behind his claw as he is hurt; once it is full a hurt update carries her.</summary>
    public RidleyHurtCarryLag HurtCarryLag { get; } = new();
    /// <summary>Gets the number of zero-health lunges completed before the authored death transition.</summary>
    public int ZeroHealthLungeCount { get; internal set; }
    /// <summary>The wait before his death's next small explosion, at the death's explosion interval.</summary>
    public BossCountdown DeathExplosionTimer { get; } = new();
    /// <summary>
    /// Where his death's next small explosion bursts: each takes the next point of the scatter shared with the Baby
    /// Metroid's death, the first after the last, starting from the scatter's first.
    /// </summary>
    public ListStep<Offset16<RoomPixels>> NextDeathExplosion { get; internal set; } = FirstDeathExplosion;
    /// <summary>Where his death's first small explosion bursts: the shared scatter's first point.</summary>
    internal static ListStep<Offset16<RoomPixels>> FirstDeathExplosion => ListStep<Offset16<RoomPixels>>.FirstOf(DeathExplosionScatterDefinitions.Offsets);

    /// <summary>Whether he carries Samus in his claws.</summary>
    internal override bool CarriesSamus => GrabState != RidleyGrabState.Released;
}
