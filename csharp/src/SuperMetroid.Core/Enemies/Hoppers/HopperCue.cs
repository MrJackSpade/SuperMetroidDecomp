namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One cue of the Sidehoppers' and Dessgeegas' animations: what it does, with the values it acts on.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Sound">The enemy-library sound a <see cref="HopperCueKind.PlaySound"/> cue queues.</param>
internal readonly record struct HopperCue(HopperCueKind Kind, SoundEffectId? Sound = null);
