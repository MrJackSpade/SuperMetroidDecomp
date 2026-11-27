using Engine.Actors;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// What is special about a boss fight, kept apart from the boss. When the player enters its region it wakes the
/// boss; when the boss is destroyed it records the defeat. It asks for no music: the music of a fight and of its
/// end is the boss's own to ask for. It does
/// not touch the room's barriers: they hold themselves shut on a condition about this fight's fact. The boss itself
/// knows nothing of any of this.
/// </summary>
public partial class BossEncounter : RoomObject
{
    /// <summary>The boss.</summary>
    [Export]
    public NodePath Boss { get; set; } = new();

    /// <summary>The width and height of the region that starts the fight, in pixels; its top-left is where the encounter stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(256, 224);

    private string _bossName = "";
    private bool _started;
    private bool _finished;

    /// <summary>Whether the fight has started.</summary>
    public bool HasStarted => _started;

    /// <summary>Whether the fight is over.</summary>
    public bool IsFinished => _finished;

    /// <summary>The region that starts the fight.</summary>
    public PixelRect Area => new(Position.X.Whole, Position.Y.Whole, Size.X, Size.Y);

    /// <inheritdoc/>
    public override bool MustBeRemembered => true;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        if (RememberedAs.Length == 0)
            throw new InvalidOperationException($"The boss encounter \"{Name}\" has no name to be remembered by.");
        _started = false;
        _finished = IsRemembered;
        Node? boss = GetNodeOrNull(Boss);
        if (boss is null)
        {
            // A fight already won has no boss: the boss is present only while the fight's fact is not recorded.
            if (!_finished)
                throw new InvalidOperationException($"The boss encounter \"{Name}\" names no boss that exists.");
            return;
        }

        if (boss is not Enemy enemy)
            throw new InvalidOperationException($"The boss encounter \"{Name}\" names \"{boss.Name}\", which is not an enemy.");
        _bossName = enemy.Name;
        enemy.Sleep();
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        if (_finished)
            return;
        if (!_started)
        {
            if (!Area.Overlaps(Room.Player.BodyRect))
                return;
            _started = true;
            if (GetNodeOrNull(Boss) is Enemy boss)
                boss.Wake();
            return;
        }

        if (!Room.IsGone(_bossName))
            return;
        _finished = true;
        Remember();
        Room.MarkGone(Name);
    }
}
