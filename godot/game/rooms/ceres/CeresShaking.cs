using Engine.World;
using Game.Data;
using Godot;

namespace Game.Rooms.Ceres;

/// <summary>
/// Ceres shaking itself apart: once its self-destruct has begun, the room is never still. Each time a shake dies
/// away another starts, a light one, or now and then a short strong one, corner to corner.
/// </summary>
public partial class CeresShaking : RoomSequence
{
    /// <summary>What of the room its shakes throw about.</summary>
    [Export]
    public ShakeReach Reach { get; set; } = ShakeReach.Everything;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (!room.Session.Progress.Has(StoryEvents.CeresSelfDestructStarted) || room.ShakeStrength > 0)
            return false;
        bool strong = room.Session.Random.Last.Part(0, 12) < 128;
        room.Shake(strong ? 3 : 1, strong ? 4 : 2, ShakeWay.Both, Reach);
        return false;
    }
}
