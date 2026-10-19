namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One cue of the Sidehoppers' and Dessgeegas' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Sound">The library-two sound a <see cref="HopperCueKind.PlaySound"/> cue queues.</param>
internal readonly record struct HopperCue(HopperCueKind Kind, SoundEffectId? Sound = null);
