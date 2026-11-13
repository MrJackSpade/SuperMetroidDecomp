using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The eye beam's room-global state: its lifetime, the eye that casts it, its angular half-width and the fixed colour
/// it adds. It describes a colour-addition reveal, not an enemy projectile or damage source.
/// </summary>
public sealed class MorphBallEyeBeamState
{
    /// <summary>The beam's lifetime, advanced before enemy AI and separate from the eye's own AI action.</summary>
    public MorphBallEyeBeamPhase Phase { get; internal set; }

    /// <summary>The eye body that cast the beam, whose position and aim the cone follows; none while no beam exists.</summary>
    public RoomEnemySlot? Body { get; internal set; }

    /// <summary>
    /// The angle from the beam's centre to each of its edges, with its fraction of a step: zero when the beam appears,
    /// grown by <see cref="AngularHalfWidthGrowth"/> each widening update with the fraction carrying into whole steps,
    /// and set to four whole steps, keeping its fraction, on the update it reaches four. The growth step is a whole
    /// number of 256ths of a step, so no finer fraction ever arises.
    /// </summary>
    public RotationAngle AngularHalfWidth { get; internal set; }

    /// <summary>The whole steps of <see cref="AngularHalfWidth"/>, none through four, which the beam's window geometry uses.</summary>
    public TrigonometryAngle AngularWidth => AngularHalfWidth.ToTrigonometryAngle();

    /// <summary>The widening speed, the turn the half-width grows by each update: zero when the beam appears and raised by a quarter step before each widening update adds it.</summary>
    public RotationAngle AngularHalfWidthGrowth { get; internal set; }

    /// <summary>Position in the sixteen-step yellow brightness cycle, 0 through 15: the colour shown next.</summary>
    public int ColorIndex { get; internal set; }

    /// <summary>
    /// The colour the beam adds inside its cone: red and green only, never blue; black before the beam appears and once
    /// it has faded. A compatibility value (#1544): the beam fades one level of the game's authored levels
    /// (<see cref="FadeLevelColour"/>) a channel an update and is removed on the update its green is found at none, so the
    /// beam's lifetime depends on those levels, as the beam fade tests the colour's green before darkening it.
    /// </summary>
    public RgbColor Color { get; internal set; }

    /// <summary>
    /// The cone the frame ending now shows: the beam's phase, width and colour with its eye's position and aim as they
    /// stand at the frame-end latch; none while no beam exists.
    /// </summary>
    internal MorphBallEyeBeamCone? LatchDisplay()
    {
        if (Phase == MorphBallEyeBeamPhase.Inactive)
            return null;
        // A live beam always has an eye with its state; losing either is a corrupt enemy population.
        RoomEnemySlot body = Body
            ?? throw new InvalidDataException($"Active Morph Ball eye beam in phase {Phase} has no eye body.");
        MorphBallEyeEnemyState eye = body.SpeciesState<MorphBallEyeEnemyState>()
            ?? throw new InvalidDataException($"Active Morph Ball eye beam's body in enemy slot {body.SlotIndex} has no eye state.");
        return new MorphBallEyeBeamCone(Phase, body.Position.Whole, eye.Angle, AngularWidth, Color);
    }

    /// <summary>Ends the beam's lifetime, releases its eye and returns its colour to black.</summary>
    internal void Reset()
    {
        Phase = MorphBallEyeBeamPhase.Inactive;
        Body = null;
        AngularHalfWidth = RotationAngle.Zero;
        AngularHalfWidthGrowth = RotationAngle.Zero;
        ColorIndex = 0;
        Color = RgbColor.Black;
    }

    /// <summary>Whether the beam's green has faded to none, which ends the beam.</summary>
    internal bool GreenFaded => Color.FadeLevels.Green == 0;

    /// <summary>Darkens each channel one level, leaving a channel already at none unchanged.</summary>
    internal void FadeOneStep() => Color = Color.FadeLevels.Map(static (_, level) => level == 0 ? 0 : level - 1).ToColour();
}
