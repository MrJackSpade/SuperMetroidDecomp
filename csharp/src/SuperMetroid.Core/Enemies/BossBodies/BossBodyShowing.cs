namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>A boss body the room shows (#2194, #2560): its layer and where its boss places it.</summary>
/// <param name="Layer">The body's layer as the room shows it now.</param>
/// <param name="Placement">Where the boss places its body, or null for a body no boss places.</param>
internal readonly partial record struct BossBodyShowing(BossBodyLayer Layer, Position16<RoomPixels>? Placement);
