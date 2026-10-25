namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The eye beam's room-global state: its lifetime, its angular half-width and the fixed colour it adds. It describes a
/// colour-math reveal, not an enemy projectile or damage source.
/// </summary>
public sealed class MorphBallEyeBeamState
{
    /// <summary>Current host projection of the room-global HDMA object's lifetime, advanced before enemy AI; separate from the body's bank-$A8 function word.</summary>
    public MorphBallEyeBeamPhase Phase { get; internal set; }

    /// <summary>Literal body slot read by <c>$88:E96D/$88:E98B</c>; retail uses slot one.</summary>
    public int BodySlotIndex { get; internal set; } = -1;

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

    /// <summary>Position in the sixteen-step yellow brightness cycle; only its low four bits select the colour shown next.</summary>
    public int ColorIndex { get; internal set; }

    /// <summary>
    /// The colour the beam adds inside its cone: red and green only, never blue; black before the beam appears and once
    /// it has faded.
    /// </summary>
    public RgbColor Color { get; internal set; }

    /// <summary>Ends the beam's lifetime and returns its colour to black.</summary>
    internal void Reset()
    {
        Phase = MorphBallEyeBeamPhase.Inactive;
        BodySlotIndex = -1;
        AngularHalfWidth = SignedSixteenSixteen.Zero;
        AngularHalfWidthGrowth = SignedSixteenSixteen.Zero;
        ColorIndex = 0;
        Color = RgbColor.Black;
    }

    /// <summary>Darkens each colour channel by one intensity, leaving a channel already at zero unchanged.</summary>
    internal void FadeOneStep() => Color = Color.Map(static (_, intensity) => intensity == 0 ? 0 : intensity - 1);
}
