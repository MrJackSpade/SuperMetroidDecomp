using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>One record run of a room palette-effect program and how a failure names it.</summary>
/// <param name="Owner">How a failure names the run.</param>
/// <param name="Frames">The records the run's colours go to.</param>
internal sealed record RoomPaletteFxRun(string Owner, PaletteFxColorFrames Frames);
