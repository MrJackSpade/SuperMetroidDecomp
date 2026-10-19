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
    public SnesSignedSixteenSixteen AngularHalfWidth { get; internal set; }

    /// <summary>The whole angle units of <see cref="AngularHalfWidth"/>, zero through four, which the beam's window geometry uses.</summary>
    public int AngularWidth => AngularHalfWidth.WholePixels;

    /// <summary>The 16.16 widening speed in angle units per update: zero when the beam appears and raised by a quarter unit before each widening update adds it.</summary>
    public SnesSignedSixteenSixteen AngularHalfWidthGrowth { get; internal set; }

    /// <summary>Position in the sixteen-step yellow brightness cycle; only its low four bits select the colour shown next.</summary>
    public int ColorIndex { get; internal set; }

    /// <summary>
    /// The red level of the added fixed colour, as the fixed-colour register takes it: the red selector with a five-bit
    /// intensity, never below <see cref="MorphBallEyeBeamDefinitions.FadedRed"/>.
    /// </summary>
    public int Red { get; internal set; } = MorphBallEyeBeamDefinitions.FadedRed;

    /// <summary>
    /// The green level of the added fixed colour, as the fixed-colour register takes it: the green selector with a
    /// five-bit intensity, never below <see cref="MorphBallEyeBeamDefinitions.FadedGreen"/>.
    /// </summary>
    public int Green { get; internal set; } = MorphBallEyeBeamDefinitions.FadedGreen;

    /// <summary>The blue level of the added fixed colour: the blue selector with zero intensity, which the beam never changes.</summary>
    public int Blue { get; internal set; } = MorphBallEyeBeamDefinitions.Blue;

    /// <summary>Clears the HDMA beam lifetime and restores neutral COLDATA selector bytes.</summary>
    internal void Reset()
    {
        Phase = MorphBallEyeBeamPhase.Inactive;
        BodySlotIndex = -1;
        AngularHalfWidth = SnesSignedSixteenSixteen.Zero;
        AngularHalfWidthGrowth = SnesSignedSixteenSixteen.Zero;
        ColorIndex = 0;
        Red = MorphBallEyeBeamDefinitions.FadedRed;
        Green = MorphBallEyeBeamDefinitions.FadedGreen;
        Blue = MorphBallEyeBeamDefinitions.Blue;
    }

    /// <summary>Steps each colour level one intensity toward its faded level, leaving a level already there unchanged.</summary>
    internal void FadeOneStep()
    {
        Red = FadeToward(Red, MorphBallEyeBeamDefinitions.FadedRed);
        Green = FadeToward(Green, MorphBallEyeBeamDefinitions.FadedGreen);
        Blue = FadeToward(Blue, MorphBallEyeBeamDefinitions.Blue);
    }

    /// <summary>
    /// One intensity lower when <paramref name="level"/> is above <paramref name="faded"/>, or unchanged at it. A level
    /// below its faded level is never produced, so reaching one is a defect rather than a wrap.
    /// </summary>
    private static int FadeToward(int level, int faded)
    {
        if (level < faded)
            throw new InvalidOperationException($"Eye beam colour level {level} is below its faded level {faded}.");
        return level == faded ? level : level - 1;
    }
}
