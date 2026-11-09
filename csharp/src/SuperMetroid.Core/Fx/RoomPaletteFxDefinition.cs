using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>How one colour effect starts and the program it starts (#1554).</summary>
/// <param name="Start">How the effect starts.</param>
/// <param name="InitialProgram">The program the effect starts.</param>
internal readonly record struct RoomPaletteFxDefinition(
    ColourEffectStart Start,
    PaletteFxProgram InitialProgram);
