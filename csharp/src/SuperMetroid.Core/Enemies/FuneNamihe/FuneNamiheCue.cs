namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>One cue of the Fune's and Namihe's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct FuneNamiheCue(FuneNamiheCueKind Kind);
