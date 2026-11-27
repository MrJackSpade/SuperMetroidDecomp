using Engine.Actors;
using Engine.Core;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Rooms.Maridia;

/// <summary>
/// The ride through Maridia's tube, update for update as the cartridge's. It starts as Samus comes in by one end's
/// door. While it plays she is held still, as she came in: her own update does not run. At the end of each update the
/// room puts her on the tube's middle line and moves her by the ride's speed through the terrain, and then the speed
/// gains unless that would take it past the fastest. The door at the far end takes her as any door does.
/// </summary>
public partial class TubeRide : RoomSequence
{
    /// <summary>How far the ride moves her on its first update, as a number of pixels; negative is upward.</summary>
    [Export]
    public double StartSpeed { get; set; }

    /// <summary>How much the speed gains each update, as a number of pixels; negative is upward.</summary>
    [Export]
    public double Gain { get; set; }

    /// <summary>The fastest the ride goes either way: a gain that would pass it is not made.</summary>
    [Export]
    public double TopSpeed { get; set; } = 14.125;

    /// <summary>The line across the room she is held on, in pixels.</summary>
    [Export]
    public int HoldsAtX { get; set; } = 128;

    /// <summary>The sound asked for a while into the ride, by name; empty for none.</summary>
    [Export]
    public string LateSound { get; set; } = "enemy_stop_elevatube";

    /// <summary>The update of the ride on which that sound is asked for.</summary>
    [Export]
    public int LateSoundOnUpdate { get; set; } = 16;

    private Pixels _speed;

    /// <inheritdoc/>
    public override Stopped Stops => Stopped.Player;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
            _speed = Pixels.FromNumber(StartSpeed);
        Player samus = room.Player;
        samus.Position = new PixelPosition(Pixels.FromWhole(HoldsAtX), samus.Position.Y);
        samus.MoveBodyDown(_speed, samus.BodyMoves with { StoppedBySolids = false });

        // She has moved after the room's own test of its exits: the door she has reached takes her on this update.
        room.MeetExits();
        Pixels faster = _speed + Pixels.FromNumber(Gain), top = Pixels.FromNumber(TopSpeed);
        if (faster >= -top && faster <= top)
            _speed = faster;
        if (LateSound.Length > 0 && elapsed + 1 == LateSoundOnUpdate)
            room.Session.Sounds.Request(LateSound, channel: "enemies", priority: SoundPriority.Normal);
        return false;
    }
}
