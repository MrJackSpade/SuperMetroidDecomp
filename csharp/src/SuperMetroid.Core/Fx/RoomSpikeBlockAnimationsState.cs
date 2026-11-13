using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>Horizontal spikes the room FX record starts, including Crocomire's left wall.</summary>
[RoomLifetime]
public sealed class RoomSpikeBlockAnimationsState
{
    /// <summary>The current room's horizontal-spike animation, which each room builds afresh.</summary>
    private readonly RoomFxBlockAnimationsState animation = new();

    /// <summary>Spawns one room's horizontal spikes, at most once, from the door-selected FX record.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    public RoomSpikeBlockAnimationsState(RoomFxSelection? fx, DoorDefinition? door)
    {
        if (fx?.Select(door) is not { } record) return;
        if (record.BlockAnimations.Contains(BlockAnimation.HorizontalSpikes))
            animation.LoadDefinition(BlockAnimation.HorizontalSpikes);
    }

    /// <summary>Advances the spike animation and selects its authored frame for the next frame boundary.</summary>
    public void Step(ArtworkFrameSelections frames) =>
        animation.Step(frames);
}
