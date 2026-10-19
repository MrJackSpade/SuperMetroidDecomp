namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One block-reaction PLM cue.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="SoundId">The library-2 sound a queue cue plays; zero for the BTS write.</param>
internal readonly record struct BlockReactionPlmCue(BlockReactionPlmCueKind Kind, int SoundId);
