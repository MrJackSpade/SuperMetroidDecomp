using Engine.World;
using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Kago: a hive that sits where it is. Every shot or bomb that lands on it shakes the room and brings out a bug;
/// after enough of them the hive is gone. A power bomb's blast only hurts it, and enough of them destroy it. The
/// bugs are enemies of their own. It is solid to Samus, who is stopped
/// by it and can stand on it, and its touch does not hurt.
/// </summary>
/// <remarks>Its timelines are "slow" and, once it has been hit, "fast".</remarks>
public partial class Kago : Enemy
{
    /// <summary>How many hits it survives; the one after that removes it.</summary>
    [Export]
    public int HitCount { get; set; } = 4;

    /// <summary>The scene of the bugs it lets out.</summary>
    [Export]
    public PackedScene? BugScene { get; set; }

    /// <summary>How hard each hit shakes the room.</summary>
    [Export, DataValue("shake_strength")]
    public int ShakeStrength { get; set; } = 1;

    /// <summary>How many updates each hit shakes the room for.</summary>
    [Export, DataValue("shake_updates")]
    public int ShakeUpdates { get; set; } = 16;

    /// <summary>The way each hit shakes the room: "across", "down" or "both".</summary>
    [Export, DataValue("shake_way")]
    public string ShakeWayName { get; set; } = "across";

    /// <summary>What of the room each hit shakes: "tiles", "tiles_and_backdrops", "everything" or "backdrops_and_things".</summary>
    [Export, DataValue("shake_reach")]
    public string ShakeReachName { get; set; } = "tiles";

    private int _hits;

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play("slow");

    /// <inheritdoc/>
    protected override void Behave()
    {
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part)
    {
        if (Answers(hit))
            Struck(dying: false);
    }

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part)
    {
        if (Answers(hit))
            Struck(dying: false);
    }

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        // A shot that empties its health still shakes the room and lets out a bug, as the cartridge's does.
        if (Answers(hit))
            Struck(dying: true);
        return true;
    }

    /// <summary>
    /// Whether the hive answers <paramref name="hit"/> by shaking the room and letting out a bug. Shots and bombs it
    /// answers; a power bomb's blast only hurts it, as it hurts any enemy, and is not counted among its hits.
    /// </summary>
    private static bool Answers(Hit hit) => !hit.Carries(new HitKind(HitKinds.PowerBomb));

    /// <summary>
    /// Answers a hit: the room shakes, a bug comes out, and the hive goes once it has taken enough, counted among the
    /// room's killed. One the hit is killing already dies of it instead.
    /// </summary>
    private void Struck(bool dying)
    {
        Room.Shake(ShakeStrength, ShakeUpdates, ShakeWayName switch
        {
            "across" => ShakeWay.Across,
            "down" => ShakeWay.Down,
            "both" => ShakeWay.Both,
            _ => throw new InvalidOperationException($"The Kago \"{Name}\" shakes the room \"{ShakeWayName}\", which is no way a room shakes."),
        }, ShakeReachName switch
        {
            "tiles" => ShakeReach.Tiles,
            "tiles_and_backdrops" => ShakeReach.TilesAndBackdrops,
            "everything" => ShakeReach.Everything,
            "backdrops_and_things" => ShakeReach.BackdropsAndThings,
            _ => throw new InvalidOperationException($"The Kago \"{Name}\" shakes \"{ShakeReachName}\" of the room, which is no reach a shake has."),
        });
        if (_hits == 0)
            Play("fast");
        _hits++;
        if (_hits > HitCount && !dying)
        {
            Room.MarkGone(Name);
            CountAsKilled();
            Vanish();
        }

        LetOutBug();
    }

    /// <summary>
    /// Lets out a bug, the last hit's as well. On the cartridge a bug is one of the enemies' shots: it holds one of the
    /// room's places while it lives, and with none free no bug comes out.
    /// </summary>
    private void LetOutBug()
    {
        PackedScene scene = BugScene ?? throw new InvalidOperationException($"The Kago \"{Name}\" names no scene for its bugs.");
        var bug = scene.Instantiate<KagoBug>();
        bug.Position = Position;
        Effect place = Effect.HoldingPlaceFor(bug);
        if (!Room.Spawn(place))
        {
            bug.Free();
            return;
        }

        bug.Hive = this;
        bug.Place = place;

        // A bug sets no speed of its own as it comes out: it has the speed its place's last holder left there, in
        // this room or the one before, until its first jump gives it one. It does not move by it; it is what the
        // bug leaves in its turn if it goes before it jumps.
        bug.Velocity = Room.FoundInPlace(place).Speed;
        Room.Spawn(bug);
    }
}
