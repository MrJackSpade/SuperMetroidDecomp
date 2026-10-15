namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One cue of Crocomire's body programs.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="DustOffset">The horizontal offset of a <see cref="CrocomireBodyCueKind.Dust"/> cue's cloud.</param>
internal readonly record struct CrocomireBodyCue(CrocomireBodyCueKind Kind, int DustOffset);
