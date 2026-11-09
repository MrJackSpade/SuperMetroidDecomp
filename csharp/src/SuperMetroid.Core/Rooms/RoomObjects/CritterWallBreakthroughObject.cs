using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The critters' wall breaking open where a shot hit it (#1554,
/// <see cref="ScriptedTerrainChangeDefinitions.CritterWallBreakthrough"/>): on the update after its last image it
/// marks the critters escaped, as it leaves the room.
/// </summary>
internal sealed class CritterWallBreakthroughObject() : TerrainChangeObject(ScriptedTerrainChangeDefinitions.CritterWallBreakthrough)
{
    /// <inheritdoc/>
    private protected override void End(RoomObjectStepContext context) =>
        (context.RoomObjects.Events ?? throw new InvalidOperationException("Animal rescue requires the room event owner."))
            .Set(StoryEvent.CrittersEscaped);
}
