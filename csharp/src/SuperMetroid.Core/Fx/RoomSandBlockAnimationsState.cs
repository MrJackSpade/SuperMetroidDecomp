using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Sand-family portion of the block animations a room's effect starts as the room loads. Selection comes
/// from the selected room effect's block animations, never from a particular room number.
/// Other block animation families retain their existing independent owners.
/// </summary>
[RoomLifetime]
public sealed class RoomSandBlockAnimationsState
{
    /// <summary>Only sand-family objects the current room's effect starts.</summary>
    internal readonly List<RoomFxBlockAnimationsState> objects = [];

    /// <summary>Spawns one room's objects from the door-selected room effect in the effect's order.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the room effect.</param>
    public RoomSandBlockAnimationsState(RoomFxSelection? fx, DoorDefinition? door)
    {
        if (fx?.Select(door) is not { } effect) return;
        foreach (BlockAnimation definition in effect.BlockAnimations)
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
