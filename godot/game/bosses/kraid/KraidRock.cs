using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A rock of Kraid's: one spat from its mouth, one thrown up as it breaks through the floor, or one shaken loose
/// from the ceiling. It falls faster and faster, drifting across as it goes, and is gone when it meets terrain.
/// Its settings say whether a shot breaks it and what it may leave.
/// </summary>
/// <remarks>The cue "stop" of its timeline brings it to rest.</remarks>
public partial class KraidRock : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The downward speed it gains each update, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.25;

    /// <summary>The speed across to the right it gains each update; nothing for one that falls straight.</summary>
    [Export, DataValue("drift")]
    public double Drift { get; set; } = 0.03125;

    /// <summary>The speed to the right at which its drift starts over, flying left as fast.</summary>
    [Export, DataValue("drift_limit")]
    public double DriftLimit { get; set; } = 1;

    private PixelVelocity _speed;
    private bool _stopped;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        // It moves itself, and meets terrain its own way.
        _speed = Velocity;
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        if (cue == "stop")
            _stopped = true;
        else if (cue == "end")
            Leave(room);
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (_stopped)
            return;
        if (MoveAcrossMeetingTerrain(room, _speed.X) || MoveDownMeetingTerrain(room, _speed.Y))
        {
            Leave(room);
            return;
        }

        Pixels across = _speed.X;
        if (Drift != 0)
        {
            across += Pixels.FromNumber(Drift);
            if (across >= Pixels.FromNumber(DriftLimit))
                across = -Pixels.FromNumber(DriftLimit);
        }

        _speed = new PixelVelocity(across, _speed.Y + Pixels.FromNumber(Gravity));
    }
}
