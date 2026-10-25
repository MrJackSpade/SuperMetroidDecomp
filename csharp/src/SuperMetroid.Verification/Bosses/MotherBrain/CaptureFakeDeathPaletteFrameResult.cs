using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The colors, function timer and function of one capture fake death palette frame result.</summary>
/// <param name="Colors">The colors.</param>
/// <param name="FunctionTimer">The function timer.</param>
/// <param name="Function">The function.</param>
internal readonly record struct CaptureFakeDeathPaletteFrameResult(RgbColor[] Colors, ushort FunctionTimer, MotherBrainBodyFunction Function);
