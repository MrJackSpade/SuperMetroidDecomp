using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of the rings Mother Brain breathes. It sits at her mouth a moment, harmless, and then flies straight. It
/// bursts on Samus, on the giant Metroid that shields her, and, with a jolt, where it leaves the fighting floor.
/// </summary>
public partial class MotherBrainRing : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How many updates it sits at her mouth.</summary>
    [Export, DataValue("mouth_updates")]
    public int MouthUpdates { get; set; } = 9;

    /// <summary>How far down the room the fighting floor's top is; a ring above it bursts.</summary>
    [Export, DataValue("top")]
    public int Top { get; set; } = 32;

    /// <summary>How far down the room the fighting floor is; a ring at or below it bursts.</summary>
    [Export, DataValue("floor")]
    public int Floor { get; set; } = 216;

    /// <summary>How far across the view a ring may go before it bursts.</summary>
    [Export, DataValue("view_right")]
    public int ViewRight { get; set; } = 248;

    private MotherBrain? _owner;
    private PixelVelocity _speed;
    private int _held;

    /// <summary>Has the ring sit at <paramref name="owner"/>'s mouth before it flies.</summary>
    public void HeldBy(MotherBrain owner) => _owner = owner;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override bool DeliversHitNow => _held <= 0 && base.DeliversHitNow;

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        _speed = Velocity;
        Velocity = PixelVelocity.Zero;
        _held = MouthUpdates;
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (_held > 0 && --_held > 0)
        {
            if (_owner is { } owner && IsInstanceValid(owner))
                Position = owner.HeadPosition + MotherBrain.MouthOffset;
            return;
        }

        Position += _speed.OverOneUpdate;
        if (room.Enemies.OfType<MotherBrainBaby>().FirstOrDefault(baby => !baby.IsDead && !room.IsLeaving(baby)) is { } shield)
        {
            // With the Metroid spent, her rings are spent too.
            if (shield.IsSpent)
            {
                Leave(room);
                return;
            }

            if (shield.Covers(Area))
            {
                shield.TakeRing(Hit.Damage);
                End(room);
                return;
            }
        }

        // The ring tests for Samus itself, as the cartridge's does: it does not ask whether a hit a moment ago has made
        // her safe, nor whether the room is trying shots against her at all, and lands on her all the same, knocking
        // her away from where it is.
        if (room.Player is Samus samus && Area.Overlaps(samus.Hurtbox.RoomRect))
        {
            bool rightOfIt = samus.Position.X.Whole >= Position.X.Whole;
            samus.TakeHitRegardless(Hit with { Direction = PixelOffset.FromWhole(rightOfIt ? 1 : -1, 0) });
            End(room);
            return;
        }

        int across = Position.X.Whole, down = Position.Y.Whole;
        int inView = across - room.Session.Camera.View.Left;
        if (down < Top || down >= Floor || across < 0 || inView < 0 || inView >= ViewRight)
        {
            room.Shake(2, 10, ShakeWay.Both, ShakeReach.Tiles);
            End(room);
        }
    }
}
