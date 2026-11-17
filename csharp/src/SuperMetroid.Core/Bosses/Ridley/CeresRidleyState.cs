namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley's encounter (#2228): the shared flight and tail with his Ceres phase, the hundred hits that end his fight,
/// his hover's attack count, his fireball's anchor and the Baby he carries.
/// </summary>
public sealed class CeresRidleyState : RidleyEncounterState
{
    /// <summary>Begins Ceres Ridley's encounter with his body at <paramref name="body"/>, the Baby in his arms.</summary>
    /// <param name="body">Where his body stands as he is placed.</param>
    internal CeresRidleyState(Position16<RoomPixels> body)
        : base(
            new RidleyFlightState(RidleyFacing.Left, CeresRidleyArenaDefinitions.MovementMinimum, CeresRidleyArenaDefinitions.MovementMaximum,
                firstWingPose: RidleyWingDefinitions.CeresFirstPose),
            new RidleyTailState(damage: RidleyTailDefinitions.CeresTipDamage))
    {
        Baby = new CeresBaby(body + CeresRidleyArenaDefinitions.BabyArmsAnchor);
        // Ceres Ridley's tail starts whipping of its own; his swoop alone stops it.
        Tail.IdleWhipEnabled = true;
    }

    /// <summary>The phase Ceres Ridley runs once he has lifted off.</summary>
    /// <exception cref="InvalidOperationException">He has not lifted off.</exception>
    public CeresRidleyPhase Phase
    {
        get => LiftedOff ? field : throw NotLiftedOff();
        internal set
        {
            field = value;
            LiftOff();
        }
    }

    /// <summary>
    /// Gets the number of shots Ridley has taken, used for health-colour progression and the hundred-hit fake-retreat
    /// trigger. It only counts up, one per accepted shot, and shots stop landing once the retreat publishes the escape
    /// handoff, so it stays far below the range where the game's counter would wrap.
    /// </summary>
    public int HitCounter { get; internal set; }

    /// <summary>Gets the hover-cycle counter used to choose the next scripted attack; every attack resets it before it passes 124.</summary>
    public int HoverCounter { get; internal set; }

    /// <summary>Gets the fireball attack's whole-pixel body anchor; firing motion aims at random jitter around this saved position.</summary>
    public Position16<RoomPixels> FireballBasePosition { get; internal set; }

    /// <summary>The Baby Metroid he carries, drops and catches.</summary>
    public CeresBaby Baby { get; }

    /// <summary>Ceres Ridley never grabs Samus.</summary>
    internal override bool CarriesSamus => false;
}
