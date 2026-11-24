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
/// Its timelines are "looking_ahead", "samus_left" and "samus_right". The path of each of their pictures holds
/// "{glow}", which is nothing while the block sleeps and "_glow_0", "_glow_1" and so on, dimmest first, as it glows:
/// its pictures are kept once for each step of the glow, as the glow recolours the whole face.
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

    /// <summary>How many steps its glow has, from dimmest to brightest.</summary>
    [Export, DataValue("glow_levels")]
    public int GlowLevels { get; set; } = 4;

    private bool _looked;

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play("looking_ahead");

    /// <inheritdoc/>
    protected override string PictureOf(TimelineStep step)
    {
        if (!IsAwake)
            return step.Picture.Replace("{glow}", "", StringComparison.Ordinal);
        // The glow brightens through its steps and dims back again, in step for every face block in the room.
        int steps = GlowLevels * 2;
        int now = (int)(Room.Session.Clock.Updates / GlowUpdates % steps);
        int level = now < GlowLevels ? now : steps - 1 - now;
        return step.Picture.Replace("{glow}", $"_glow_{level}", StringComparison.Ordinal);
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
