using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A gob of Draygon's goop. It flies straight; coming near Samus it sticks to her, and each one stuck halves her
/// speed across again. A stuck one wears off after a while, or is thrown off by anything her body alone can hurt
/// with. A power bomb's blast clears every one it reaches and frees her of them all. Its settings say what a shot
/// does to it in flight and what it may leave.
/// </summary>
public partial class DraygonGoop : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How near Samus it must come across, in pixels, to stick.</summary>
    [Export, DataValue("reach_across")]
    public int ReachAcross { get; set; } = 16;

    /// <summary>How near Samus it must come up or down, in pixels, to stick.</summary>
    [Export, DataValue("reach_down")]
    public int ReachDown { get; set; } = 20;

    /// <summary>The most that can be stuck to her at once; one more does not stick.</summary>
    [Export, DataValue("most_stuck")]
    public int MostStuck { get; set; } = 5;

    /// <summary>How many updates one stays stuck.</summary>
    [Export, DataValue("stuck_updates")]
    public int StuckUpdates { get; set; } = 256;

    /// <summary>How far below her middle the first stuck one sits, in pixels; it may be above.</summary>
    [Export, DataValue("stack_top")]
    public int StackTop { get; set; } = -12;

    /// <summary>How far below the one before each stuck one sits, in pixels.</summary>
    [Export, DataValue("stack_spacing")]
    public int StackSpacing { get; set; } = 4;

    /// <summary>The size of the chamber, in pixels each way; one that flies out of it is gone.</summary>
    [Export, DataValue("chamber_size")]
    public int ChamberSize { get; set; } = 512;

    /// <summary>The timeline of one stuck to her.</summary>
    [Export, DataValue("stuck_timeline")]
    public string StuckTimeline { get; set; } = "";

    private bool _stuck;
    private int _place;
    private int _left;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override bool DeliversHitNow => !_stuck && base.DeliversHitNow;

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (hit.Carries(new HitKind(HitKinds.PowerBomb)))
        {
            Room.Player.SpeedHalvings = 0;
            Leave(Room);
            return HitResult.Destroyed;
        }

        // Shots pass through one that is stuck to her.
        return _stuck ? HitResult.Ignored : base.TakeHit(hit, part);
    }

    /// <inheritdoc/>
    protected override void OnLaunched(Room room) => DataValues.Fill(this, room.Content);

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        Player samus = room.Player;
        if (_stuck)
        {
            if (samus.BodyPowers.Any(power => power.IsActive) || --_left <= 0)
            {
                samus.SpeedHalvings = Math.Max(0, samus.SpeedHalvings - 1);
                Leave(room);
                return;
            }

            Position = PixelPosition.FromWhole(samus.Position.X.Whole, samus.Position.Y.Whole + _place * StackSpacing + StackTop);
            return;
        }

        if (Math.Abs(samus.Position.X.Whole - Position.X.Whole) < ReachAcross && Math.Abs(samus.Position.Y.Whole - Position.Y.Whole) < ReachDown)
        {
            if (samus.SpeedHalvings >= MostStuck)
                return;
            samus.SpeedHalvings++;
            _place = samus.SpeedHalvings;
            _left = StuckUpdates;
            _stuck = true;
            Velocity = PixelVelocity.Zero;
            PlayTimeline(StuckTimeline);
            return;
        }

        if (Position.X.Whole < 0 || Position.X.Whole >= ChamberSize || Position.Y.Whole < 0 || Position.Y.Whole >= ChamberSize)
            Leave(room);
    }
}
