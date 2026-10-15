namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One palette-FX program command with its typed operand.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="ColorByteIndex">The CGRAM byte index a <see cref="PaletteFxCueKind.SetColorIndex"/> cue selects.</param>
/// <param name="PreInstruction">The pre-instruction a <see cref="PaletteFxCueKind.SetPreInstruction"/> cue installs.</param>
/// <param name="HeatPaletteIndex">The heat palette index a <see cref="PaletteFxCueKind.SetHeatPaletteIndex"/> cue publishes.</param>
/// <param name="Sound">The library-2 sound a <see cref="PaletteFxCueKind.QueueSoundLibrary2"/> cue queues.</param>
public readonly record struct PaletteFxCue(
    PaletteFxCueKind Kind,
    ushort ColorByteIndex = 0,
    PaletteFxPreInstruction PreInstruction = default,
    byte HeatPaletteIndex = 0,
    byte Sound = 0);
