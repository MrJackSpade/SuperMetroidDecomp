namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One cue of the Magdollite's programs, with the operand its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Sound">The sound a <see cref="MagdolliteCueKind.PlaySoundIfOnScreen"/> cue plays.</param>
internal readonly record struct MagdolliteCue(MagdolliteCueKind Kind, SoundEffectId? Sound = null);
