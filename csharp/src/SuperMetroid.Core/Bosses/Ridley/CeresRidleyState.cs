using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley's encounter (#2228): the shared flight and tail with his Ceres phase, the hundred hits that end his fight,
/// his hover's attack count, his fireball's anchor and the Baby he carries.
/// </summary>
public sealed class CeresRidleyState : RidleyEncounterState
{
    /// <summary>Begins Ceres Ridley's encounter with his body at <paramref name="body"/>, the Baby in his arms.</summary>
    /// <param name="body">Where his body stands as he is placed.</param>
    /// <param name="warning">The room's foreground warning, which the self-destruct his getaway starts writes.</param>
    internal CeresRidleyState(Position16<RoomPixels> body, RoomWarning warning)
        : base(
            new RidleyFlightState(RidleyFacing.Left, CeresRidleyArenaDefinitions.MovementMinimum, CeresRidleyArenaDefinitions.MovementMaximum,
                firstWingPose: RidleyWingDefinitions.CeresFirstPose),
            new RidleyTailState(damage: RidleyTailDefinitions.CeresTipDamage))
    {
        Baby = new CeresBaby(body + CeresRidleyArenaDefinitions.BabyArmsAnchor);
        Warning = Ensure.NotNull(warning);
        // Ceres Ridley's tail starts whipping of its own; his swoop alone stops it.
        Tail.IdleWhipEnabled = true;
    }

    /// <summary>The room's foreground warning, which the self-destruct his getaway starts writes (#2554).</summary>
    internal RoomWarning Warning { get; }

    /// <summary>The phase Ceres Ridley runs once he has lifted off.</summary>
    /// <exception cref="InvalidOperationException">He has not lifted off.</exception>
    public CeresRidleyPhase Phase
    {
        get => FightPhase<CeresRidleyPhase>();
        internal set => SetFightPhase(value);
    }

    /// <summary>
    /// Gets the number of shots Ridley has taken, used for health-colour progression and the fake-retreat
    /// trigger. It only counts up, one per accepted shot, and shots stop landing once the retreat publishes the escape
    /// handoff.
    /// </summary>
    public int HitCounter { get; internal set; }

    /// <summary>The wait his hover holds while he is outside the hover box before he attacks anyway, loaded as each hover begins.</summary>
    public BossCountdown HoverWait { get; } = new();

    /// <summary>
    /// The wait above the hover line after which his move into his fireball position gives up for a swoop, loaded as he
    /// chooses his attack.
    /// </summary>
    public BossCountdown FireballMoveWait { get; } = new();

    /// <summary>Gets the fireball attack's whole-pixel body anchor; firing motion aims at random jitter around this saved position.</summary>
    public Position16<RoomPixels> FireballBasePosition { get; internal set; }

    /// <summary>The Baby Metroid he carries, drops and catches.</summary>
    public CeresBaby Baby { get; }

    /// <summary>Ceres Ridley never grabs Samus.</summary>
    internal override bool CarriesSamus => false;
}
