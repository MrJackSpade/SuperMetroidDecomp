namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One room PLM a boss asks to spawn at a block, typed by the family that performs it: a family's spawner accepts only
/// its own <typeparamref name="TObject"/>.
/// </summary>
/// <typeparam name="TObject">The family's own set of PLMs it can spawn.</typeparam>
/// <param name="Block">The room block the PLM is created at.</param>
/// <param name="RoomObject">The PLM spawned there.</param>
public readonly record struct RoomObjectSpawnRequest<TObject>(Position16<RoomBlocks> Block, TObject RoomObject)
    where TObject : struct, Enum;
