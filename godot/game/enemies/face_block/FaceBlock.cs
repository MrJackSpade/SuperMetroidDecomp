using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A face block: a carved face in the rock that does nothing until Samus has the Morph Ball. From then on it glows,
/// and turns its eyes to her the first time she comes near on the side it watches. Nothing touches it.
/// </summary>
/// <remarks>
/// Its timelines are "looking_ahead", "samus_left" and "samus_right". Its data lists the tints of its glow, dimmest
/// first, under "glow_tints".
/// </remarks>
public partial class FaceBlock : Enemy
{
    /// <summary>How near Samus must come, in pixels across and down, for it to look at her.</summary>
    [Export]
    public int TriggerDistance { get; set; } = 64;

    /// <summary>The equipment Samus must have for it to wake.</summary>
    [Export, DataValue("wakes_with")]
    public string WakesWith { get; set; } = "";

    /// <summary>How many updates each step of its glow lasts.</summary>
    [Export, DataValue("glow_updates")]
    public int GlowUpdates { get; set; } = 16;

    private readonly List<Color> _tints = [];
    private bool _looked;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _tints.Clear();
        _tints.AddRange(data.List("glow_tints").Select(tint => new Color(tint.AsText())));
        if (_tints.Count == 0)
            throw data.Problem("must list at least one glow tint");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play("looking_ahead");

    /// <inheritdoc/>
    protected override Color RestingTint
    {
        get
        {
            if (!IsAwake)
                return Colors.White;
            // The glow brightens through its tints and dims back again, in step for every face block in the room.
            int steps = _tints.Count * 2;
            int step = (int)(Room.Session.Clock.Updates / GlowUpdates % steps);
            return _tints[step < _tints.Count ? step : steps - 1 - step];
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (!IsAwake || _looked)
            return;
        if (Math.Abs(Target.Y.Whole - Position.Y.Whole) >= TriggerDistance)
            return;
        int across = Target.X.Whole - Position.X.Whole;
        if (Math.Abs(across) >= TriggerDistance)
            return;
        Facing side = across < 0 ? Facing.Left : Facing.Right;
        if (side != Facing)
            return;
        Play(side == Facing.Left ? "samus_left" : "samus_right", restart: true);
        _looked = true;
    }

    /// <summary>Whether Samus has what wakes it.</summary>
    private bool IsAwake => Room.Player.Inventory.Owns(WakesWith);
}
