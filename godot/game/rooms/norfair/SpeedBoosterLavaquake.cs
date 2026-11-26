using Engine.World;
using Game.Data;
using Godot;

namespace Game.Rooms.Norfair;

/// <summary>
/// The lava that chases Samus out once she has the Speed Booster. Entering the hall from the Speed Booster's room
/// with it, the lava will rise once its wait is over; once she has run far enough left it waits no longer, and at
/// each of three lines she passes it is brought up to a height and made faster. When she has passed them all the lava has been
/// outrun, and it never comes again.
/// </summary>
public partial class SpeedBoosterLavaquake : RoomSequence
{
    /// <summary>How far across the room Samus must have come, running left, before the lava moves, in pixels.</summary>
    [Export]
    public int StartsMovingAt { get; set; } = 2784;

    /// <summary>How fast the lava rises to begin with, as a number of pixels an update.</summary>
    [Export]
    public double StartSpeed { get; set; } = 0.5;

    /// <summary>
    /// The lines Samus passes, in order: how far across the room each is, the height the lava is brought up to there
    /// if it lies lower, and how fast it rises from then on in 256ths of a pixel an update.
    /// </summary>
    [Export]
    public Godot.Collections.Array<Vector3I> Rises { get; set; } = [new(1835, 447, 176), new(1290, 359, 224), new(580, 256, 224)];

    private bool _moving;
    private bool _idle;
    private int _passed;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        Liquid lava = room.Liquid ?? throw new InvalidOperationException($"The room \"{room.Name}\" has no lava for its lavaquake.");
        int samus = room.Player.Position.X.Whole;
        if (elapsed == 0)
        {
            // Without the Speed Booster there is no chase, and nothing to have outrun.
            (_moving, _passed, _idle) = (false, 0, !room.Player.Inventory.Owns(Equipment.SpeedBooster));
            // The lava is given its speed, toward the height and after the wait its room gives it.
            if (!_idle)
                lava.HeadFor(lava.HighSurface, StartSpeed, lava.MoveDelay);
            return false;
        }

        if (_idle)
            return false;

        if (!_moving)
        {
            if (samus > StartsMovingAt)
                return false;
            _moving = true;
            lava.EndWait();
            return false;
        }

        if (_passed == Rises.Count)
            return true;
        Vector3I rise = Rises[_passed];
        if (samus > rise.X)
            return false;
        if (lava.Surface.Whole > rise.Y)
            lava.PutAt(rise.Y);
        lava.MoveSpeed = rise.Z / 256.0;
        _passed++;
        return false;
    }
}
