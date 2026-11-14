using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// A kept behaviour as some areas resolve it (#1805): the special-block and special-air readings those areas' own tables
/// give it, with every other reading the behaviour's own. <see cref="RoomBlockBehavior.In"/> holds it in a room of one of
/// those areas.
/// </summary>
/// <param name="Areas">The areas that resolve the behaviour so.</param>
/// <param name="Readings">Every reading of the behaviour in those areas.</param>
public sealed record AreaBlockReadings(IReadOnlyList<AreaId> Areas, RoomBlockReadings Readings);
