using SuperMetroid.Core.Assets;namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The eye beam's room-global state: its lifetime, the eye that casts it, its angular half-width and the colour it
/// adds. It describes a colour-addition reveal, not an enemy projectile or damage source.
/// </summary>
public sealed class MorphBallEyeBeamState : IDisplayLatch
{
    /// <summary>The beam's lifetime, advanced before enemy AI and separate from the eye's own AI action.</summary>
    public MorphBallEyeBeamPhase Phase { get; internal set; }

    /// <summary>The eye body that cast the beam, whose position and aim the cone follows; none while no beam exists.</summary>
    public RoomEnemyPlace? Body { get; internal set; }

    /// <summary>
    /// The angle from the beam's centre to each of its edges, with its fraction of a step: zero when the beam appears,
    /// grown by <see cref="AngularHalfWidthGrowth"/> each widening update with the fraction carrying into whole steps,
    /// and set to <see cref="MorphBallEyeBeamDefinitions.FullHalfWidth"/>, keeping its fraction, on the update its whole steps reach it.
    /// </summary>
    public RotationAngle AngularHalfWidth { get; internal set; }

    /// <summary>The whole steps of <see cref="AngularHalfWidth"/>, which the beam's window geometry uses.</summary>
    internal TrigonometryAngle AngularWidth => AngularHalfWidth.ToTrigonometryAngle();

    /// <summary>The widening speed, the turn the half-width grows by each update: zero when the beam appears and raised by <see cref="MorphBallEyeBeamDefinitions.WidthGrowthStep"/> before each widening update adds it.</summary>
    public RotationAngle AngularHalfWidthGrowth { get; internal set; }

    /// <summary>The colour of the sixteen-colour yellow brightness cycle the full beam shows next.</summary>
    public MorphBallEyeBeamColour NextColour { get; internal set; }

    /// <summary>
    /// The colour the beam adds inside its cone: red and green only, never blue; black before the beam appears and once
    /// it has faded. A compatibility value (#1544): the beam fades one level of the game's authored levels
    /// (<see cref="FadeLevelColour"/>) a channel an update and is removed on the update its green is found at none, so the
    /// beam's lifetime depends on those levels, as the beam fade tests the colour's green before darkening it.
    /// </summary>
    internal RgbColor Color { get; set; }

    /// <summary>
    /// The cone the frame boundary last latched: the beam's phase, width and colour with its eye's position and aim as they
    /// stood at the latch; none while no beam existed then.
    /// </summary>
    internal MorphBallEyeBeamCone? Latched { get; private set; }

    /// <summary>Latches the cone the frame ending now shows (<see cref="Latched"/>).</summary>
    void IDisplayLatch.LatchDisplay() => Latched = Cone();

    /// <summary>
    /// The cone the beam shows now: its phase, width and colour with its eye's position and aim; none while no beam exists.
    /// </summary>
    private MorphBallEyeBeamCone? Cone()
    {
        if (Phase == MorphBallEyeBeamPhase.Inactive)
            return null;
        // A live beam always has an eye with its state; losing either is a corrupt enemy population.
        RoomEnemyPlace body = Body
            ?? throw new InvalidDataException($"Active Morph Ball eye beam in phase {Phase} has no eye body.");
        MorphBallEyeEnemyState eye = body.SpeciesState<MorphBallEyeEnemyState>()
            ?? throw new InvalidDataException($"Active Morph Ball eye beam's body in enemy place {body.Ordinal} has no eye state.");
        return new MorphBallEyeBeamCone(Phase, body.Position.Whole, eye.Angle, AngularWidth, Color);
    }

    /// <summary>Ends the beam's lifetime, releases its eye and returns its colour to black.</summary>
    internal void Reset()
    {
        Phase = MorphBallEyeBeamPhase.Inactive;
        Body = null;
        AngularHalfWidth = RotationAngle.Zero;
        AngularHalfWidthGrowth = RotationAngle.Zero;
        NextColour = MorphBallEyeBeamColour.First;
        Color = RgbColor.Black;
    }

    /// <summary>Whether the beam's green has faded to none, which ends the beam.</summary>
    internal bool GreenFaded => Color.FadeLevels.Green == 0;

    /// <summary>Darkens each channel one level, leaving a channel already at none unchanged.</summary>
    internal void FadeOneStep() => Color = Color.FadeLevels.Map(static (_, level) => level == 0 ? 0 : level - 1).ToColour();
}
