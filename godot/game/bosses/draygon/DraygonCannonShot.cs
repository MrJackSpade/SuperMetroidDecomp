using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A shot from one of the cannons in Draygon's walls. It gathers at the muzzle until the cue "fire" of its timeline,
/// and then flies the way it was aimed when it appeared, until it has left the chamber.
/// </summary>
public partial class DraygonCannonShot : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How wide and how tall the chamber is, from the room's corner, in pixels: a shot outside it is gone.</summary>
    [Export, DataValue("chamber_size")]
    public int ChamberSize { get; set; } = 512;

    private PixelVelocity _aimed;
    private bool _flying;

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
        // It moves itself, so that it is gone as soon as it has moved out of the chamber.
        _aimed = Velocity;
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        if (cue == "fire")
            _flying = true;
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (!_flying)
            return;
        Position += _aimed.OverOneUpdate;
        int x = Position.X.Whole, y = Position.Y.Whole;
        // A place before the room's corner counts as beyond its far side, as the old game's unsigned test has it.
        if (x < 0 || y < 0 || x >= ChamberSize || y >= ChamberSize)
            Leave(room);
    }
}
