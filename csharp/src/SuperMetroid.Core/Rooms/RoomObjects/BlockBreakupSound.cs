namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The sound a breaking block's rock comes with, and how crowded the sound's queue may be for it to join (#2557).</summary>
/// <param name="Effect">The sound.</param>
/// <param name="Priority">How crowded the queue may be for the request to join it.</param>
internal readonly partial record struct BlockBreakupSound(SoundEffectId Effect, SoundRequestPriority Priority);
