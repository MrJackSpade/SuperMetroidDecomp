using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A bullet from one of the turrets in Mother Brain's room. It flies straight and is gone without a trace once its
/// middle is in a block. It stops Samus's shots only on every other update, from its first. Shot, it becomes its smoke
/// where it is: the smoke holds still, stops no more shots, and harms Samus until it touches her or plays out. Touching
/// Samus unshot, it becomes its smoke too but lives on: it moves once more, then holds still, harms her again whenever
/// she can be hurt, starting its smoke over each time, and stops shots or not as it did on its last move.
/// </summary>
/// <remarks>Its data names "smoke_timeline", whose cue "end" takes it away.</remarks>
public partial class MotherBrainTurretBullet : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The timeline of the smoke it becomes when it is shot or touches Samus.</summary>
    [Export, DataValue("smoke_timeline")]
    public string SmokeTimeline { get; set; } = "";

    private PixelVelocity _speed;
    private bool _shotDown;
    private bool _touchedSamus;
    private int? _lastMovedAt;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    /// <remarks>
    /// The old bullet switches whether it stops shots each time it moves, before her shots meet it in that update: from
    /// stopping none it stops them after its first move, not after its second, and so on. Held still, it keeps what its
    /// last move left.
    /// </remarks>
    public override bool IsHitBy(HitSide side) => !_shotDown && (_lastMovedAt ?? Age) % 2 == 0 && base.IsHitBy(side);

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        _shotDown = true;
        PlayTimeline(SmokeTimeline);
        return HitResult.Destroyed;
    }

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        // It moves itself, so that it meets a block by its middle alone.
        _speed = Velocity;
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (_shotDown || _lastMovedAt is not null)
            return;
        Position += _speed.OverOneUpdate;
        // The old bullet moves once more on the update after it touches Samus, before its smoke stops it.
        if (_touchedSamus)
            _lastMovedAt = Age;
        if (room.Terrain.IsSolidIn(new PixelRect(Position.X.Whole, Position.Y.Whole, 1, 1), SlopeTreatment.Wall))
            Leave(room);
    }

    /// <inheritdoc/>
    protected override void OnMetTarget(Room room, IHittable target, HitResult result)
    {
        if (result == HitResult.Ignored)
            return;

        // Its smoke from a shot is gone the moment it touches Samus, leaving nothing.
        if (_shotDown)
        {
            Leave(room);
            return;
        }

        // Unshot, it lives on as its smoke, which starts over each time it touches her.
        _touchedSamus = true;
        PlayTimeline(SmokeTimeline);
    }

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        if (cue != "end")
            throw new InvalidOperationException($"A turret's bullet does not know the cue \"{cue}\".");
        Leave(room);
    }
}
