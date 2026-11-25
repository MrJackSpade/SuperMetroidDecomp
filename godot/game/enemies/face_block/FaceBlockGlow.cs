using Engine.Ui;
using Engine.World;

namespace Game.Enemies;

/// <summary>
/// The glow the face blocks of a room share: the room has one, not one a block. The first face block set up in a
/// room makes it. It starts once a block finds that Samus has what wakes the blocks, and from then on every block of
/// the room, and whatever else shares their colours, shows its level: a change of the colours of their pictures. Each
/// level lasts the same number of updates, which starts over, the level kept, whenever a block is set up or wakes.
/// It takes its turn with the room's objects, after the enemies have had theirs, and so stands still through a
/// door's passage.
/// </summary>
public partial class FaceBlockGlow : RoomObject
{
    /// <summary>The name its colours are asked under on what shows them.</summary>
    private const string Source = "face_block_glow";

    private readonly List<IRecoloured> _shownOn = [];
    private IReadOnlyList<ColourChange> _levels = [];
    private int _updates = 1;
    private int _at;
    private int _left;

    /// <summary>Whether a face block has started it.</summary>
    public bool HasStarted { get; private set; }

    /// <summary>
    /// The glow of <paramref name="room"/>: the one already there, or one made now from a block's own levels, each
    /// lasting <paramref name="updates"/> updates. A new one stands on its last level, so that its first step shows
    /// its first.
    /// </summary>
    public static FaceBlockGlow In(Room room, IReadOnlyList<ColourChange> levels, int updates)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(levels);
        if (room.Objects.OfType<FaceBlockGlow>().FirstOrDefault() is { } shared)
            return shared;
        if (levels.Count == 0 || updates < 1)
            throw new InvalidOperationException("A face block's data gives its glow no level, or a level that lasts no update.");
        var glow = new FaceBlockGlow { Name = "FaceBlockGlow", _levels = levels, _updates = updates, _at = levels.Count - 1, _left = updates };
        room.Spawn(glow);
        return glow;
    }

    /// <summary>Adds something that shows the glow's level each time it changes, once it has started.</summary>
    public void Join(IRecoloured visual)
    {
        ArgumentNullException.ThrowIfNull(visual);
        if (!_shownOn.Contains(visual))
            _shownOn.Add(visual);
    }

    /// <summary>Starts the glow if it has not started, and shows its present level on <paramref name="visual"/> at once, and on from there.</summary>
    public void StartOn(IRecoloured visual)
    {
        HasStarted = true;
        Join(visual);
        _levels[_at].ShowOn(visual, Source);
    }

    /// <summary>Shows its present level for a whole level's updates from now, as a block's setting up and its waking do.</summary>
    public void HoldLevelAnew() => _left = _updates;

    /// <summary>Once started it counts its level down, and as that runs out everything that shows it shows the next.</summary>
    protected override void OnStep()
    {
        if (!HasStarted || --_left > 0)
            return;
        _at = (_at + 1) % _levels.Count;
        _left = _updates;
        foreach (IRecoloured visual in _shownOn)
            _levels[_at].ShowOn(visual, Source);
    }
}
