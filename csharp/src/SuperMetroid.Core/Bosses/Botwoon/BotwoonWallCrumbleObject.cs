using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's wall crumbling as Botwoon breaks out (#1554): the crumble terrain change
/// (<see cref="BotwoonWallMutationDefinitions.Crumble"/>), which opens the room's first two scroll cells as its first
/// row starts to crumble, on the update its setup's hold runs out.
/// </summary>
internal sealed class BotwoonWallCrumbleObject() : TerrainChangeObject(BotwoonWallMutationDefinitions.Crumble)
{
    /// <inheritdoc/>
    private protected override void Begin(RoomObjectStepContext context)
    {
        RoomScrollGrid scrolls = context.Frame.Scrolls ?? throw new InvalidOperationException(
            "Botwoon's wall crumble requires the active room scroll grid.");
        // The game's opening step sets the first two cells of the scroll grid together.
        scrolls.SetLogicalState(new(0, 0), RoomScrollState.Blue);
        scrolls.SetLogicalState(new(1, 0), RoomScrollState.Blue);
    }
}
