using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>Horizontal spikes the room's effect starts, including Crocomire's left wall.</summary>
[RoomLifetime]
public sealed class RoomSpikeBlockAnimationsState
{
    /// <summary>The animations this owner plays.</summary>
    private static readonly BlockAnimation[] Played = [BlockAnimation.HorizontalSpikes];

    /// <summary>The current room's horizontal-spike animations, which each room builds afresh.</summary>
    internal readonly List<BlockAnimationPlayer> objects;

    /// <summary>Spawns one room's horizontal spikes from the door-selected room effect.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the room effect.</param>
    public RoomSpikeBlockAnimationsState(RoomFxSelection? fx, DoorDefinition? door) =>
        objects = RoomEffectBlockAnimations.Start(fx, door, Played);

    /// <summary>Advances the spike animation and selects its authored frame for the next frame boundary.</summary>
    public void Step(ArtworkFrameSelections frames)
    {
        foreach (BlockAnimationPlayer animation in objects) animation.Step(frames);
    }
}
