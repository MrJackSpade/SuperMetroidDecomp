using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Ripper: it flies straight across at a steady speed and turns back at walls. The plain Ripper, the Jet Ripper and
/// the Grapple Ripper are all this scene with their own data files. One placed with patrol bounds turns back at them
/// too, and one whose data says so can be latched onto by the grapple beam.
/// </summary>
/// <remarks>Its timelines are "left" and "right", and "frozen_left" and "frozen_right" where it has them.</remarks>
public partial class Ripper : Enemy, ILatchable
{
    /// <summary>How far it flies each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 0.5;

    /// <summary>The column left of which it turns back; with <see cref="PatrolRight"/> both zero, it has no bounds.</summary>
    [Export]
    public int PatrolLeft { get; set; }

    /// <summary>The column at or right of which it turns back.</summary>
    [Export]
    public int PatrolRight { get; set; }

    /// <summary>Whether the grapple beam can latch onto it.</summary>
    [Export, DataValue("can_be_latched")]
    public bool Latchable { get; set; }

    /// <inheritdoc/>
    public bool CanBeLatched => Latchable && !IsDead;

    /// <inheritdoc/>
    public PixelPosition LatchPoint => Position;

    /// <inheritdoc/>
    public void Latched(PixelPosition at)
    {
    }

    /// <inheritdoc/>
    public void Unlatched()
    {
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom() => ShowHeading();

    /// <inheritdoc/>
    protected override void Behave()
    {
        bool turns = MoveAcrossBy(Facing.Toward(Pixels.FromNumber(Speed)));
        // A patrol bound, unlike a wall, does not put it back flush: it keeps what it overshot by.
        if (!turns && (PatrolLeft != 0 || PatrolRight != 0))
            turns = Facing == Facing.Left ? Position.X.Whole < PatrolLeft : Position.X.Whole >= PatrolRight;
        if (!turns)
            return;
        Facing = Facing.Opposite();
        ShowHeading();
    }

    /// <summary>Where its flight was when a frozen picture took its place; none while it shows its flight.</summary>
    private TimelineMark? _beforeFrozen;

    /// <inheritdoc/>
    protected override void OnStatusBegan(string status)
    {
        string frozen = Facing == Facing.Left ? "frozen_left" : "frozen_right";
        if (status != Frozen || !Timelines.Has(frozen) || Playing == frozen)
            return;
        _beforeFrozen = MarkTimeline();
        Play(frozen);
    }

    /// <inheritdoc/>
    protected override void OnStatusEnded(string status)
    {
        // Thawed, its flight goes on from where it was; one with no frozen picture never left it.
        if (_beforeFrozen is not { } mark)
            return;
        _beforeFrozen = null;
        ResumeTimeline(mark);
    }

    /// <summary>Shows the timeline of the way it is flying.</summary>
    private void ShowHeading() => Play(Facing == Facing.Left ? "left" : "right", restart: true);
}
