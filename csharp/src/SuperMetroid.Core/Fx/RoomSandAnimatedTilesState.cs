using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Sand-family portion of the animated-tile objects a room's FX record starts as the room loads. Selection comes
/// from the selected FX record's animated tiles, never from a particular room number.
/// Other animated-tile families retain their existing independent owners.
/// </summary>
[RoomLifetime]
public sealed class RoomSandAnimatedTilesState
{
    /// <summary>Only sand-family objects the current room's FX record starts.</summary>
    internal readonly List<RoomFxAnimatedTilesState> objects = [];

    /// <summary>Spawns one room's objects from the door-selected FX record in the record's order.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    public RoomSandAnimatedTilesState(RoomFxSelection? fx, DoorDefinition? door)
    {
        if (fx?.Select(door) is not { } record) return;
        foreach (AnimatedTileObject definition in record.AnimatedTiles)
        {
            if (definition is not (AnimatedTileObject.MaridiaSandCeiling or AnimatedTileObject.MaridiaSandFalling))
                continue;
            var animation = new RoomFxAnimatedTilesState();
            animation.LoadDefinition(definition);
            objects.Add(animation);
        }
    }

    /// <summary>Advances each sand animation and selects its authored frame for the next frame boundary.</summary>
    public void Step(ArtworkFrameSelections frames)
    {
        foreach (var animation in objects) animation.Step(frames);
    }
}
