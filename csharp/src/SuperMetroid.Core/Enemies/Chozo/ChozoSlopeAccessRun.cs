using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One run of the Wrecked Ship slope access's blocks along its row (#2557), with how it collides opened and closed.</summary>
/// <param name="From">The run's first block, from the object's block.</param>
/// <param name="Length">How many blocks the run covers.</param>
/// <param name="Opened">How the run's blocks collide with the access opened.</param>
/// <param name="Closed">How the run's blocks collide with the access closed.</param>
internal readonly record struct ChozoSlopeAccessRun(Offset16<RoomBlocks> From, int Length, RoomCollisionType Opened, RoomCollisionType Closed);
