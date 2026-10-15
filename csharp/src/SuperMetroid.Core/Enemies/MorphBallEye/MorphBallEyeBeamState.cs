namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// Debugger-readable form of the eye beam's HDMA-object variables and three shared WRAM
/// accumulators. It describes a color-math reveal, not an enemy projectile or damage source.
/// </summary>
public sealed class MorphBallEyeBeamState
{
    /// <summary>Current host projection of the room-global HDMA object's lifetime, advanced before enemy AI; separate from the body's bank-$A8 function word.</summary>
    public MorphBallEyeBeamPhase Phase { get; internal set; }

    /// <summary>Literal body slot read by <c>$88:E96D/$88:E98B</c>; retail uses slot one.</summary>
    public int BodySlotIndex { get; internal set; } = -1;

    /// <summary>Whole angular half-width, corresponding to HDMA object variable two.</summary>
    public ushort AngularWidth { get; internal set; }

    /// <summary>Fractional angular half-width, corresponding to HDMA object variable three.</summary>
    public ushort AngularSubwidth { get; internal set; }

    /// <summary>Whole widening velocity in the shared eye-beam delta word.</summary>
    public ushort AngularWidthDelta { get; internal set; }

    /// <summary>Fractional widening velocity in the shared eye-beam sub-delta word.</summary>
    public ushort AngularSubwidthDelta { get; internal set; }

    /// <summary>Index into the native sixteen-step yellow brightness cycle.</summary>
    public ushort ColorIndex { get; internal set; }

    /// <summary>Raw SNES COLDATA red selector/component byte.</summary>
    public byte Red { get; internal set; } = MorphBallEyeBeamDefinitions.FadedRed;

    /// <summary>Raw SNES COLDATA green selector/component byte.</summary>
    public byte Green { get; internal set; } = MorphBallEyeBeamDefinitions.FadedGreen;

    /// <summary>Raw SNES COLDATA blue selector/component byte.</summary>
    public byte Blue { get; internal set; } = MorphBallEyeBeamDefinitions.Blue;

    /// <summary>Clears the HDMA beam lifetime and restores neutral COLDATA selector bytes.</summary>
    internal void Reset()
    {
        Phase = MorphBallEyeBeamPhase.Inactive;
        BodySlotIndex = -1;
        AngularWidth = 0;
        AngularSubwidth = 0;
        AngularWidthDelta = 0;
        AngularSubwidthDelta = 0;
        ColorIndex = 0;
        Red = MorphBallEyeBeamDefinitions.FadedRed;
        Green = MorphBallEyeBeamDefinitions.FadedGreen;
        Blue = MorphBallEyeBeamDefinitions.Blue;
    }
}
