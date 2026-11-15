using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One run of consecutive cartridge records (#1739, #1982): frames of one colour effect, in its frames' order.</summary>
/// <param name="Effect">The effect whose frames show the run's records.</param>
/// <param name="FirstFrame">The effect's frame that shows the run's first record.</param>
/// <param name="FrameCount">The run's records, one for each frame from <paramref name="FirstFrame"/>.</param>
internal readonly record struct ColourEffectRecordRun(ColourEffectId Effect, int FirstFrame, int FrameCount);
