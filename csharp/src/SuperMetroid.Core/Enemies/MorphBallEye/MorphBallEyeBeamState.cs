using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The eye beam's room-global state: its lifetime, the eye that casts it, its angular half-width and the fixed colour
/// it adds. It describes a colour-addition reveal, not an enemy projectile or damage source.
/// </summary>
public sealed class MorphBallEyeBeamState
{
    /// <summary>The beam's lifetime, advanced before enemy AI and separate from the eye's own AI function.</summary>
    public MorphBallEyeBeamPhase Phase { get; internal set; }

    /// <summary>The eye body that cast the beam, whose position and aim the cone follows; none while no beam exists.</summary>
    public RoomEnemySlot? Body { get; internal set; }

    /// <summary>
    /// The beam's half-width as a 16.16 value in 256-per-turn angle units: zero when the beam appears, grown by
    /// <see cref="AngularHalfWidthGrowth"/> each widening update with the fraction carrying into the whole part, and
    /// set to a whole part of four, keeping its fraction, on the update its whole part reaches four.
    /// </summary>
    public SignedSixteenSixteen AngularHalfWidth { get; internal set; }

    /// <summary>The whole angle units of <see cref="AngularHalfWidth"/>, zero through four, which the beam's window geometry uses.</summary>
    public int AngularWidth => AngularHalfWidth.WholePixels;

    /// <summary>The 16.16 widening speed in angle units per update: zero when the beam appears and raised by a quarter unit before each widening update adds it.</summary>
    public SignedSixteenSixteen AngularHalfWidthGrowth { get; internal set; }

    /// <summary>Position in the sixteen-step yellow brightness cycle, 0 through 15: the colour shown next.</summary>
    public int ColorIndex { get; internal set; }

    /// <summary>
    /// The colour the beam adds inside its cone: red and green only, never blue; black before the beam appears and once
    /// it has faded.
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
        AngularHalfWidth = SignedSixteenSixteen.Zero;
        AngularHalfWidthGrowth = SignedSixteenSixteen.Zero;
        ColorIndex = 0;
        Color = RgbColor.Black;
    }

    /// <summary>Darkens each colour channel by one intensity, leaving a channel already at zero unchanged.</summary>
    internal void FadeOneStep() => Color = Color.Map(static (_, intensity) => intensity == 0 ? 0 : intensity - 1);
}
