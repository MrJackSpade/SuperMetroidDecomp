namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One palette-FX program command with its typed operand.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="ColorIndex">The CGRAM colour a <see cref="PaletteFxCueKind.SetColorIndex"/> cue makes the object's next record start at.</param>
/// <param name="PreInstruction">The pre-instruction a <see cref="PaletteFxCueKind.SetPreInstruction"/> cue installs.</param>
/// <param name="HeatPhase">The Norfair heat animation phase a <see cref="PaletteFxCueKind.SetHeatPaletteIndex"/> cue publishes.</param>
/// <param name="Sound">The library-2 sound a <see cref="PaletteFxCueKind.QueueSoundLibrary2"/> cue queues.</param>
public readonly record struct PaletteFxCue(
    PaletteFxCueKind Kind,
    int ColorIndex = 0,
    PaletteFxPreInstruction PreInstruction = default,
    int HeatPhase = 0,
    SoundEffectId? Sound = null);
