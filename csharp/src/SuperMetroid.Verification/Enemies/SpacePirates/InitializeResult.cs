using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>The slot and animation playback of one initialize result.</summary>
/// <param name="Slot">The slot.</param>
/// <param name="Playback">The animation playback.</param>
internal readonly record struct InitializeResult(RoomEnemySlot Slot, AnimationPlayback<NinjaSpacePirateSegment> Playback);
