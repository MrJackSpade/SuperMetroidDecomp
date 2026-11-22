using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// A kept behaviour as some areas resolve it (#1805, #2556): the special-block and special-air reactions it has in those
/// areas, with every other reading the behaviour's own. <see cref="RoomBlockBehavior.In"/> holds it in a room of one of
/// those areas.
/// </summary>
/// <param name="Areas">The areas that resolve the behaviour so.</param>
/// <param name="Unresolved">The behaviour the areas resolve, holding neither the special-block nor the special-air reading.</param>
/// <param name="Special">What special blocks make of it in those areas.</param>
/// <param name="SpecialAir">What special air makes of it in those areas.</param>
public sealed partial record AreaBlockReadings(IReadOnlyList<AreaId> Areas, RoomBlockReadings Unresolved, SpecialBlockReaction Special, SpecialAirReaction SpecialAir)
{
    /// <summary>Every reading of the behaviour in those areas, which hold its meaning to special blocks and special air.</summary>
    public RoomBlockReadings Readings => Unresolved with { Special = Special, SpecialAir = SpecialAir, ResolvedByArea = false };
}
