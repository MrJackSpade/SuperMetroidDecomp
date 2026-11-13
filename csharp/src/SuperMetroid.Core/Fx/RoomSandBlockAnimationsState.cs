using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Sand-family portion of the block animations a room's FX record starts as the room loads. Selection comes
/// from the selected FX record's block animations, never from a particular room number.
/// Other block animation families retain their existing independent owners.
/// </summary>
[RoomLifetime]
public sealed class RoomSandBlockAnimationsState
{
    /// <summary>Only sand-family objects the current room's FX record starts.</summary>
    internal readonly List<RoomFxBlockAnimationsState> objects = [];

    /// <summary>Spawns one room's objects from the door-selected FX record in the record's order.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    public RoomSandBlockAnimationsState(RoomFxSelection? fx, DoorDefinition? door)
    {
        if (fx?.Select(door) is not { } record) return;
        foreach (BlockAnimation definition in record.BlockAnimations)
        {
            if (definition is not (BlockAnimation.MaridiaSandCeiling or BlockAnimation.MaridiaSandFalling))
                continue;
            var animation = new RoomFxBlockAnimationsState();
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
