using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Fireflea: a glowing insect that bobs up and down, or circles, lighting its room. Touching Samus hurts her and
/// kills it; and each one that dies, however it dies, makes the room darker.
/// </summary>
/// <remarks>Its timeline is "flying". Nothing in the room stops it.</remarks>
public partial class Fireflea : Enemy
{
    /// <summary>Whether it circles; one that does not bobs up and down.</summary>
    [Export]
    public bool Circles { get; set; }

    /// <summary>How far above and below where it starts it bobs, or how far from its middle it circles, in pixels.</summary>
    [Export]
    public int Radius { get; set; } = 16;

    /// <summary>For one that bobs: how far it moves each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 0.5;

    /// <summary>For one that bobs: whether it starts by going up.</summary>
    [Export]
    public bool StartsUpward { get; set; }

    /// <summary>For one that circles: how far round it goes each update, in 256ths of a turn; negative goes the other way.</summary>
    [Export]
    public double TurnPerUpdate { get; set; } = 1;

    /// <summary>The room effect that is told to "darken" as it dies.</summary>
    [Export]
    public NodePath Darkens { get; set; } = new();

    /// <summary>The effect of the second death the cartridge gives one a blow of Samus's own body has killed.</summary>
    [Export, DataValue("second_death_effect")]
    public string SecondDeathEffect { get; set; } = "";

    private PixelPosition _middle;
    private bool _goingUp;
    private Pixels _angle;
    private bool _killedByHerBody;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _middle = Position;
        _goingUp = StartsUpward;
        if (Circles)
            Position = OnCircle();
        Play("flying");
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (Circles)
        {
            Position = OnCircle();
            _angle += Pixels.FromNumber(TurnPerUpdate);
            return;
        }

        Pixels speed = Pixels.FromNumber(Speed);
        Position += new PixelOffset(Pixels.Zero, _goingUp ? -speed : speed);
        if (Position.Y.Whole < _middle.Y.Whole - Radius || Position.Y.Whole >= _middle.Y.Whole + Radius)
            _goingUp = !_goingUp;
    }

    /// <inheritdoc/>
    protected override void OnTouchedPlayer() => Die();

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        _killedByHerBody = hit.CarriesAny(Game.Data.HitKinds.OfHerBody);
        return true;
    }

    /// <inheritdoc/>
    protected override void OnDied()
    {
        if (!Darkens.IsEmpty && GetNodeOrNull(Darkens) is ITriggerTarget darkness)
            darkness.Triggered("darken");

        // The cartridge's Fireflea runs its death after every touch, whatever the touch did. Where the touch was a
        // blow of her body that had already killed it, that second death finds its place emptied: its explosion, the
        // small one, stands at the room's corner and drops nothing. (It counts a second kill too, which nothing in a
        // Fireflea's room reads.)
        if (_killedByHerBody)
            LeaveEffect(SecondDeathEffect, PixelPosition.Origin);
    }

    /// <summary>Where on its circle it is now, on whole pixels: the cartridge's short product of its radius.</summary>
    private PixelPosition OnCircle() =>
        PixelPosition.FromWhole(_middle.X.Whole, _middle.Y.Whole) + CartridgeAngles.CircleOffset(_angle.Whole, Radius);
}
