namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One downward-gate trigger (#2484): what its room object draws, the shots that set it off, and the sound of a shot it turns away.</summary>
/// <param name="Change">The change the trigger's room object makes: its shot block drawn once.</param>
/// <param name="SetOffBy">The shots that set the trigger off.</param>
/// <param name="RejectionSound">The sound a shot the trigger turns away makes; null for a trigger that turns shots away silently.</param>
internal sealed record DownwardGateTriggerDefinition(TerrainChange Change, DownwardGateTriggerShots SetOffBy, SoundEffectId? RejectionSound);
