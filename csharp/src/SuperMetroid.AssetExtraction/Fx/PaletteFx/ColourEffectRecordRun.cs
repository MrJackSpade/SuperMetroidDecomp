using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One run of consecutive cartridge records (#1739): frames of one colour effect animation, in its frames' order.</summary>
/// <param name="Animation">The animation whose frames show the run's records.</param>
/// <param name="FirstFrame">The animation's frame that shows the run's first record.</param>
/// <param name="FrameCount">The run's records, one for each frame from <paramref name="FirstFrame"/>.</param>
internal readonly record struct ColourEffectRecordRun(ColourEffectAnimation Animation, int FirstFrame, int FrameCount);
