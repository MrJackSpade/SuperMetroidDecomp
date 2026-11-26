using Engine.World;
using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Kago: a hive that sits where it is. Every shot or bomb that lands on it shakes the room and brings out a bug;
/// after enough of them the hive is gone. The bugs are enemies of their own. It is solid to Samus, who is stopped
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

    private int _hits;

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play("slow");

    /// <inheritdoc/>
    protected override void Behave()
    {
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part) => Struck();

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part) => Struck();

    /// <summary>Answers a hit: the room shakes, a bug comes out, and the hive goes once it has taken enough.</summary>
    private void Struck()
    {
        Room.Shake(ShakeStrength, ShakeUpdates, ShakeWayName switch
        {
            "across" => ShakeWay.Across,
            "down" => ShakeWay.Down,
            "both" => ShakeWay.Both,
            _ => throw new InvalidOperationException($"The Kago \"{Name}\" shakes the room \"{ShakeWayName}\", which is no way a room shakes."),
        });
        if (_hits == 0)
            Play("fast");
        _hits++;
        if (_hits > HitCount)
        {
            Room.MarkGone(Name);
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
        Effect place = Effect.HoldingPlace(Position);
        if (!Room.Spawn(place))
            return;
        var bug = scene.Instantiate<KagoBug>();
        bug.Position = Position;
        bug.Hive = this;
        bug.Place = place;
        Room.Spawn(bug);
    }
}
