using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A face block: a carved face in the rock that does nothing until Samus has the Morph Ball. From then on it glows,
/// each with a glow of its own, and turns its eyes to her the first
/// time she comes near on the side it watches. It is solid to her, a block wide and high; her shots and bombs
/// take no notice of it and fly on, her grapple's line is stopped by it, and nothing harms it.
/// </summary>
/// <remarks>
/// Its timelines are "looking_ahead", "samus_left" and "samus_right". Its data lists the glow's levels under "glow",
/// in the order the glow goes through them and round again: each a change of the colours of its pictures, and of
/// whatever else shares them, written place by place as any change of colours is. "glow_updates" is how long each
/// level lasts.
/// </remarks>
public partial class FaceBlock : Enemy
{
    /// <summary>How near Samus must come, in pixels across and down, for it to look at her.</summary>
    [Export]
    public int TriggerDistance { get; set; } = 64;

    /// <summary>The equipment Samus must have for it to wake.</summary>
    [Export, DataValue("wakes_with")]
    public string WakesWith { get; set; } = "";

    /// <summary>How many updates each level of its glow lasts.</summary>
    [Export, DataValue("glow_updates")]
    public int GlowUpdates { get; set; } = 16;

    /// <summary>The name its glow's colours are asked under on its visual.</summary>
    private const string GlowColours = "face_block_glow";

    private ColourChange[] _glowLevels = [];

    /// <summary>Whether it glows yet, the level of its glow it shows, and the updates that level has left.</summary>
    private bool _glowing;
    private int _glowAt;
    private int _glowLeft;
    private bool _looked;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _glowLevels = [.. data.List("glow").Select(ColourChange.Read)];
        if (_glowLevels.Length == 0)
            throw data.Problem("must give at least one level of its glow");
    }

    /// <summary>Set up, it glows from its first level if Samus already has what wakes it.</summary>
    protected override void OnEnterRoom()
    {
        Play("looking_ahead");
        (_looked, _glowing) = (false, false);
        if (IsAwake)
            Glow();
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (!IsAwake)
            return;
        Glow();
        if (_looked)
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

    /// <summary>
    /// Its glow, its own: it begins on its first level, and each level lasts its updates and gives way to the next,
    /// round and round. The blocks of a room that wake together glow together only because each counts alike.
    /// </summary>
    private void Glow()
    {
        if (Visual is not { } visual)
            return;
        if (!_glowing)
            (_glowing, _glowAt, _glowLeft) = (true, 0, GlowUpdates);
        else if (--_glowLeft > 0)
            return;
        else
            (_glowAt, _glowLeft) = ((_glowAt + 1) % _glowLevels.Length, GlowUpdates);
        _glowLevels[_glowAt].ShowOn(visual, GlowColours);
    }

    /// <summary>Whether Samus has what wakes it.</summary>
    private bool IsAwake => Room.Player.Inventory.Owns(WakesWith);
}
