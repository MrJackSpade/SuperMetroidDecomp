using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>Horizontal spikes the room FX record starts, including Crocomire's left wall.</summary>
[RoomLifetime]
public sealed class RoomSpikeAnimatedTilesState
{
    /// <summary>The current room's horizontal-spike animation, which each room builds afresh.</summary>
    private readonly RoomFxAnimatedTilesState animation = new();

    /// <summary>Spawns one room's horizontal spikes, at most once, from the door-selected FX record.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    public RoomSpikeAnimatedTilesState(RoomFxSelection? fx, DoorDefinition? door)
    {
        if (fx?.Select(door) is not { } record) return;
        if (record.AnimatedTiles.Contains(AnimatedTileObject.HorizontalSpikes))
            animation.LoadDefinition(AnimatedTileObject.HorizontalSpikes);
    }

    /// <summary>Advances the spike animation and selects its authored frame for the next frame boundary.</summary>
    public void Step(ArtworkFrameSelections frames) =>
        animation.Step(frames);
}
