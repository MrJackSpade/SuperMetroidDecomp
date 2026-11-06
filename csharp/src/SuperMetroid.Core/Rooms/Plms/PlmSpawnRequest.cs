namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One room PLM a boss asks to spawn at a block, typed by the family that performs it: a family's spawner accepts only
/// its own <typeparamref name="TPlm"/>.
/// </summary>
/// <typeparam name="TPlm">The family's own set of PLMs it can spawn.</typeparam>
/// <param name="Block">The room block the PLM is created at.</param>
/// <param name="Plm">The PLM spawned there.</param>
public readonly record struct PlmSpawnRequest<TPlm>(Position16<RoomBlocks> Block, TPlm Plm)
    where TPlm : struct, Enum;
